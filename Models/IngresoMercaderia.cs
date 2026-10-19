using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class IngresoMercaderia
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idIngresoMercaderia { get; set; }
        [Required]
        public DateTime fechaIngreso { get; set; }
        [Required, StringLength(50)]
        public string estado { get; set; }
        [StringLength(255)]
        public string observaciones { get; set; }

        //relacion de m : 1 con OrdenCompra
        public int idOrdenCompra { get; set; }
        public OrdenCompra ordenCompra { get; set; }

        //relacion de m : 1 con Proveedor
        public int idProveedor { get; set; }
        public Proveedor proveedor { get; set; }

        //Relaciones 1 : M con DetalleIngreso
        public ICollection<DetalleIngreso> DetalleIngresos { get; set; }
    }
}
