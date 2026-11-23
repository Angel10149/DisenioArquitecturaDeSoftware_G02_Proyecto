using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Proveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idProveedor { get; set; }

        [Required, StringLength(11)]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "El RUC debe contener exactamente 11 dígitos.")]
        public string ruc { get; set; } = String.Empty;

        [Required, StringLength(150)]
        public string razonSocial { get; set; } = String.Empty;

        [Required, StringLength(100)]
        public string contacto { get; set; } = String.Empty;

        [Required, StringLength(20)]
        [Phone(ErrorMessage = "El número de teléfono no es válido")]
        public string telefono { get; set; } = String.Empty;

        [Required, StringLength(255)]
        public string direccion { get; set; } = String.Empty;

        [Required, StringLength(100)]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string email { get; set; } = String.Empty;

        [Required]
        public bool estado { get; set; } = true;

        //Relaciones 1 : M con IngresoMercaderia
        public ICollection<IngresoMercaderia> IngresosMercaderia { get; set; } = new List<IngresoMercaderia>();


        //Relaciones 1 : M con OrdenCompra
        public ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();
    }
}
