using AppWeb.Models;

namespace AppWeb.Services
{
    public interface IUsuarioService
    {
        Task<bool> ExisteAdministradorActivoAsync();
        Task<Usuario?> ObtenerPorCorreoAsync(string correo);
        Task<IEnumerable<Rol>> ObtenerRolesDisponiblesRegistroAsync();
        Task<bool> ExisteCorreoAsync(string correo);
        Task RegistrarAsync(Usuario usuario);
    }
}
