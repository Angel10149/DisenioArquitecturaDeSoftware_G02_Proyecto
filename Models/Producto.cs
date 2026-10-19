using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Producto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idProducto { get; set; }
        [Required, StringLength(50)]
        public string codigoBarras { get; set; }
        [Required, StringLength(100)]
        public string nombre { get; set; }
        [StringLength(255)]
        public string descripcion { get; set; }
        [Required]
        public int stockActual { get; set; }
        [Required]
        public int stockMinimo { get; set; }
        [Required]
        public decimal precioCosto { get; set; }
        [Required]
        public decimal precioVenta { get; set; }
        [Required]
        public bool estado { get; set; }

        //Relacion de M : 1 con Categoria
        public int idCategoria { get; set; }
        public Categoria categoria { get; set; } 

        //Relacion de M : 1 con UnidadMedida
        public int idUnidadMedida { get; set; }
        public UnidadMedida unidadMedida { get; set; }

        //Relaciones 1 : M con DetalleCotizacion
        public ICollection<DetalleCotizacion> DetalleCotizacions { get; set; }

        //Relaciones 1 : M con DetalleIngreso
        public ICollection<DetalleIngreso> DetalleIngresos { get; set; }

        //Relaciones 1 : M con DetalleOrdenCompra
        public ICollection<DetalleOrdenCompra> DetalleOrdenCompras { get; set; }

        //Relaciones 1 : M con DetalleVenta
        public ICollection<DetalleVenta> DetalleVentas { get; set; }

        //Relaciones 1 : M con RequerimientoCompra
        public ICollection<RequerimientoCompra> RequerimientoCompras { get; set; }

        //Relaciones 1 : M con Stock
        public ICollection<Stock> Stocks { get; set; }
    }
}