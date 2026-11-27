using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class Producto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idProducto { get; set; }

        [Required, StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9\-_]+$", ErrorMessage = "El código solo puede contener letras, números o guiones")]
        public string codigoBarras { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [RegularExpression(@"^(?=.*[a-zA-ZáéíóúÁÉÍÓÚñÑ]).+$", ErrorMessage = "El nombre del producto debe incluir letras")]
        public string nombre { get; set; } = string.Empty;

        [StringLength(255)]
        public string? descripcion { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock actual no puede ser negativo")]
        public int stockActual { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int stockMinimo { get; set; }
        [Required, Column(TypeName = "decimal(18, 2)")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El precio de costo no puede ser negativo")]
        public decimal precioCosto { get; set; }
        [Required, Column(TypeName = "decimal(18, 2)")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "El precio de venta no puede ser negativo")]
        public decimal precioVenta { get; set; }
        [Required]
        public bool estado { get; set; } = true;

        //Relacion de M : 1 con Categoria
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida")]
        public int idCategoria { get; set; }
        public Categoria? categoria { get; set; }

        //Relacion de M : 1 con UnidadMedida
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una unidad de medida válida")]
        public int idUnidadMedida { get; set; }
        public UnidadMedida? unidadMedida { get; set; }

        //Relaciones 1 : M con DetalleCotizacion
        public ICollection<DetalleCotizacion> DetalleCotizacions { get; set; } = new List<DetalleCotizacion>();

        //Relaciones 1 : M con DetalleIngreso
        public ICollection<DetalleIngreso> DetalleIngresos { get; set; } = new List<DetalleIngreso>();

        //Relaciones 1 : M con DetalleOrdenCompra
        public ICollection<DetalleOrdenCompra> DetalleOrdenCompras { get; set; } = new List<DetalleOrdenCompra>();

        //Relaciones 1 : M con DetalleVenta
        public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();

        //Relaciones 1 : M con RequerimientoCompra
        public ICollection<RequerimientoCompra> RequerimientoCompras { get; set; } = new List<RequerimientoCompra>();

        //Relaciones 1 : M con Stock
        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    }
}