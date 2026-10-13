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
        [Required, StringLength(150)]
        public string nombre { get; set; }
        [StringLength(300)]
        public string descripcion { get; set; }
        [Required]
        public int stockActual { get; set; }
        [Required]
        public int stockMinimo { get; set; }
        [Required]
        public decimal precioCosto { get; set; }
        [Required]
        public decimal precioVenta { get; set; }
        /*[Required]
        public bool estado { get; set; }*/

        //Relacion de M : 1 con Categoria
        public int IDCATEGORIA { get; set; }
        public Categoria Categoria { get; set; }

        //Relacion de M : 1 con UnidadMedida
        public int IDUNIDADMEDIDA { get; set; }
        public UnidadMedida UnidadMedida { get; set; }
    }
}