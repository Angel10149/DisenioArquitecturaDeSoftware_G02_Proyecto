using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppWeb.Validations;

namespace AppWeb.Models
{
    public class Stock
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idStock { get; set; }

        [Required(ErrorMessage = "La cantidad en stock es obligatoria")]
        [NumeroNoNegativo(ErrorMessage = "La cantidad en stock no puede ser negativa")]
        [Display(Name = "Cantidad en Stock")]
        public int cantidad { get; set; }

        [Required(ErrorMessage = "La fecha de actualización es obligatoria")]
        [Display(Name = "Fecha de Actualización")]
        public DateTime fecha_actualizacion { get; set; }

        // Relacion de M : 1 con Producto
        [Required(ErrorMessage = "El producto es obligatorio")]
        [NumeroPositivo(ErrorMessage = "Debe asociar un producto válido")]
        public int idProducto { get; set; }
        public Producto? producto { get; set; }
    }
}
