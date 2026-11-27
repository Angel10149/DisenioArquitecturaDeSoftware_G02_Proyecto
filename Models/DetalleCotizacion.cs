using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class DetalleCotizacion
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleCotizacion { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
        public int cantidad { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo")]
        public decimal precioUnitario { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El descuento no puede ser negativo")]
        public decimal descuento { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El subtotal no puede ser negativo")]
        public decimal subtotal { get; set; }

        //Relacion de M : 1 con Cotizacion
        public int idCotizacion { get; set; }
        public Cotizacion? cotizacion { get; set; }

        //Relacion de M : 1 con Producto
        public int idProducto { get; set; }
        public Producto? producto { get; set; }
    }
}