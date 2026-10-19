using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class UnidadMedida
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idUnidadMedida { get; set; }
        [ Required, StringLength (50)]
        public string nombre { get; set; }
        [Required, StringLength(10)]
        public string abreviatura { get; set; }
        [Required]
        public bool estado { get; set; }
    }
}

