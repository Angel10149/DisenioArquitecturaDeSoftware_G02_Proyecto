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
        [Required, StringLength(20)]
        public string estado { get; set; }
        [StringLength(300)]
        public string observaciones { get; set; }
        [Required]
        public decimal total { get; set; }

        //Relacion de M : 1 con Cliente
        public int IDCLIENTE { get; set; }
        public Cliente Cliente { get; set; }

        //Relacion de M : 1 con Usuario
        public int IDUSUARIO { get; set; }
        public Usuario Usuario { get; set; }
    }
}