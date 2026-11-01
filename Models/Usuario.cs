using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required(ErrorMessage = "El nombre del usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del usuario no puede exceder los 50 caracteres")]
        public string nombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La contraseña no puede exceder los 100 caracteres")]
        public string password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres")]
        public string correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres")]
        public string telefono { get; set; } = string.Empty;

        /*[Required]
        public bool estado { get; set; }*/

        // Relacion de M : 1 con Rol
        [Required(ErrorMessage = "El rol es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido")]
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
