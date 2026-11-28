using System.ComponentModel.DataAnnotations;

namespace AppWeb.ViewModels
{
    public class EditarUsuarioVM
    {
        public int id { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string nombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        public string correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Ingrese un teléfono válido")]
        [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres")]
        [RegularExpression(@"^\+?(?=(?:.*\d){7,15}$)[0-9\-\s]+$", ErrorMessage = "Ingrese un teléfono válido entre 7 y 15 dígitos")]
        public string telefono { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "El rol seleccionado no es válido")]
        public int IDROL { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener entre 6 y 100 caracteres")]
        public string? nuevaPassword { get; set; }
    }
}
