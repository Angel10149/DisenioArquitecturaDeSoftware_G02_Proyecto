using System.ComponentModel.DataAnnotations;
using AppWeb.Validations;

namespace AppWeb.ViewModels
{
    public class UsuarioVM
    {
        [CampoObligatorio(ErrorMessage = "El nombre del usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del usuario no puede exceder los 50 caracteres")]
        [SoloLetras(ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string nombreUsuario { get; set; } = string.Empty;

        [CampoObligatorio(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string password { get; set; } = string.Empty;

        [CampoObligatorio(ErrorMessage = "Debe repetir la contraseña")]
        [Compare("password", ErrorMessage = "Las contraseñas no coinciden")]
        public string reppassword { get; set; } = string.Empty;

        [CampoObligatorio(ErrorMessage = "El correo electrónico es obligatorio")]
        [CorreoElectronico]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres")]
        public string correo { get; set; } = string.Empty;

        [CampoObligatorio(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres")]
        [TelefonoPeruano]
        public string telefono { get; set; } = string.Empty;

        //Relacion de M : 1 con Rol
        [NumeroPositivo(ErrorMessage = "Debe seleccionar un rol válido")]
        public int IDROL { get; set; }
    }
}
