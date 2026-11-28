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
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
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

        //relacion de m : 1 con Venta
        public int idventa { get; set; }
        public Venta? venta { get; set; }

        //relacion de m : 1 con Producto
        public int idproducto { get; set; }
        public Producto? producto { get; set; }

    }
}
