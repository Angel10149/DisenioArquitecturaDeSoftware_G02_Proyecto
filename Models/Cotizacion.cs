using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Cotizacion
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idCotizacion { get; set; }
        [Required]
        public DateTime fechaEmision { get; set; }
        [Required, StringLength(30)]
        public string estado { get; set; }
        [StringLength(255)]
        public string observaciones { get; set; }
        [Required]
        public decimal total { get; set; }

        //Relacion de M : 1 con Cliente
        public int idCliente { get; set; }
        public Cliente cliente { get; set; }

        //Relacion de M : 1 con Usuario
        public int idUsuario { get; set; }
        public Usuario usuario { get; set; }

        //Relaciones 1 : M con DetalleCotizacion
        public ICollection<DetalleCotizacion> DetalleCotizacions { get; set; }

        //Relaciones 1 : M con Venta
        public ICollection<Venta> Ventas { get; set; }
    }
}