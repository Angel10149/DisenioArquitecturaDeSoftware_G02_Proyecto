using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class DetalleIngreso
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleIngreso { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad recibida debe ser mayor a 0")]
        public int cantidadRecibida { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad rechazada no puede ser negativa")]
        public int cantidadRechazada { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [Range(0,(double)decimal.MaxValue, ErrorMessage = "El costo unitario debe ser mayor a 0")]
        public decimal costoUnitario { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El subtotal debe ser mayor a 0")]
        public decimal subtotal { get; set; }


        //Relacion de M : 1 con IngresoMercaderia
        public int idIngresoMercaderia { get; set; }
        public IngresoMercaderia? ingresoMercaderia { get; set; }

        //Relacion de M : 1 con Producto
        public int idProducto { get; set; }
        public Producto? producto { get; set; }
    }
}