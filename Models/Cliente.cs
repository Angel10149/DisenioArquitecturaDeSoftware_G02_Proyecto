using AppWeb.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace AppWeb.Models
{
    public class Cliente : IValidatableObject
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idCliente { get; set; }

        [CampoObligatorio, StringLength(20)]
        [RegularExpression(ValidationPatterns.TipoDocumentoCliente, ErrorMessage = "El tipo de documento debe ser DNI, RUC, CE o Pasaporte.")]
        public String tipoDocumento { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(20)]
        public String numeroDocumento { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(100), SoloLetras(permitirPunto: true)]
        public String nombres { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(150), RazonSocial]
        public String razonSocial { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(20), TelefonoPeruano]
        public string telefono { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(255)]
        public String direccion { get; set; } = String.Empty;

        [CampoObligatorio, StringLength(100), CorreoElectronico]
        public String email { get; set; } = String.Empty;

        [Required]
        public bool estado { get; set; } = true;


        //Relaciones 1 : M con Cotizacion
        public ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();

        //Relaciones 1 : M con Venta
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();



        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var doc = numeroDocumento?.Trim() ?? string.Empty;
            if (tipoDocumento == "DNI" && !Regex.IsMatch(doc, ValidationPatterns.Dni))
            {
                yield return new ValidationResult(
                    "El DNI debe contener exactamente 8 dígitos numéricos.",
                    new[] { nameof(numeroDocumento) });
            }
            else if (tipoDocumento == "RUC" && !Regex.IsMatch(doc, ValidationPatterns.Ruc))
            {
                yield return new ValidationResult(
                    "El RUC debe contener exactamente 11 dígitos numéricos.",
                    new[] { nameof(numeroDocumento) });
            }
            else if ((tipoDocumento == "CE" || tipoDocumento == "Pasaporte") && !Regex.IsMatch(doc, ValidationPatterns.CeOPasaporte))
            {
                yield return new ValidationResult(
                    $"El {tipoDocumento} debe tener entre 6 y 20 caracteres alfanuméricos.",
                    new[] { nameof(numeroDocumento) });
            }
        }
    }
}
