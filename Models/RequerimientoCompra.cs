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
        public DateTime fechaRequerida { get; set; }
        [Required, StringLength(20)]
        public string prioridad { get; set; }
        [Required, StringLength(20)]
        public string estado { get; set; }
        [StringLength(300)]
        public string motivo { get; set; }
        public DateTime fechaCreacion { get; set; }

        //Relacion de M : 1 con Producto
        public int IDPRODUCTO { get; set; }
        public Producto Producto { get; set; }

        //Relacion de M : 1 con Usuario
        public int IDUSUARIO { get; set; }
        public Usuario Usuario { get; set; }
    }
}