using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Rol
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idRol { get; set; }
        [Required, StringLength(50)]
        public string nameRol { get; set; } = string.Empty;
        [Required, StringLength(200)]
        public string descripcion { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Estado")]
        public bool estado { get; set; } = true;

        //Relaciones 1 : M con Usuarios
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
