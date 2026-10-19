using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace AppWeb.Models
{
    public class OrdenCompra
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idOrdenCompra { get; set; }
        [Required]
        public DateTime fechaCompra { get; set; }
        [Required]
        public String estado { get; set; }
        [Required]
        public decimal total { get; set; }

        //relacion de m : 1 con OrdenCompra
        public int idProveedor { get; set; }
        public Proveedor proveedor { get; set; }

        //relacion de m : 1 con OrdenCompra
        public int idUsuario { get; set; }
        public Usuario usuario { get; set; }

        //relacion de m : 1 con OrdenCompra
        public int idRequerimiento { get; set; }
        public RequerimientoCompra requerimiento { get; set; }

        //Relaciones 1 : M con DetalleOrdenCompra
        public ICollection<DetalleOrdenCompra> DetalleOrdenCompras { get; set; }

        //Relaciones 1 : M con IngresoMercaderia
        public ICollection<IngresoMercaderia> IngresosMercaderia { get; set; }
    }
}
