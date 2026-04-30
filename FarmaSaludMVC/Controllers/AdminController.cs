using FarmaSaludMVC.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace FarmaSaludMVC.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly IReservaService _reservaService;

        public AdminController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var reservas = await _reservaService.GetAllReservasActivasAsync();
            return View(reservas);
        }

        [HttpPost]
        public async Task<IActionResult> EjecutarLimpieza()
        {
            int procesados = await _reservaService.ProcesarCancelacionesAutomaticasAsync();
            TempData["Mensaje"] = $"Se procesaron y cancelaron {procesados} reservas expiradas.";
            return RedirectToAction("Dashboard");
        }
        public IActionResult VerReceta(string nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo)) return NotFound();

            // Construimos la ruta física: wwwroot/uploads/recetas/nombre.jpg
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "recetas", nombreArchivo);

            if (!System.IO.File.Exists(path)) return NotFound("El archivo no existe en el servidor.");

            var fileBytes = System.IO.File.ReadAllBytes(path);

            // Determinamos el tipo de contenido (puedes ajustarlo si usas PDF)
            return File(fileBytes, "image/jpeg");
        }
        [HttpPost]
        public async Task<IActionResult> Finalizar(int id)
        {
            var exito = await _reservaService.FinalizarReservaAsync(id);

            if (exito)
            {
                TempData["Mensaje"] = "La reserva ha sido marcada como ENTREGADA y TERMINADA.";
            }
            else
            {
                TempData["Error"] = "No se pudo actualizar el estado de la reserva.";
            }

            return RedirectToAction("Dashboard");
        }
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DetalleReserva(int id)
        {
            // Buscamos la reserva incluyendo al usuario, los detalles y el medicamento de cada detalle
            var reserva = await _reservaService.GetReservaDetalladaAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]        
        public async Task<IActionResult> Cancelar(int id)
        {
            var exito = await _reservaService.CancelarReservaManualAsync(id);
            if (exito) TempData["Mensaje"] = "Reserva cancelada y stock restablecido.";
            else TempData["Error"] = "No se pudo cancelar la reserva.";

            return RedirectToAction("Dashboard");
        }
    }
}