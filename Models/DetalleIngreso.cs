using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class DetalleIngreso
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleIngreso { get; set; }
        [Required]
        public int cantidadRecibida { get; set; }
        [Required]
        public int cantidadRechazada { get; set; }
        [Required]
        public decimal costoUnitario { get; set; }
        [Required]
        public decimal subtotal { get; set; }


        //Relacion de M : 1 con IngresoMercaderia
        public int idIngresoMercaderia { get; set; }
        public IngresoMercaderia ingresoMercaderia { get; set; }

        //Relacion de M : 1 con Producto
        public int idProducto { get; set; }
        public Producto producto { get; set; }
    }
}