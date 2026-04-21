using System.ComponentModel.DataAnnotations;

namespace FarmaSaludMVC.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(8)]
        public string DNI { get; set; }

        [Required, StringLength(50)]
        public string Nombre { get; set; }

        [Required, StringLength(50)]
        public string Apellido { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [StringLength(9)]
        public string? Telefono { get; set; }

        // Relación con Usuario
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}