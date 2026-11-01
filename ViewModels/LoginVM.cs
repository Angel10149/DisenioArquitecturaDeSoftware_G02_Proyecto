using System.ComponentModel.DataAnnotations;

namespace AppWeb.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        public string correo { get; set; } = string.Empty;

    }
}
