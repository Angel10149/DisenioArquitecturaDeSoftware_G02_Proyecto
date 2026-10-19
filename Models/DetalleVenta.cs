using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace AppWeb.Models
{
    public class DetalleVenta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int idDetalleVenta { get; set; }
        [Required]
        public int cantidad { get; set; }
        [Required]
        public decimal precioUnitario { get; set; }
        [Required]
        public decimal descuento { get; set; }
        [Required]
        public decimal subtotal { get; set; }

        //relacion de m : 1 con Venta
        public int idventa { get; set; }
        public Venta venta { get; set; }

        //relacion de m : 1 con Producto
        public int idproducto { get; set; }
        public Producto producto { get; set; }

    }
}
