using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace AppWeb.Models
{
    public class Cliente : IValidatableObject
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idCliente { get; set; }
        [Required, StringLength(20)]
        [RegularExpression(@"^(DNI|RUC|CE|Pasaporte)$", ErrorMessage = "El tipo de documento debe ser DNI, RUC, CE o Pasaporte.")]
        public String tipoDocumento { get; set; } = String.Empty;

        [Required, StringLength(20)]
        public String numeroDocumento { get; set; } = String.Empty;

        [Required, StringLength(100)]
        public String nombres { get; set; } = String.Empty;
        [Required, StringLength(150)]
        public String razonSocial { get; set; } = String.Empty;
        [Required, StringLength(20)]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string telefono { get; set; } = String.Empty;
        [Required, StringLength(255)]
        public String direccion { get; set; } = String.Empty;
        [Required, StringLength(100)]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
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
            if (tipoDocumento == "DNI" && !Regex.IsMatch(doc, @"^\d{8}$"))
            {
                yield return new ValidationResult(
                    "El DNI debe contener exactamente 8 dígitos numéricos.",
                    new[] { nameof(numeroDocumento) });
            }
            else if (tipoDocumento == "RUC" && !Regex.IsMatch(doc, @"^\d{11}$"))
            {
                yield return new ValidationResult(
                    "El RUC debe contener exactamente 11 dígitos numéricos.",
                    new[] { nameof(numeroDocumento) });
            }
            else if ((tipoDocumento == "CE" || tipoDocumento == "Pasaporte") && !Regex.IsMatch(doc, @"^[A-Za-z0-9]{6,20}$"))
            {
                yield return new ValidationResult(
                    $"El {tipoDocumento} debe tener entre 6 y 20 caracteres alfanuméricos.",
                    new[] { nameof(numeroDocumento) });
            }
        }
    }
}
