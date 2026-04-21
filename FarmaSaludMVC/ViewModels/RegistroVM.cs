using System.ComponentModel.DataAnnotations;

namespace FarmaSaludMVC.ViewModels
{
    public class RegistroVM
    {
        [Required, StringLength(8)]
        public string DNI { get; set; }

        [Required, StringLength(50)]
        public string Nombre { get; set; }

        [Required, StringLength(50)]
        public string Apellido { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        [MinLength(3, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmarPassword { get; set; }
    }
}
