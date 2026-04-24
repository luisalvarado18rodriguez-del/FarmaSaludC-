using FarmaSaludMVC.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSaludMVC.Controllers
{
    [Authorize(Roles = "Cliente,Admin,SuperAdmin")]
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
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Confirmar(List<IFormFile> archivosRecetas)
        {
            var carrito = _carritoService.ObtenerCarrito();
            if (carrito == null || !carrito.Any()) return RedirectToAction("Index", "Carrito");

            // AJUSTE: Extraer ID real del Claim
            var clienteIdClaim = User.FindFirst("ClienteId")?.Value;
            if (string.IsNullOrEmpty(clienteIdClaim)) return RedirectToAction("Login", "Account");

            int clienteIdReal = int.Parse(clienteIdClaim);

            try
            {
                // Usamos el ID real obtenido del usuario logueado
                int reservaId = await _reservaService.CrearReservaAsync(carrito, clienteIdReal, archivosRecetas);

                _carritoService.LimpiarCarrito();
                TempData["Exito"] = $"Reserva #{reservaId} generada con éxito. Tienes 24 horas para recogerla.";
                return RedirectToAction("MisReservas");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al procesar la reserva: " + ex.Message);
                return RedirectToAction("Index", "Carrito");
            }
        }

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisReservas()
        {
            // AJUSTE: Extraer ID real del Claim
            var clienteIdClaim = User.FindFirst("ClienteId")?.Value;
            if (string.IsNullOrEmpty(clienteIdClaim)) return RedirectToAction("Login", "Account");

            int clienteIdReal = int.Parse(clienteIdClaim);

            // El servicio ahora solo traerá las reservas de este cliente
            var reservas = await _reservaService.GetReservasByClienteAsync(clienteIdReal);
            return View(reservas);
        }

        public async Task<IActionResult> Detalles(int id)
        {
            var reserva = await _reservaService.GetReservaDetalladaAsync(id);

            if (reserva == null) return NotFound();

            // SEGURIDAD EXTRA: Si es un Cliente, verificar que la reserva le pertenezca
            if (User.IsInRole("Cliente"))
            {
                var clienteId = int.Parse(User.FindFirst("ClienteId")?.Value ?? "0");
                if (reserva.ClienteId != clienteId) return Forbid();
            }

            return View(reserva);
        }
    }
}