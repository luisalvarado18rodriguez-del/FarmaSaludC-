using FarmaSaludMVC.Data;
using FarmaSaludMVC.Models;
using FarmaSaludMVC.security;
using FarmaSaludMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FarmaSaludMVC.Controllers
{

    [AllowAnonymous]
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
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. Buscamos al usuario por su email
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario != null)
            {
                // 2. Verificamos si la cuenta está activa (Baneo)
                if (!usuario.Activo)
                {
                    // CORRECCIÓN: No existe 'model', usamos ModelState directamente
                    ModelState.AddModelError(string.Empty, "Esta cuenta ha sido inhabilitada por el administrador.");
                    return View();
                }

                // 3. VALIDACIÓN TÉCNICA (Inyectando la sal antigua)
                bool esValido = security.SecurityHelper.VerificarPassword(password, usuario.Password);

                if (esValido)
                {
                    // 4. Creación de Claims y Cookies
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("UsuarioId", usuario.Id.ToString())
            };

                    var claimsIdentity = new ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity)
                    );

                    return RedirectToAction("Catalogo", "Medicamento");
                }
            }

            // 5. Mensaje genérico de error de credenciales
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View();
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