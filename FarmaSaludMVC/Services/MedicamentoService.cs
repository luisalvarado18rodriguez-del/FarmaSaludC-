using FarmaSaludMVC.Data;
using FarmaSaludMVC.Interfaces;
using FarmaSaludMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmaSaludMVC.Services
{
    public class MedicamentoService : IMedicamentoService
    {
        private readonly AppDbContext _context;

        public MedicamentoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Medicamento>> GetAllAsync() =>
            await _context.Medicamentos.Include(m => m.Categoria).ToListAsync();

        public async Task<Medicamento?> GetByIdAsync(int id) =>
            await _context.Medicamentos.Include(m => m.Categoria).FirstOrDefaultAsync(m => m.Id == id);

        public async Task<List<Medicamento>> GetByCategoriaAsync(int categoriaId) =>
            await _context.Medicamentos.Where(m => m.CategoriaId == categoriaId).ToListAsync();

        public async Task<bool> ActualizarStockAsync(int id, int cantidad)
        {
            var med = await _context.Medicamentos.FindAsync(id);
            if (med == null) return false;

            med.Stock += cantidad; // cantidad puede ser negativa para restar
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            return await _context.Categorias.ToListAsync();
        }
    }
}