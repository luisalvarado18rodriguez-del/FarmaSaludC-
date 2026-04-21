namespace FarmaSaludMVC.ViewModels
{
    public class CarritoItemViewModel
    {
        public int MedicamentoId { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public bool RequiereReceta { get; set; }
        public string? RutaReceta { get; set; } // Para cumplir con la regla de negocio
        public decimal Subtotal => Precio * Cantidad;
    }
}