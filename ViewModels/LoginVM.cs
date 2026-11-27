using System.ComponentModel.DataAnnotations;

namespace AppWeb.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La contraseña no puede exeder los 100 caracteres")]
        public string password { get; set; } = string.Empty;


        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exeder los 100 caracteres")]
        public string correo { get; set; } = string.Empty;

    }
}
