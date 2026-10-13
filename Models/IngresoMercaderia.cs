using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class IngresoMercaderia
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idIngreso { get; set; }
        [Required]
        public DateTime fechaIngreso { get; set; }
        [Required, StringLength(50)]
        public string estado { get; set; }
        [StringLength(255)]
        public string observaciones { get; set; }

        //public int idOrdenCompra { get; set; }
        //public int idProveedor { get; set; }
        //public int idAlmacen { get; set; }
    }
}
