using AppWeb.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Proveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idProveedor { get; set; }

        [CampoObligatorio, StringLength(11), RucPeruano]
        public string ruc { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(150), RazonSocial]
        public string razonSocial { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(100)]
        [SoloLetras(permitirPunto: true, ErrorMessage = "El contacto solo puede contener letras y espacios")]
        public string contacto { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(20), TelefonoPeruano]
        public string telefono { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(255)]
        public string direccion { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(100), CorreoElectronico]
        public string email { get; set; } = String.Empty;

        [Required]
        public bool estado { get; set; } = true;

        //Relaciones 1 : M con IngresoMercaderia
        public ICollection<IngresoMercaderia> IngresosMercaderia { get; set; } = new List<IngresoMercaderia>();


        //Relaciones 1 : M con OrdenCompra
        public ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();
    }
}
