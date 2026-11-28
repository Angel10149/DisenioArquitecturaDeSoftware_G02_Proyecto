using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppWeb.Validations;

namespace AppWeb.Models
{
    public class DetalleVenta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleVenta { get; set; }

        [Required]
        [NumeroPositivo(ErrorMessage = "La cantidad debe ser mayor que cero")]
        public int cantidad { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [NumeroNoNegativo(ErrorMessage = "El precio unitario no puede ser negativo")]
        public decimal precioUnitario { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [NumeroNoNegativo(ErrorMessage = "El descuento no puede ser negativo")]
        public decimal descuento { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [NumeroNoNegativo(ErrorMessage = "El subtotal no puede ser negativo")]
        public decimal subtotal { get; set; }

        //relacion de m : 1 con Venta
        public int idventa { get; set; }
        public Venta? venta { get; set; }

        //relacion de m : 1 con Producto
        public int idproducto { get; set; }
        public Producto? producto { get; set; }
    }
}
