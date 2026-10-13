using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace AppWeb.Models
{
    public class OrdenCompra
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idOrdenCompra { get; set; }
        [Required]
        public DateTime fechaCompra { get; set; }
        [Required]
        public String estado { get; set; }
        [Required]
        public decimal total { get; set; }


        //public int idProveedor { get; set; }
        //public int id_usuario { get; set; }
        //public int idRequerimiento { get; set; }
    }
}
