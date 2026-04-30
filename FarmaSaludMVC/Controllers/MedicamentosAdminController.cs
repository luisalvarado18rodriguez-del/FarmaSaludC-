using FarmaSaludMVC.Interfaces;
using FarmaSaludMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmaSaludMVC.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class MedicamentosAdminController : Controller
    {
        private readonly IMedicamentoService _medicamentoService;

        public MedicamentosAdminController(IMedicamentoService medicamentoService)
        {
            _medicamentoService = medicamentoService;
        }

        // LISTADO GENERAL: El "Filtro Operativo"
        public async Task<IActionResult> Index()
        {
            var medicamentos = await _medicamentoService.GetAllAsync();
            return View(medicamentos);
        }

        // APARTADO DE ESTADÍSTICAS: La "Inteligencia de Promociones"
        public async Task<IActionResult> Alertas()
        {
            // Usamos el método que acabamos de crear
            var medicamentosEnRiesgo = await _medicamentoService.GetMedicamentosEnRiesgoAsync();
            return View(medicamentosEnRiesgo);
        }

        // ACCIÓN PARA CREAR (GET): Carga el formulario y el combo de categorías
        public async Task<IActionResult> Crear()
        {
            ViewBag.Categorias = await _medicamentoService.GetCategoriasAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Medicamento medicamento, IFormFile? imagen)
        {
            if (ModelState.IsValid)
            {
                // El servicio se encarga de guardar la imagen y el registro
                await _medicamentoService.AddAsync(medicamento, imagen);
                return RedirectToAction(nameof(Index));
            }

            // Si hay error, recargamos las categorías para el combo
            ViewBag.Categorias = await _medicamentoService.GetCategoriasAsync();
            return View(medicamento);
        }
        // GET: Carga el formulario con los datos existentes
        public async Task<IActionResult> Editar(int id)
        {
            var medicamento = await _medicamentoService.GetByIdAsync(id);
            if (medicamento == null) return NotFound();

            ViewBag.Categorias = await _medicamentoService.GetCategoriasAsync();
            return View(medicamento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Medicamento medicamento, IFormFile? imagen)
        {
            if (id != medicamento.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // El servicio ya tiene la lógica para decidir si reemplaza la imagen o deja la anterior
                bool actualizado = await _medicamentoService.UpdateAsync(medicamento, imagen);

                if (actualizado) return RedirectToAction(nameof(Index));

                ModelState.AddModelError("", "No se pudo actualizar el medicamento.");
            }

            ViewBag.Categorias = await _medicamentoService.GetCategoriasAsync();
            return View(medicamento);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _medicamentoService.DeleteAsync(id);
            if (!eliminado)
            {
                TempData["Error"] = "No se pudo eliminar el medicamento.";
            }
            else
            {
                TempData["Exito"] = "Medicamento eliminado correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
