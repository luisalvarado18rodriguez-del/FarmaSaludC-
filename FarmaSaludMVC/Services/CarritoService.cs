using FarmaSaludMVC.Interfaces;
using FarmaSaludMVC.ViewModels;
using Newtonsoft.Json; // Necesitarás instalar el paquete Newtonsoft.Json vía NuGet

namespace FarmaSaludMVC.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "CarritoFarmaSalud";

        public CarritoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<CarritoItemViewModel> ObtenerCarrito()
        {
            var sessionData = _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);
            return sessionData == null ? new List<CarritoItemViewModel>()
                                     : JsonConvert.DeserializeObject<List<CarritoItemViewModel>>(sessionData)!;
        }

        public void AgregarProducto(CarritoItemViewModel item)
        {
            var carrito = ObtenerCarrito();
            var existente = carrito.FirstOrDefault(x => x.MedicamentoId == item.MedicamentoId);

            if (existente != null) existente.Cantidad += item.Cantidad;
            else carrito.Add(item);

            GuardarCarrito(carrito);
        }

        public void QuitarProducto(int medicamentoId)
        {
            var carrito = ObtenerCarrito();
            carrito.RemoveAll(x => x.MedicamentoId == medicamentoId);
            GuardarCarrito(carrito);
        }

        public void LimpiarCarrito() => _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);

        public decimal ObtenerTotal() => ObtenerCarrito().Sum(x => x.Subtotal);

        private void GuardarCarrito(List<CarritoItemViewModel> carrito)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(SessionKey, JsonConvert.SerializeObject(carrito));
        }
    }
}