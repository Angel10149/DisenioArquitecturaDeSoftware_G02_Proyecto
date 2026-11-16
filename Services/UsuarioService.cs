using AppWeb.Models;
using AppWeb.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public class UsuarioService : IUsuarioService
    {
        private const string RolAdministrador = "Administrador";
        private readonly IRepository<Usuario> _usuarioRepo;
        private readonly IRepository<Rol> _rolRepo;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

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

        public async Task<Usuario?> ObtenerPorIdConRolAsync(int id)
        {
            return await _usuarioRepo.Query()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.id == id);
        }

        public async Task<IEnumerable<Usuario>> ListarConRolAsync()
        {
            return await _usuarioRepo.Query()
                .Include(u => u.Rol)
                .ToListAsync();
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

        public async Task<IEnumerable<Rol>> ObtenerRolesEdicionAsync(int idRolActual)
        {
            return await _rolRepo.Query()
                .Where(r => r.estado || r.idRol == idRolActual)
                .ToListAsync();
        }

        public async Task<Rol?> ObtenerRolPorIdAsync(int idRol)
        {
            return await _rolRepo.Query().FirstOrDefaultAsync(r => r.idRol == idRol);
        }

        public async Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null)
        {
            string correoNormalizado = correo.Trim();
            return await _usuarioRepo.Query()
                .AnyAsync(u => u.correo == correoNormalizado && (!excluirId.HasValue || u.id != excluirId.Value));
        }

        public async Task RegistrarAsync(Usuario usuario, string passwordPlano)
        {
            usuario.password = _passwordHasher.HashPassword(usuario, passwordPlano);
            await _usuarioRepo.AddAsync(usuario);
            await _usuarioRepo.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            _usuarioRepo.Update(usuario);
            await _usuarioRepo.SaveChangesAsync();
        }

        public async Task<Usuario?> CambiarEstadoAsync(int id)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(id);
            if (usuario == null) return null;

            usuario.estado = !usuario.estado;
            _usuarioRepo.Update(usuario);
            await _usuarioRepo.SaveChangesAsync();
            return usuario;
        }

        public bool VerificarPassword(Usuario usuario, string passwordPlano, out bool requiereRehash)
        {
            requiereRehash = false;
            try
            {
                var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.password, passwordPlano);
                if (resultado == PasswordVerificationResult.Success)
                {
                    return true;
                }
                if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    requiereRehash = true;
                    return true;
                }
            }
            catch (FormatException)
            {
                // Contraseña heredada en texto plano
            }

            if (usuario.password == passwordPlano)
            {
                requiereRehash = true;
                return true;
            }

            return false;
        }

        public string HashearPassword(Usuario usuario, string passwordPlano) =>
            _passwordHasher.HashPassword(usuario, passwordPlano);
    }
}
