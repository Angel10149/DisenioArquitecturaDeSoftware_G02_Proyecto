using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppWeb.Validations;

namespace AppWeb.Models
{
    public class DetalleOrdenCompra
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleOrden { get; set; }

        [Required]
        [NumeroPositivo(ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int cantidad { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [NumeroNoNegativo(ErrorMessage = "El precio unitario no puede ser negativo")]
        public decimal precioUnitario { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [NumeroNoNegativo(ErrorMessage = "El subtotal no puede ser negativo")]
        public decimal subtotal { get; set; }

        //Relacion de M : 1 con OrdenCompra
        public int idOrdenCompra { get; set; }
        public OrdenCompra? ordenCompra { get; set; }

        //Relacion de M : 1 con Producto
        public int idProducto { get; set; }
        public Producto? producto { get; set; }
    }
}
