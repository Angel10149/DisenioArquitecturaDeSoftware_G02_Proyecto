using AppWeb.Models;

namespace AppWeb.Services
{
    public interface IUsuarioService
    {
        Task<bool> ExisteAdministradorActivoAsync();
        Task<Usuario?> ObtenerPorCorreoAsync(string correo);
        Task<Usuario?> ObtenerPorIdConRolAsync(int id);
        Task<IEnumerable<Usuario>> ListarConRolAsync();
        Task<IEnumerable<Rol>> ObtenerRolesDisponiblesRegistroAsync();
        Task<IEnumerable<Rol>> ObtenerRolesEdicionAsync(int idRolActual);
        Task<Rol?> ObtenerRolPorIdAsync(int idRol);
        Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null);
        Task RegistrarAsync(Usuario usuario, string passwordPlano);
        Task ActualizarAsync(Usuario usuario);
        Task<Usuario?> CambiarEstadoAsync(int id);
        bool VerificarPassword(Usuario usuario, string passwordPlano, out bool requiereRehash);
        string HashearPassword(Usuario usuario, string passwordPlano);
    }
}
