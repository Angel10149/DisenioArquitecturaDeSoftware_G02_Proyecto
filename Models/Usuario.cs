using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppWeb.Validations;

namespace AppWeb.Models
{
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [CampoObligatorio(ErrorMessage = "El nombre del usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del usuario no puede exceder los 50 caracteres")]
        [SoloLetras(ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string nombreUsuario { get; set; } = string.Empty;

        [CampoObligatorio(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La contraseña no puede exceder los 100 caracteres")]
        public string password { get; set; } = string.Empty;

        [CampoObligatorio(ErrorMessage = "El correo electrónico es obligatorio")]
        [CorreoElectronico]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres")]
        public string correo { get; set; } = string.Empty;

        [CampoObligatorio(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres")]
        [TelefonoPeruano]
        public string telefono { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Estado")]
        public bool estado { get; set; } = true;

        // Relacion de M : 1 con Rol
        [Required(ErrorMessage = "El rol es obligatorio")]
        [NumeroPositivo(ErrorMessage = "Debe seleccionar un rol válido")]
        public int IDROL { get; set; }
        public Rol? Rol { get; set; }

        // Relaciones 1 : M con Cotizacion
        public ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();

        // Relaciones 1 : M con OrdenCompra
        public ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();

        // Relaciones 1 : M con Pago
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // Relaciones 1 : M con RequerimientoCompra
        public ICollection<RequerimientoCompra> RequerimientoCompras { get; set; } = new List<RequerimientoCompra>();

        // Relaciones 1 : M con Venta
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
