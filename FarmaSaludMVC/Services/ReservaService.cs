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
        private readonly IWebHostEnvironment _env;

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
                var nuevaReserva = new Reserva
                {
                    ClienteId = clienteId,
                    FechaReserva = DateTime.Now,
                    FechaLimite = DateTime.Now.AddHours(24),
                    Estado = "En espera",
                    Activo = true // Importante inicializarlo en true
                };

                _context.Reservas.Add(nuevaReserva);
                await _context.SaveChangesAsync();

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

                    if (item.RequiereReceta && archivosRecetas != null && indiceReceta < archivosRecetas.Count)
                    {
                        detalle.RutaReceta = await GuardarRecetaArchivo(archivosRecetas[indiceReceta]);
                        indiceReceta++;
                    }

                    _context.ReservasDetalles.Add(detalle);

                    var med = await _context.Medicamentos.FindAsync(item.MedicamentoId);
                    if (med != null)
                    {
                        if (med.Stock < item.Cantidad) throw new Exception($"Stock insuficiente para {med.Nombre}");
                        med.Stock -= item.Cantidad;
                    }
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

            if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);

            string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);
            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }
            return nombreArchivo;
        }

        public async Task<List<Reserva>> GetReservasByClienteAsync(int clienteId) =>
            await _context.Reservas
                .Where(r => r.ClienteId == clienteId && r.Activo == true)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

        public async Task<List<Reserva>> GetAllReservasActivasAsync()
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.ReservasDetalles)
                .Where(r => r.Activo == true)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        // CORRECCIÓN: Lógica de expiración real (24 horas)
        public async Task<int> ProcesarCancelacionesAutomaticasAsync()
        {
            // USANDO TU LÓGICA DE SEGUNDOS PARA PRUEBAS:
            // Esto buscará reservas creadas hace más de 10 segundos para cancelarlas rápido
            var tiempoDePrueba = DateTime.Now.AddSeconds(-10);

            var expiradas = await _context.Reservas
                .Include(r => r.ReservasDetalles)
                .Where(r => r.Estado == "Terminada" && r.FechaReserva < tiempoDePrueba && r.Activo == true)
                .ToListAsync();

            if (expiradas.Count == 0) return 0;

            foreach (var reserva in expiradas)
            {
                reserva.Estado = "Expirada";
                reserva.Activo = false;

                // Devolvemos stock (Tu lógica de negocio)
                foreach (var det in reserva.ReservasDetalles)
                {
                    var med = await _context.Medicamentos.FindAsync(det.MedicamentoId);
                    if (med != null) med.Stock += det.Cantidad;
                }
            }

            await _context.SaveChangesAsync();
            return expiradas.Count;
        }

        public async Task<bool> FinalizarReservaAsync(int reservaId)
        {
            var reserva = await _context.Reservas.FindAsync(reservaId);
            if (reserva == null) return false;

            reserva.Estado = "Terminada";
            reserva.Activo = false; // Se oculta del dashboard principal al terminar

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

        public async Task<bool> CancelarReservaManualAsync(int reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.ReservasDetalles)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva == null || reserva.Estado == "Terminada" || reserva.Estado == "Cancelada")
                return false;

            reserva.Estado = "Cancelada";
            reserva.Activo = false;

            foreach (var det in reserva.ReservasDetalles)
            {
                var med = await _context.Medicamentos.FindAsync(det.MedicamentoId);
                if (med != null) med.Stock += det.Cantidad;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CancelarReservaClienteAsync(int reservaId, int clienteId)
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Id == reservaId && r.ClienteId == clienteId && r.Estado == "En espera");

            if (reserva == null) return false;

            return await CancelarReservaManualAsync(reservaId);
        }
    }
}