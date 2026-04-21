using FarmaSaludMVC.Interfaces;
using FarmaSaludMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSaludMVC.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ICarritoService _carritoService;
        private readonly IMedicamentoService _medicamentoService;

        public CarritoController(ICarritoService carritoService, IMedicamentoService medicamentoService)
        {
            _carritoService = carritoService;
            _medicamentoService = medicamentoService;
        }

        // 1. Ver el Carrito
        public IActionResult Index()
        {
            var items = _carritoService.ObtenerCarrito();
            ViewBag.Total = _carritoService.ObtenerTotal();
            return View(items);
        }

        // 2. Agregar al Carrito (Desde el catálogo)
        [HttpPost]
        public async Task<IActionResult> Agregar(int id, int cantidad) // Añadimos el parámetro cantidad
        {
            var med = await _medicamentoService.GetByIdAsync(id);
            if (med == null || med.Stock < cantidad)
            {
                TempData["Error"] = "No hay suficiente stock disponible.";
                return RedirectToAction("Catalogo", "Medicamento");
            }

            var item = new CarritoItemViewModel
            {
                MedicamentoId = med.Id,
                Nombre = med.Nombre,
                Precio = med.Precio,
                Cantidad = cantidad, // Usamos la cantidad recibida
                RequiereReceta = med.RequiereReceta
            };

            _carritoService.AgregarProducto(item);
            return RedirectToAction("Index");
        }

        // 3. Eliminar del Carrito
        public IActionResult Eliminar(int id)
        {
            _carritoService.QuitarProducto(id);
            return RedirectToAction("Index");
        }
    }
}