using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Cliente
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idCliente { get; set; }
        [Required, StringLength(20)]
        public String tipoDocumento { get; set; }
        [Required, StringLength(20)]
        public String numeroDocumento { get; set; }
        [Required, StringLength(100)]
        public String nombres { get; set; }
        [Required, StringLength(150)]
        public String razonSocial { get; set; }
        [Required, StringLength(20)]
        public string telefono { get; set; }
        [Required, StringLength(255)]
        public String direccion { get; set; }
        [Required, StringLength(100)]
        public String email { get; set; }
        [Required]
        public bool estado { get; set; }

    }
}
