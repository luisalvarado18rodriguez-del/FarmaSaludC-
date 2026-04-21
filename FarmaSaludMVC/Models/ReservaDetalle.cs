using System.ComponentModel.DataAnnotations;

namespace FarmaSaludMVC.Models
{
    public class ReservaDetalle
    {
        [Key]
        public int Id { get; set; }

        public int ReservaId { get; set; }
        public Reserva? Reserva { get; set; }

        public int MedicamentoId { get; set; }
        public Medicamento? Medicamento { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        // Ruta del archivo de la receta (solo si el medicamento lo requiere)
        public string? RutaReceta { get; set; }
    }
}