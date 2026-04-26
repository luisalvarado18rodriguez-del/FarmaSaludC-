using FarmaSaludMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FarmaSaludMVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // --- ÚNICO MÉTODO ERROR CORREGIDO ---
        [Route("Home/Error/{id?}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? id)
        {
            // Si el error es por falta de permisos (403) o no logueado (401)
            if (id == 403 || id == 401)
            {
                ViewBag.Mensaje = "No tienes permisos para acceder a esta sección de FarmaSalud.";
                ViewBag.Icono = "bi-shield-lock-fill";
                return View("AccesoDenegado");
            }

            // Si la página no existe
            if (id == 404)
            {
                ViewBag.Mensaje = "La página que buscas no se encuentra disponible.";
                ViewBag.Icono = "bi-exclamation-circle";
                return View("NoEncontrado");
            }

            // Para cualquier otro error (como el 500) o si id es null
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}