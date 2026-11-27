using System.ComponentModel.DataAnnotations;

namespace AppWeb.ViewModels
{
    public class PerfilUsuarioVM
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string nombreUsuario { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "La contraseña actual no puede exceder los 100 caracteres")]
        public string? passwordActual { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener entre 6 y 100 caracteres")]
        public string? nuevaPassword { get; set; }

        [Compare("nuevaPassword", ErrorMessage = "La nueva contraseña y su confirmación no coinciden")]
        public string? confirmarPassword { get; set; }
    }
}
