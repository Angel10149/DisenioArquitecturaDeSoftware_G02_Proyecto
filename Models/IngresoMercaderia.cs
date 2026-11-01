using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class IngresoMercaderia
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idIngresoMercaderia { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
        [Display(Name = "Fecha de Ingreso")]
        public DateTime fechaIngreso { get; set; }

        [Required(ErrorMessage = "El estado del ingreso es obligatorio")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres")]
        public string estado { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Las observaciones no pueden exceder los 255 caracteres")]
        public string observaciones { get; set; } = string.Empty;

        // Relacion de M : 1 con OrdenCompra
        [Required(ErrorMessage = "La orden de compra es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe asociar una orden de compra válida")]
        public int idOrdenCompra { get; set; }
        public OrdenCompra? ordenCompra { get; set; }

        // Relacion de M : 1 con Proveedor
        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido")]
        public int idProveedor { get; set; }
        public Proveedor? proveedor { get; set; }

        // Relaciones 1 : M con DetalleIngreso
        public ICollection<DetalleIngreso> DetalleIngresos { get; set; } = new List<DetalleIngreso>();
    }
}
