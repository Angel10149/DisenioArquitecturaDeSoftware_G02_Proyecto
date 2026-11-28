using System.ComponentModel.DataAnnotations;
using AppWeb.Validations;

namespace AppWeb.ViewModels
{
    public class EditarUsuarioVM
    {
        public int id { get; set; }

        [CampoObligatorio(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres")]
        [SoloLetras(ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string nombreUsuario { get; set; } = string.Empty;

        [CampoObligatorio(ErrorMessage = "El correo electrónico es obligatorio")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres")]
        [CorreoElectronico]
        public string correo { get; set; } = string.Empty;

        [CampoObligatorio(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres")]
        [TelefonoPeruano]
        public string telefono { get; set; } = string.Empty;

        [NumeroPositivo(ErrorMessage = "El rol seleccionado no es válido")]
        public int IDROL { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener entre 6 y 100 caracteres")]
        public string? nuevaPassword { get; set; }
    }
}
