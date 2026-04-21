using System.ComponentModel.DataAnnotations;

namespace FarmaSaludMVC.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required, StringLength(255)]
        public string Password { get; set; }

        [Required]
        public string Rol { get; set; } // "Administrador" o "Cliente"

        public bool Activo { get; set; } = true; // Para banear/desactivar
    }
}