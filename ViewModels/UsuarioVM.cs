using System.ComponentModel.DataAnnotations;

namespace AppWeb.ViewModels
{
    public class UsuarioVM
    {
        [Required(ErrorMessage = "El nombre del usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del usuario no puede exceder los 50 caracteres")]
        public string nombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe repetir la contraseña")]
        [Compare("password", ErrorMessage = "Las contraseñas no coinciden")]
        public string reppassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres")]
        public string correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres")]
        public string telefono { get; set; } = string.Empty;
        /*
        public bool estado { get; set; }*/

        //Relacion de M : 1 con Rol
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido")]
        public int IDROL { get; set; }
    }
}
