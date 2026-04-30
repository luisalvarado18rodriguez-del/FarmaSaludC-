using FarmaSaludMVC.Data;
using FarmaSaludMVC.Interfaces;
using FarmaSaludMVC.Models;
using FarmaSaludMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FarmaSaludMVC.Services
{
    public class ReservaService : IReservaService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // Para acceder a wwwroot

        public ReservaService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<int> CrearReservaAsync(List<CarritoItemViewModel> carrito, int clienteId, List<IFormFile> archivosRecetas)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Crear la cabecera de la Reserva
                var nuevaReserva = new Reserva
                {
                    ClienteId = clienteId,
                    FechaReserva = DateTime.Now,
                    FechaLimite = DateTime.Now.AddHours(24), // Regla de las 24 horas
                    Estado = "En espera"
                };

                _context.Reservas.Add(nuevaReserva);
                await _context.SaveChangesAsync();

                // 2. Procesar los detalles del carrito
                int indiceReceta = 0;
                foreach (var item in carrito)
                {
                    var detalle = new ReservaDetalle
                    {
                        ReservaId = nuevaReserva.Id,
                        MedicamentoId = item.MedicamentoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio
                    };

                    // Lógica de carga de receta si el medicamento la requiere
                    if (item.RequiereReceta && archivosRecetas != null && indiceReceta < archivosRecetas.Count)
                    {
                        detalle.RutaReceta = await GuardarRecetaArchivo(archivosRecetas[indiceReceta]);
                        indiceReceta++;
                    }

                    _context.ReservasDetalles.Add(detalle);

                    // 3. Restar Stock (Lógica de negocio)
                    var med = await _context.Medicamentos.FindAsync(item.MedicamentoId);
                    if (med != null) med.Stock -= item.Cantidad;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return nuevaReserva.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<string> GuardarRecetaArchivo(IFormFile archivo)
        {
            string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
            string rutaCarpeta = Path.Combine(_env.WebRootPath, "uploads", "recetas");
            string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }
            return nombreArchivo;
        }

        public async Task<List<Reserva>> GetReservasByClienteAsync(int clienteId) =>
            await _context.Reservas.Where(r => r.ClienteId == clienteId).ToListAsync();

       
        // 1. Obtener todas las reservas activas (para el Admin)
        public async Task<List<Reserva>> GetAllReservasActivasAsync()
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.ReservasDetalles)
                .Where(r => r.Activo == true)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        // 2. Lógica de Cancelación Automática (Regla de las 24 horas)
        public async Task<int> ProcesarCancelacionesAutomaticasAsync()
        {
            // Tiempo de espera después de que pasó a "Terminada"
            var tiempoLimite = DateTime.Now.AddSeconds(-10);

            // BUSQUEDA: Solo las que ya están "Terminada" y siguen "Activo = true"
            var paraLimpiar = await _context.Reservas
                .Where(r => r.Estado == "Terminada"&& r.Activo == true)
                .ToListAsync();

            if (paraLimpiar.Count == 0) return 0;

            foreach (var reserva in paraLimpiar)
            {
                // NOTA: Aquí NO devolvemos stock porque si está "Terminada" 
                // significa que el medicamento ya se vendió.

                reserva.Activo = false; // Esto la quita del Dashboard
            }

            await _context.SaveChangesAsync();
            return paraLimpiar.Count;
        }
        public async Task<bool> FinalizarReservaAsync(int reservaId)
        {
            var reserva = await _context.Reservas.FindAsync(reservaId);

            if (reserva == null) return false;

            reserva.Estado = "Terminada";

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Reserva> GetReservaDetalladaAsync(int reservaId)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.ReservasDetalles)
                    .ThenInclude(d => d.Medicamento)
                .FirstOrDefaultAsync(r => r.Id == reservaId);
        }

    }
}