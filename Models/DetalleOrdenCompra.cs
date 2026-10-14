using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class DetalleOrdenCompra
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleOrden { get; set; }
        [Required]
        public int cantidad { get; set; }
        [Required]
        public decimal precioUnitario { get; set; }
        [Required]
        public decimal subtotal { get; set; }

        //Relacion de M : 1 con OrdenCompra
        public int IDORDENCOMPRA { get; set; }
        public OrdenCompra OrdenCompra { get; set; }

        //Relacion de M : 1 con Producto
        public int IDPRODUCTO { get; set; }
        public Producto Producto { get; set; }
    }
}