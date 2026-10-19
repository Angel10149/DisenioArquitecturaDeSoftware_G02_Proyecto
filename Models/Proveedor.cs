using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Proveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idProveedor { get; set; }
        [Required, StringLength(20)]
        public string ruc { get; set; }
        [Required, StringLength(150)]
        public string razonSocial { get; set; }
        [Required, StringLength(100)]
        public string contacto { get; set; }
        [Required, StringLength(20)]
        public string telefono { get; set; }
        [Required, StringLength(255)]
        public string direccion { get; set; }
        [Required, StringLength(100)]
        public string email { get; set; }
        [Required]
        public bool estado { get; set; }

        //Relaciones 1 : M con IngresoMercaderia
        public ICollection<IngresoMercaderia> IngresosMercaderia { get; set; }


        //Relaciones 1 : M con OrdenCompra
        public ICollection<OrdenCompra> OrdenesCompra { get; set; }
    }
}
