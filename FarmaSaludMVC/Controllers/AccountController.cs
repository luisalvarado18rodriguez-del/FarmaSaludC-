using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmaSaludMVC.Data;
using FarmaSaludMVC.Models;
using FarmaSaludMVC.ViewModels;
using FarmaSaludMVC.security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace FarmaSaludMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register() => View();

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistroVM model)
        {
            if (ModelState.IsValid)
            {
                // 1. Crear el objeto Usuario con password ENCRIPTADO
                var nuevoUsuario = new Usuario
                {
                    Email = model.Email,
                    Password = SecurityHelper.EncriptarPassword(model.Password),
                    Rol = "Cliente",
                    Activo = true
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

             
                var nuevoCliente = new Cliente
                {
                    DNI = model.DNI,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Email = model.Email,
                    UsuarioId = nuevoUsuario.Id
                };

                _context.Clientes.Add(nuevoCliente);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Buscamos al usuario solo por Email
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // 2. Si existe, verificamos el Password usando BCrypt
            if (usuario != null && SecurityHelper.VerificarPassword(model.Password, usuario.Password))
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.UsuarioId == usuario.Id);

                // --- CONFIGURACIÓN DE CLAIMS PARA ROLES ---
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol), // Esto permite que @User.IsInRole funcione
                    new Claim("UsuarioId", usuario.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

               
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                HttpContext.Session.SetString("ClienteNombre", cliente?.Nombre ?? "Usuario");

                return RedirectToAction("Catalogo", "Medicamento");
            }

            ModelState.AddModelError("", "Correo o contraseña no válidos.");
            return View(model);
        }

        // POST: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}