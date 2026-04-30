using FarmaSaludMVC.Models;

namespace FarmaSaludMVC.Interfaces
{
    public interface IMedicamentoService
    {
        Task<List<Medicamento>> GetAllAsync();
        Task<Medicamento?> GetByIdAsync(int id);
        Task<List<Medicamento>> GetByCategoriaAsync(int categoriaId);
        Task<List<Categoria>> GetCategoriasAsync();
        Task<List<Medicamento>> GetMedicamentosEnRiesgoAsync();

        Task<bool> AddAsync(Medicamento medicamento, IFormFile? imagen);
        Task<bool> UpdateAsync(Medicamento medicamento, IFormFile? imagen);
        Task<bool> DeleteAsync(int id);
        // Métodos para el proceso de stock
        Task<bool> ActualizarStockAsync(int id, int cantidad);


    }
}