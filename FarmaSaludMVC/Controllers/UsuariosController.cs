using FarmaSaludMVC.Data;
using FarmaSaludMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaSaludMVC.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string buscar)
        {
            
            var usuarios = _context.Usuarios
                .Where(u => u.Rol == "Admin" || u.Rol == "SuperAdmin");

            if (!string.IsNullOrEmpty(buscar))
            {
                usuarios = usuarios.Where(s => s.Email.Contains(buscar));
            }

            ViewData["FiltroActual"] = buscar;

            return View(await usuarios.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            // Preparamos la lista de roles
            ViewBag.Roles = new List<SelectListItem>
    {
        new SelectListItem { Value = "Admin", Text = "Administrador" }
    };
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Password,Rol,Activo")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                
                usuario.Password = security.SecurityHelper.EncriptarPassword(usuario.Password);

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            // 1. Cargamos los Roles (asegúrate de que los Value coincidan con los de tu DB: "Admin", "Cliente", etc.)
            var roles = new List<SelectListItem>
    {
        new SelectListItem { Value = "Admin", Text = "Administrador" },
    };
            // El cuarto parámetro marca el valor seleccionado automáticamente
            ViewBag.Roles = new SelectList(roles, "Value", "Text", usuario.Rol);

            // 2. Cargamos los Estados para el Baneo
            var estados = new List<SelectListItem>
    {
        new SelectListItem { Value = "true", Text = "Cuenta Activa" },
        new SelectListItem { Value = "false", Text = "Cuenta Inhabilitada (Baneo)" }
    };
            ViewBag.Estados = new SelectList(estados, "Value", "Text", usuario.Activo.ToString().ToLower());

            return View(usuario);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,Activo,Rol")] Usuario usuario)
        {
            if (id != usuario.Id) return NotFound();

            // 1. Buscamos la data original de la DB (Clave para no perder datos)
            var usuarioOriginal = await _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuarioOriginal == null) return NotFound();

            // 2. Sincronizamos datos que no vienen en el form o son automáticos
            usuario.Password = usuarioOriginal.Password;

            // Si el rol venía nulo o vacío por ser cliente, lo recuperamos de la DB
            if (string.IsNullOrEmpty(usuario.Rol))
            {
                usuario.Rol = usuarioOriginal.Rol;
            }

            // 3. Protección de SuperAdmin
            if (usuarioOriginal.Rol == "SuperAdmin")
            {
                usuario.Activo = true;
                usuario.Rol = "SuperAdmin";
            }

            // 4. Limpiamos errores de validación que no nos interesan (como el Password que no enviamos)
            ModelState.Remove("Password");
            ModelState.Remove("Rol");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();

                    return usuario.Rol == "Cliente"
                        ? RedirectToAction(nameof(Clientes))
                        : RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id)) return NotFound();
                    else throw;
                }
            }

            // Si llega aquí es porque falló el ModelState. Recargamos combos para que no se vea vacío.
            PrepararCombos(usuario);
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Clientes(string buscar)
        {
            // Filtramos SOLO Clientes
            var clientes = _context.Usuarios.Where(u => u.Rol == "Cliente");

            if (!string.IsNullOrEmpty(buscar))
            {
                clientes = clientes.Where(s => s.Email.Contains(buscar));
            }

            ViewData["FiltroActual"] = buscar;
            return View(await clientes.ToListAsync());
        }

        private void PrepararCombos(Usuario usuario)
        {
            // Llenamos la lista de Roles (solo Admin para este panel)
            var roles = new List<SelectListItem>
    {
        new SelectListItem { Value = "Admin", Text = "Administrador" }
    };
            ViewBag.Roles = new SelectList(roles, "Value", "Text", usuario.Rol);

            // Llenamos la lista de Estados (Activo/Baneado)
            var estados = new List<SelectListItem>
    {
        new SelectListItem { Value = "true", Text = "Cuenta Activa" },
        new SelectListItem { Value = "false", Text = "Cuenta Inhabilitada (Baneo)" }
    };
            // El cuarto parámetro asegura que el combo se quede en la opción que ya tiene el usuario
            ViewBag.Estados = new SelectList(estados, "Value", "Text", usuario.Activo.ToString().ToLower());
        }

    }
}
