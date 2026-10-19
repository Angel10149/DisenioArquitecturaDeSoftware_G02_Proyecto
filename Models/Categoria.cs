using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Categoria
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int idCategoria {  get; set; }
        [Required, StringLength(50)]
        public String nombre { get; set; }
        [Required, StringLength(255)]
        public String descripcion { get; set; }
        [Required]
        public bool estado {  get; set; }


        //Relaciones 1 : M con Producto
        public ICollection<Producto> Productos { get; set; }
    }
}
