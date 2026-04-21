using FarmaSaludMVC.ViewModels;

namespace FarmaSaludMVC.Interfaces
{
    public interface ICarritoService
    {
        List<CarritoItemViewModel> ObtenerCarrito();
        void AgregarProducto(CarritoItemViewModel item);
        void QuitarProducto(int medicamentoId);
        void LimpiarCarrito();
        decimal ObtenerTotal();
    }
}