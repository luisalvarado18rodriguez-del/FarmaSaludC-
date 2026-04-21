using System.ComponentModel.DataAnnotations;

namespace FarmaSaludMVC.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
    }
}
