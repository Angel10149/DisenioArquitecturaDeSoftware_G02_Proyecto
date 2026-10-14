using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Pago
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idPago { get; set; }
        [Required]
        public DateTime fechaPago { get; set; }
        [Required, StringLength(30)]
        public string metodoPago { get; set; }
        [Required]
        public decimal montoRecibido { get; set; }
        [Required, StringLength(20)]
        public string estado { get; set; }

        //Relacion de M : 1 con Venta
        public int IDVENTA { get; set; }
        public Venta Venta { get; set; }

        //Relacion de M : 1 con Usuario
        public int IDUSUARIO { get; set; }
        public Usuario Usuario { get; set; }
    }
}