using AppWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWeb.ViewModels
{
    public class UsuarioVM
    {

        public string nombreUsuario { get; set; }

        public string password { get; set; }

        public string reppassword { get; set; }

        public string correo { get; set; }

        public string telefono { get; set; }
        /*
        public bool estado { get; set; }*/

        //Relacion de M : 1 con Rol
        public int IDROL { get; set; }
        public Rol Rol { get; set; }
    }
}
