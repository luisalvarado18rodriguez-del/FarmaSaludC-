using System.ComponentModel.DataAnnotations;

namespace FarmaSaludMVC.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaReserva { get; set; } = DateTime.Now;

        // Fecha limite para recoger (24 horas después)
        public DateTime FechaLimite { get; set; }

        [Required]
        public string Estado { get; set; } = "En espera"; // En espera, Confirmada, Terminada, Cancelada

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        [Required]
        public bool Activo { get; set; } = true; // Nuevo: true = vigente, false = borrado lógico
        public ICollection<ReservaDetalle> ReservasDetalles { get; set; } = new List<ReservaDetalle>();
    }
}