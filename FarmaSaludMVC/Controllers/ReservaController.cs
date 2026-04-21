using FarmaSaludMVC.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSaludMVC.Controllers
{
    public class ReservaController : Controller
    {
        private readonly IReservaService _reservaService;
        private readonly ICarritoService _carritoService;

        public ReservaController(IReservaService reservaService, ICarritoService carritoService)
        {
            _reservaService = reservaService;
            _carritoService = carritoService;
        }

        [HttpPost]
        public async Task<IActionResult> Confirmar(List<IFormFile> archivosRecetas)
        {
            var carrito = _carritoService.ObtenerCarrito();
            if (carrito == null || !carrito.Any()) return RedirectToAction("Index", "Carrito");

            // Por ahora usaremos un ID de cliente fijo (1) hasta implementar el Login
            int clienteIdSimulado = 1;

            try
            {
                int reservaId = await _reservaService.CrearReservaAsync(carrito, clienteIdSimulado, archivosRecetas);

                // Si se guarda con éxito, limpiamos el carrito
                _carritoService.LimpiarCarrito();

                // Usamos TempData para mostrar el mensaje de éxito como en el instructivo
                TempData["Exito"] = $"Reserva #{reservaId} generada con éxito. Tienes 24 horas para recogerla.";
                return RedirectToAction("MisReservas");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al procesar la reserva: " + ex.Message);
                return RedirectToAction("Index", "Carrito");
            }
        }

        public async Task<IActionResult> MisReservas()
        {
            int clienteIdSimulado = 1;
            var reservas = await _reservaService.GetReservasByClienteAsync(clienteIdSimulado);
            return View(reservas);
        }
        public async Task<IActionResult> Detalles(int id)
        {
            var reserva = await _reservaService.GetReservaDetalladaAsync(id);

            if (reserva == null) return NotFound();

            return View(reserva);
        }
    }
}