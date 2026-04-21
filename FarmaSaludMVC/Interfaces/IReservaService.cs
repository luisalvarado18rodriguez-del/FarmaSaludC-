using FarmaSaludMVC.Models;
using FarmaSaludMVC.ViewModels;

namespace FarmaSaludMVC.Interfaces
{
    public interface IReservaService
    {
        // Proceso principal: Crea la reserva, resta stock y guarda archivos de recetas
        Task<int> CrearReservaAsync(List<CarritoItemViewModel> carrito, int clienteId, List<IFormFile> archivosRecetas);

        // Consulta para el Cliente: Historial de sus propias reservas
        Task<List<Reserva>> GetReservasByClienteAsync(int clienteId);

        // Consulta para el Administrador: Todas las reservas vigentes en el sistema
        Task<List<Reserva>> GetAllReservasActivasAsync();
        // ver detalle reserva
        Task<Reserva> GetReservaDetalladaAsync(int reservaId);

        // Lógica de negocio: Cancela reservas fuera de tiempo (24h) y devuelve el stock al inventario
        Task<int> ProcesarCancelacionesAutomaticasAsync();
        Task<bool> FinalizarReservaAsync(int reservaId);
    }
}