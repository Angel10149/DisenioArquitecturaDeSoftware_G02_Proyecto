using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class UnidadMedida
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idUnidadMedida { get; set; }
        [Required, StringLength(50)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string nombre { get; set; } = string.Empty;

        [Required, StringLength(10)]
        [RegularExpression(@"^(?=.*[a-zA-Z])[a-zA-Z0-9]{1,10}$", ErrorMessage = "La abreviatura debe incluir letras y solo puede contener letras o números (ej. UNI, KG, M2)")]
        public string abreviatura { get; set; } = string.Empty;

        [Required]
        public bool estado { get; set; } = true;


        //Relaciones 1 : M con Producto
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}

