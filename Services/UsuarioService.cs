using AppWeb.Models;
using AppWeb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public class UsuarioService : IUsuarioService
    {
        private const string RolAdministrador = "Administrador";
        private readonly IRepository<Usuario> _usuarioRepo;
        private readonly IRepository<Rol> _rolRepo;

        public UsuarioService(IRepository<Usuario> usuarioRepo, IRepository<Rol> rolRepo)
        {
            _usuarioRepo = usuarioRepo;
            _rolRepo = rolRepo;
        }

        public async Task<bool> ExisteAdministradorActivoAsync()
        {
            return await _usuarioRepo.Query()
                .AnyAsync(u => u.estado && u.Rol != null && u.Rol.nameRol == RolAdministrador);
        }

        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
        {
            string correoNormalizado = correo.Trim();
            return await _usuarioRepo.Query()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.correo == correoNormalizado);
        }

        public async Task<IEnumerable<Rol>> ObtenerRolesDisponiblesRegistroAsync()
        {
            bool existeAdmin = await ExisteAdministradorActivoAsync();
            var query = _rolRepo.Query().Where(r => r.estado);
            if (existeAdmin)
            {
                query = query.Where(r => r.nameRol != RolAdministrador);
            }
            return await query.ToListAsync();
        }

        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            string correoNormalizado = correo.Trim();
            return await _usuarioRepo.Query().AnyAsync(u => u.correo == correoNormalizado);
        }

        public async Task RegistrarAsync(Usuario usuario)
        {
            await _usuarioRepo.AddAsync(usuario);
            await _usuarioRepo.SaveChangesAsync();
        }
    }
}
