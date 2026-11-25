using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class RequerimientoCompra
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idRequerimiento { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad sugerida no puede ser negativa")]
        public int cantidadSugerida { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad requerida debe ser mayor a 0")]
        public int cantidadRequerida { get; set; }

        [Required]
        public DateOnly fechaRequerida { get; set; }

        [Required, StringLength(20)]
        [RegularExpression(@"^(Baja|Media|Alta)$", ErrorMessage = "La prioridad debe ser Baja, Media o Alta")]
        public string prioridad { get; set; } = "Media";

        [Required, StringLength(30)]
        public string estado { get; set; } = "Pendiente";

        [StringLength(255)]
        public string? motivo { get; set; }

        [Required]
        public DateTime fechaCreacion { get; set; }

        //Relacion de M : 1 con Producto
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido")]
        public int idProducto { get; set; }
        public Producto? producto { get; set; }

        //Relacion de M : 1 con Usuario
        public int idUsuario { get; set; }
        public Usuario? usuario { get; set; }

        //Relaciones 1 : M con OrdenCompra
        public ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();
    }
}