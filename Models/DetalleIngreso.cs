using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppWeb.Validations;

namespace AppWeb.Models
{
    public class DetalleIngreso
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleIngreso { get; set; }

        [Required]
        [NumeroNoNegativo(ErrorMessage = "La cantidad recibida no puede ser negativa")]
        public int cantidadRecibida { get; set; }

        [Required]
        [NumeroNoNegativo(ErrorMessage = "La cantidad rechazada no puede ser negativa")]
        public int cantidadRechazada { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [NumeroNoNegativo(ErrorMessage = "El costo unitario no puede ser negativo")]
        public decimal costoUnitario { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [NumeroNoNegativo(ErrorMessage = "El subtotal no puede ser negativo")]
        public decimal subtotal { get; set; }

        //Relacion de M : 1 con IngresoMercaderia
        public int idIngresoMercaderia { get; set; }
        public IngresoMercaderia? ingresoMercaderia { get; set; }

        //Relacion de M : 1 con Producto
        public int idProducto { get; set; }
        public Producto? producto { get; set; }
    }
}
