using AppWeb.Models;
using AppWeb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly IRepository<ConfiguracionEmpresa> _configRepo;
        private readonly IRepository<Rol> _rolRepo;
        private readonly IRepository<Usuario> _usuarioRepo;
        private readonly IRepository<Producto> _productoRepo;
        private readonly IRepository<Cliente> _clienteRepo;
        private readonly IRepository<Proveedor> _proveedorRepo;

        public ConfiguracionService(
            IRepository<ConfiguracionEmpresa> configRepo,
            IRepository<Rol> rolRepo,
            IRepository<Usuario> usuarioRepo,
            IRepository<Producto> productoRepo,
            IRepository<Cliente> clienteRepo,
            IRepository<Proveedor> proveedorRepo)
        {
            _configRepo = configRepo;
            _rolRepo = rolRepo;
            _usuarioRepo = usuarioRepo;
            _productoRepo = productoRepo;
            _clienteRepo = clienteRepo;
            _proveedorRepo = proveedorRepo;
        }

        public async Task<ConfiguracionEmpresa> ObtenerOCrearActualAsync()
        {
            var config = await _configRepo.Query()
                .OrderBy(c => c.idConfiguracion)
                .FirstOrDefaultAsync();

            if (config != null) return config;

            config = new ConfiguracionEmpresa();
            await _configRepo.AddAsync(config);
            await _configRepo.SaveChangesAsync();
            return config;
        }

        public async Task GuardarAsync(ConfiguracionEmpresa model)
        {
            var config = await _configRepo.Query()
                .OrderBy(c => c.idConfiguracion)
                .FirstOrDefaultAsync();

            if (config == null)
            {
                model.fechaActualizacion = DateTime.Now;
                await _configRepo.AddAsync(model);
            }
            else
            {
                config.razonSocial = model.razonSocial.Trim();
                config.ruc = model.ruc.Trim();
                config.direccion = model.direccion?.Trim();
                config.telefono = model.telefono?.Trim();
                config.logoUrl = string.IsNullOrWhiteSpace(model.logoUrl) ? null : model.logoUrl.Trim();
                config.moneda = model.moneda.Trim();
                config.igv = model.igv;
                config.serieBoleta = model.serieBoleta.Trim();
                config.serieFactura = model.serieFactura.Trim();
                config.correo = string.IsNullOrWhiteSpace(model.correo) ? null : model.correo.Trim();
                config.mensajePie = string.IsNullOrWhiteSpace(model.mensajePie) ? null : model.mensajePie.Trim();
                config.fechaActualizacion = DateTime.Now;
                _configRepo.Update(config);
            }

            await _configRepo.SaveChangesAsync();
        }

        public async Task<ResumenSistemaDto> ObtenerResumenSistemaAsync()
        {
            return new ResumenSistemaDto(
                await _rolRepo.Query().CountAsync(r => r.estado),
                await _usuarioRepo.Query().CountAsync(u => u.estado),
                await _productoRepo.Query().CountAsync(p => p.estado),
                await _clienteRepo.Query().CountAsync(c => c.estado),
                await _proveedorRepo.Query().CountAsync(p => p.estado)
            );
        }
    }
}
