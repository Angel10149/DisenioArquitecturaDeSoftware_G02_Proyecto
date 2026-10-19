using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class DetalleCotizacion
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleCotizacion { get; set; }
        [Required]
        public int cantidad { get; set; }
        [Required]
        public decimal precioUnitario { get; set; }
        [Required]
        public decimal descuento { get; set; }
        [Required]
        public decimal subtotal { get; set; }

        //Relacion de M : 1 con Cotizacion
        public int idCotizacion { get; set; }
        public Cotizacion cotizacion { get; set; }

        //Relacion de M : 1 con Producto
        public int idProducto { get; set; }
        public Producto producto { get; set; }
    }
}