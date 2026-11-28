using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppWeb.Validations;

namespace AppWeb.Models
{
    public class RequerimientoCompra
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idRequerimiento { get; set; }

        [Required]
        [NumeroNoNegativo(ErrorMessage = "La cantidad sugerida no puede ser negativa")]
        public int cantidadSugerida { get; set; }

        [Required]
        [NumeroPositivo(ErrorMessage = "La cantidad requerida debe ser mayor a 0")]
        public int cantidadRequerida { get; set; }

        [Required]
        public DateOnly fechaRequerida { get; set; }

        [CampoObligatorio, StringLength(20)]
        [RegularExpression(ValidationPatterns.PrioridadRequerimiento, ErrorMessage = "La prioridad debe ser Baja, Media o Alta")]
        public string prioridad { get; set; } = "Media";

        [CampoObligatorio, StringLength(30)]
        public string estado { get; set; } = "Pendiente";

        [StringLength(255)]
        public string? motivo { get; set; }

        [Required]
        public DateTime fechaCreacion { get; set; }

        //Relacion de M : 1 con Producto
        [NumeroPositivo(ErrorMessage = "Debe seleccionar un producto válido")]
        public int idProducto { get; set; }
        public Producto? producto { get; set; }

        //Relacion de M : 1 con Usuario
        public int idUsuario { get; set; }
        public Usuario? usuario { get; set; }

        //Relaciones 1 : M con OrdenCompra
        public ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();
    }
}
