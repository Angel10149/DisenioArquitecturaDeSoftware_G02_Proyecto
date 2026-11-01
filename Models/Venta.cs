using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Venta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idVenta { get; set; }

        [Required(ErrorMessage = "La fecha de la venta es obligatoria")]
        [Display(Name = "Fecha de Venta")]
        public DateTime fechaVenta { get; set; }

        [Required(ErrorMessage = "El estado de la venta es obligatorio")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres")]
        public string estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "El total de la venta es obligatorio")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.00, (double)decimal.MaxValue, ErrorMessage = "El total de la venta no puede ser negativo")]
        [Display(Name = "Total")]
        public decimal total { get; set; }

        // Relacion de M : 1 con Cliente
        [Required(ErrorMessage = "El cliente es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido")]
        public int idCliente { get; set; }
        public Cliente? cliente { get; set; }

        // Relacion de M : 1 con Usuario
        [Required(ErrorMessage = "El usuario vendedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un usuario responsable")]
        public int idUsuario { get; set; }
        public Usuario? usuario { get; set; }

        // Relacion de M : 1 con Cotizacion
        [Required(ErrorMessage = "La cotización es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe asociar una cotización válida")]
        public int idCotizacion { get; set; }
        public Cotizacion? cotizacion { get; set; }

        // Relaciones 1 : M con DetalleVenta
        public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();

        // Relaciones 1 : M con Pago
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}