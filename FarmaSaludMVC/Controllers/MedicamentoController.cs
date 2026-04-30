using FarmaSaludMVC.Interfaces;
using FarmaSaludMVC.Services;
using FarmaSaludMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSaludMVC.Controllers
{
    [Authorize(Roles = "Cliente,Admin,SuperAdmin")]
    public class MedicamentoController : Controller
    {
        private readonly IMedicamentoService _service;

        // Inyección del servicio a través del constructor
        public MedicamentoController(IMedicamentoService service)
        {
            _service = service;
        }

        // Acción para ver el Catálogo (Vista para el Cliente)
        public async Task<IActionResult> Catalogo(int? categoriaId)
        {
            var medicamentosQuery = _service.GetAllAsync(); // Supongamos que este trae todos
            var medicamentos = await medicamentosQuery;

            if (categoriaId.HasValue && categoriaId > 0)
            {
                medicamentos = medicamentos.Where(m => m.CategoriaId == categoriaId).ToList();
            }

            // Cargamos las categorías para el ComboBox
            ViewBag.Categorias = await _service.GetCategoriasAsync();
            ViewBag.CategoriaSeleccionada = categoriaId;

            var viewModel = medicamentos.Select(m => new MedicamentoCatalogoViewModel
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Precio = m.Precio,
                CategoriaNombre = m.Categoria?.Nombre,
                UrlImagen = m.UrlImagen,
                RequiereReceta = m.RequiereReceta,
                StockDisponible = m.Stock

            }).ToList();

            return View(viewModel);
        }
        // Acción para ver el detalle de un medicamento
        public async Task<IActionResult> Detalle(int id)
        {
            var medicamento = await _service.GetByIdAsync(id);
            if (medicamento == null) return NotFound();

            return View(medicamento);
        }
        public async Task<IActionResult> Alertas()
        {
            // Este método ya filtra por < 3 meses o Stock > 50
            var medicamentosEnRiesgo = await _service.GetMedicamentosEnRiesgoAsync();
            return View(medicamentosEnRiesgo);
        }
    }
}