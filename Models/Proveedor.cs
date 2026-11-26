using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Proveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idProveedor { get; set; }

        [Required, StringLength(11)]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "El RUC debe contener exactamente 11 dígitos")]
        public string ruc { get; set; } = String.Empty;

        [Required, StringLength(150)]
        [RegularExpression(@"^(?=.*[a-zA-ZáéíóúÁÉÍÓÚñÑ])[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s\.\-&]+$", ErrorMessage = "La razón social debe contener letras")]
        public string razonSocial { get; set; } = String.Empty;

        [Required, StringLength(100)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\.]+$", ErrorMessage = "El contacto solo puede contener letras y espacios")]
        public string contacto { get; set; } = String.Empty;

        [Required, StringLength(20)]
        [Phone(ErrorMessage = "El número de teléfono no es válido")]
        [RegularExpression(@"^\+?(?=(?:.*\d){7,15}$)[0-9\-\s]+$", ErrorMessage = "Ingrese un teléfono válido (7 a 15 dígitos, ej. +51 956521578)")]
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
