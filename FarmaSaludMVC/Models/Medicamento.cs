using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaSaludMVC.Models
{
    public class Medicamento
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Define precisión para moneda
        public decimal Precio { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required, StringLength(20)]
        public string Lote { get; set; }

        [Required]
        public DateTime FechaVencimiento { get; set; }

        [Required]
        public bool RequiereReceta { get; set; }

        [StringLength(500)]
        public string? UrlImagen { get; set; }

        // Relación con Categoría
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
    }
}