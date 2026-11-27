using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Categoria
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int idCategoria { get; set; }

        [Required, StringLength(50)]
        [RegularExpression(@"^(?=.*[a-zA-ZáéíóúÁÉÍÓÚñÑ])[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s\-]+$",
    ErrorMessage = "El nombre debe contener letras y no puede ser solo números o símbolos")]
        public string nombre { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public String descripcion { get; set; } = string.Empty;

        [Required]
        public bool estado { get; set; } = true;


        //Relaciones 1 : M con Producto
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
