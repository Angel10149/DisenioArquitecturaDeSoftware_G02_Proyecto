using AppWeb.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class ConfiguracionEmpresa
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idConfiguracion { get; set; }

        [CampoObligatorio(ErrorMessage = "La razón social es obligatoria"), StringLength(150), RazonSocial]
        public string razonSocial { get; set; } = String.Empty;

        [CampoObligatorio(ErrorMessage = "El RUC es obligatorio"), StringLength(11), RucPeruano]
        public string ruc { get; set; } = String.Empty;

        [StringLength(200)]
        public string? direccion { get; set; } = String.Empty;

        [StringLength(20), TelefonoPeruano]
        public string? telefono { get; set; } = String.Empty;

        [StringLength(500), LogoRutaOUrl]
        public string? logoUrl { get; set; }

        [CampoObligatorio, StringLength(10)]
        [RegularExpression(ValidationPatterns.MonedaIso, ErrorMessage = "La moneda debe contener exactamente 3 letras mayúsculas")]
        public string moneda { get; set; } = String.Empty;

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 30, ErrorMessage = "El IGV debe estar entre 0% y 30%")]
        public decimal igv { get; set; } = 18m;

        [CampoObligatorio, StringLength(10)]
        [RegularExpression(ValidationPatterns.SerieBoleta, ErrorMessage = "La serie de boleta debe tener formato B001")]
        public string serieBoleta { get; set; } = string.Empty;

        [CampoObligatorio, StringLength(10)]
        [RegularExpression(ValidationPatterns.SerieFactura, ErrorMessage = "La serie de factura debe tener formato F001")]
        public string serieFactura { get; set; } = string.Empty;

        [StringLength(120), CorreoElectronico]
        public string? correo { get; set; }

        [StringLength(250)]
        public string? mensajePie { get; set; }

        public DateTime fechaActualizacion { get; set; } = DateTime.Now;
    }
}
