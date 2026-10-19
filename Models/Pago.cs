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
        [Required, StringLength(50)]
        public string metodoPago { get; set; }
        [Required]
        public decimal montoRecibido { get; set; }
        [Required, StringLength(30)]
        public string estado { get; set; }

        //Relacion de M : 1 con Venta
        public int idVenta { get; set; }
        public Venta venta { get; set; }

        //Relacion de M : 1 con Usuario
        public int idUsuario { get; set; }
        public Usuario usuario { get; set; }

        //Relaciones 1 : M con Comprobante
        public ICollection<Comprobante> Comprobantes { get; set; }

    }
}