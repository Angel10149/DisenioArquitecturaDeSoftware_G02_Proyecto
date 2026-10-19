using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace AppWeb.Models
{
    public class Comprobante
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idComprobante { get; set; }
        [Required, StringLength(30)]
        public string tipoComprobante { get; set; }
        [Required, StringLength(10)]
        public string serie { get; set; }
        [Required, StringLength(20)]
        public string numero { get; set; }
        [Required, StringLength(255)]
        public string archivoPdf { get; set; }
        [Required]
        public bool estado { get; set; }
      
        public int idPago {  get; set; }
        public Pago pago { get; set; }
    }
}
