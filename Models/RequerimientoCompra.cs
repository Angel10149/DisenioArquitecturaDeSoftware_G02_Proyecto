using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.Models
{
    public class RequerimientoCompra
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idRequerimiento { get; set; }
        [Required]
        public int cantidadSugerida { get; set; }
        [Required]
        public int cantidadRequerida { get; set; }
        [Required]
        public DateOnly fechaRequerida { get; set; }
        [Required, StringLength(20)]
        public string prioridad { get; set; }
        [Required, StringLength(30)]
        public string estado { get; set; }
        [StringLength(255)]
        public string motivo { get; set; }
        [Required]
        public DateTime fechaCreacion { get; set; }

        //Relacion de M : 1 con Producto
        public int idProducto { get; set; }
        public Producto producto { get; set; }

        //Relacion de M : 1 con Usuario
        public int idUsuario { get; set; }
        public Usuario usuario { get; set; }

        //Relaciones 1 : M con OrdenCompra
        public ICollection<OrdenCompra> OrdenesCompra { get; set; }
    }
}