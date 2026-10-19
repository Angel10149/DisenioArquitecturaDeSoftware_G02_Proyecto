using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Venta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idVenta { get; set; }
        [Required]
        public DateTime fechaVenta { get; set; }
        [Required]
        public string estado { get; set; }
        [Required]
        public decimal total { get; set; }

        //Relacion de M : 1 con Cliente
        public int idCliente { get; set; }
        public Cliente cliente { get; set; }

        //Relacion de M : 1 con Usuario
        public int idUsuario { get; set; }
        public Usuario usuario { get; set; }

        //Relacion de M : 1 con Cotizacion
        public int idCotizacion { get; set; }
        public Cotizacion cotizacion { get; set; }

        //Relaciones 1 : M con DetalleVenta
        public ICollection<DetalleVenta> DetalleVentas { get; set; }

        //Relaciones 1 : M con Pago
        public ICollection<Pago> Pagos { get; set; }
    }
}