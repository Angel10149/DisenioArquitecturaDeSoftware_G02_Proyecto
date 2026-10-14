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
        public int IDINGRESO { get; set; }
        public IngresoMercaderia IngresoMercaderia { get; set; }

        //Relacion de M : 1 con Producto
        public int IDPRODUCTO { get; set; }
        public Producto Producto { get; set; }
    }
}