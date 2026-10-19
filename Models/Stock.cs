using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Stock
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idStock { get; set; }
        [Required]
        public int cantidad { get; set; }
        [Required]
        public DateTime fecha_actualizacion { get; set; }

        //Relacion de M : 1 con Producto
        public int idProducto { get; set; }
        public Producto producto { get; set; }
    }
}
