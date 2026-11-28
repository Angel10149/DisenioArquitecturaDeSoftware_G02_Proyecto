using System.ComponentModel.DataAnnotations;
using AppWeb.Validations;

namespace AppWeb.ViewModels
{
    public class LoginVM
    {
        [CampoObligatorio(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La contraseña no puede exceder los 100 caracteres")]
        public string password { get; set; } = string.Empty;

        [CampoObligatorio(ErrorMessage = "El correo es obligatorio")]
        [CorreoElectronico]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres")]
        public string correo { get; set; } = string.Empty;
    }
}
