using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required, StringLength(50)]
        public string nombreUsuario { get; set; }
        [Required, StringLength(50)]
        public string password { get; set; }
        [Required, StringLength(50)]
        public string correo { get; set; }
        [Required, StringLength(15)]
        public string telefono { get; set; }
        /*[Required]
        public bool estado { get; set; }*/

        //Relacion de M : 1 con Rol
        public int IDROL { get; set; }
        public Rol Rol { get; set; }

        //Relaciones 1 : M con Cotizacion
        public ICollection<Cotizacion> Cotizacions { get; set; }

        //Relaciones 1 : M con OrdenCompra
        public ICollection<OrdenCompra> OrdenesCompra { get; set; }

        //Relaciones 1 : M con Pago
        public ICollection<Pago> Pagos { get; set; }

        //Relaciones 1 : M con RequerimientoCompra
        public ICollection<RequerimientoCompra> RequerimientoCompras { get; set; }

        //Relaciones 1 : M con Venta
        public ICollection<Venta> Ventas { get; set; }
    }
}
