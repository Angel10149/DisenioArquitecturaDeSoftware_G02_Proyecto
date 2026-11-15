using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class ConfiguracionEmpresa
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idConfiguracion { get; set; }

        [Required(ErrorMessage = "La razón social es obligatoria."), StringLength(150)]
        public string razonSocial { get; set; } = "MKG Importaciones S.A.C.";

        [Required(ErrorMessage = "El RUC es obligatorio."), StringLength(11)]
        public string ruc { get; set; } = "20601234567";

        [StringLength(200)]
        public string? direccion { get; set; } = "Cajamarca, Perú";

        [StringLength(20)]
        public string? telefono { get; set; } = "076-555-0100";

        [StringLength(500)]
        public string? logoUrl { get; set; } = "https://i.postimg.cc/FRcSYwJ6/Logo-MKG.webp";

        [Required, StringLength(10)]
        public string moneda { get; set; } = "PEN";

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 30, ErrorMessage = "El IGV debe estar entre 0% y 30%.")]
        public decimal igv { get; set; } = 18m;

        [Required, StringLength(10)]
        public string serieBoleta { get; set; } = "B001";

        [Required, StringLength(10)]
        public string serieFactura { get; set; } = "F001";

        [StringLength(120)]
        public string? correo { get; set; }

        [StringLength(250)]
        public string? mensajePie { get; set; }

        public DateTime fechaActualizacion { get; set; } = DateTime.Now;
    }
}
