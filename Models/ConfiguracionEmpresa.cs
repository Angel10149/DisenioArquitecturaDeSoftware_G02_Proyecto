using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class ConfiguracionEmpresa
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idConfiguracion { get; set; }

        [Required(ErrorMessage = "La razón social es obligatoria"), StringLength(150)]
        [RegularExpression(@"^(?=.*[a-zA-ZáéíóúÁÉÍÓÚñÑ])[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s\.\-&]+$", ErrorMessage = "La razón social debe contener letras")]
        public string razonSocial { get; set; } = String.Empty;

        [Required(ErrorMessage = "El RUC es obligatorio."), StringLength(11)]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "El RUC debe contener exactamente 11 dígitos")]
        public string ruc { get; set; } = String.Empty;

        [StringLength(200)]
        public string? direccion { get; set; } = String.Empty;

        [StringLength(20)]
        [Phone(ErrorMessage = "El número de teléfono no es válido")]
        [RegularExpression(@"^\+?(?=(?:.*\d){7,15}$)[0-9\-\s]+$", ErrorMessage = "Ingrese un teléfono válido (7 a 15 dígitos, ej. +51 956521578)")]
        public string? telefono { get; set; } = String.Empty;

        [StringLength(500)]
        [RegularExpression(@"^(https?://.+|/pictures/.+)$", ErrorMessage = "Ingrese una URL válida (http/https) o una ruta local en /pictures/")]
        public string? logoUrl { get; set; }


        [Required, StringLength(10)]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "La moneda debe contener exactamente 3 letras mayúsculas")]
        public string moneda { get; set; } = String.Empty;

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 30, ErrorMessage = "El IGV debe estar entre 0% y 30%")]
        public decimal igv { get; set; } = 18m;

        [Required, StringLength(10)]
        [RegularExpression(@"^B[0-9]{3}$", ErrorMessage = "La serie de boleta debe tener formato B001")]
        public string serieBoleta { get; set; } = string.Empty;

        [Required, StringLength(10)]
        [RegularExpression(@"^F[0-9]{3}$", ErrorMessage = "La serie de factura debe tener formato F001")]
        public string serieFactura { get; set; } = string.Empty;

        [StringLength(120)]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string? correo { get; set; }

        [StringLength(250)]
        public string? mensajePie { get; set; }

        public DateTime fechaActualizacion { get; set; } = DateTime.Now;
    }
}
