using FarmaSaludMVC.Data;
using FarmaSaludMVC.Interfaces;
using FarmaSaludMVC.Models;
using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Hosting; // Necesario para IWebHostEnvironment
//using Microsoft.Http;


namespace FarmaSaludMVC.Services
{
    public class MedicamentoService : IMedicamentoService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; //para manejar la ruta de imagenes

        public MedicamentoService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<List<Medicamento>> GetAllAsync() =>
            await _context.Medicamentos.Include(m => m.Categoria).ToListAsync();

        public async Task<Medicamento?> GetByIdAsync(int id) =>
            await _context.Medicamentos.Include(m => m.Categoria).FirstOrDefaultAsync(m => m.Id == id);

        public async Task<List<Medicamento>> GetByCategoriaAsync(int categoriaId) =>
            await _context.Medicamentos.Where(m => m.CategoriaId == categoriaId).ToListAsync();

        public async Task<bool> AddAsync(Medicamento medicamento, IFormFile? imagen)
        {
            if (imagen != null)
            {
                medicamento.UrlImagen = await GuardarImagen(imagen);
            }

            _context.Medicamentos.Add(medicamento);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Medicamento medicamento, IFormFile? imagen)
        {
            var medExistente = await _context.Medicamentos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == medicamento.Id);
            if (medExistente == null) return false;

            if (imagen != null)
            {
                // Si hay imagen nueva, la guardamos
                medicamento.UrlImagen = await GuardarImagen(imagen);
            }
            else
            {
                // Si no hay imagen nueva, conservamos la que ya tenía
                medicamento.UrlImagen = medExistente.UrlImagen;
            }

            _context.Medicamentos.Update(medicamento);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var med = await _context.Medicamentos.FindAsync(id);
            if (med == null) return false;

            _context.Medicamentos.Remove(med);
            return await _context.SaveChangesAsync() > 0;
        }

        // MÉTODO PRIVADO AUXILIAR PARA LA IMAGEN
        private async Task<string> GuardarImagen(IFormFile imagen)
        {
            string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
            string rutaCarpeta = Path.Combine(_env.WebRootPath, "uploads", "productos");

            // Crear carpeta si no existe
            if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);

            string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }
            return nombreArchivo; // Retornamos solo el nombre para guardarlo en la DB
        }
        public async Task<List<Medicamento>> GetMedicamentosEnRiesgoAsync()
        {
            DateTime fechaLimiteVencimiento = DateTime.Now.AddMonths(3);

            return await _context.Medicamentos
                .Include(m => m.Categoria)
                .Where(m => m.FechaVencimiento <= fechaLimiteVencimiento || m.Stock > 50)
                .OrderBy(m => m.FechaVencimiento) // Priorizamos los que vencen pronto
                .ToListAsync();
        }
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