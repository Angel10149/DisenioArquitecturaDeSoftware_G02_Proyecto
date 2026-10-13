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

        //public int idCliente { get; set; }
        //public int idUsuario { get; set; }
        //public int idCotizacion { get; set; }
    }
}