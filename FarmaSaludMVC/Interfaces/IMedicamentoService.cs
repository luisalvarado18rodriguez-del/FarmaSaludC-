using FarmaSaludMVC.Models;

namespace FarmaSaludMVC.Interfaces
{
    public interface IMedicamentoService
    {
        Task<List<Medicamento>> GetAllAsync();
        Task<Medicamento?> GetByIdAsync(int id);
        Task<List<Medicamento>> GetByCategoriaAsync(int categoriaId);
        // Métodos para el proceso de stock
        Task<bool> ActualizarStockAsync(int id, int cantidad);
        Task<List<Categoria>> GetCategoriasAsync();
    }
}