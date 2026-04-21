namespace FarmaSaludMVC.ViewModels
{
    public class MedicamentoCatalogoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal Precio { get; set; }
        public string? CategoriaNombre { get; set; }
        public string? UrlImagen { get; set; }
        public bool RequiereReceta { get; set; }
        public int StockDisponible { get; set; }
    }
}