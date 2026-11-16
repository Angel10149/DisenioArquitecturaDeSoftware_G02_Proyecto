using AppWeb.Models;
using AppWeb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly IRepository<ConfiguracionEmpresa> _configRepo;

        public ConfiguracionService(IRepository<ConfiguracionEmpresa> configRepo)
        {
            _configRepo = configRepo;
        }

        public async Task<ConfiguracionEmpresa?> ObtenerActualAsync()
        {
            return await _configRepo.Query()
                .OrderBy(c => c.idConfiguracion)
                .FirstOrDefaultAsync();
        }

        public async Task GuardarAsync(ConfiguracionEmpresa configuracion)
        {
            configuracion.fechaActualizacion = DateTime.Now;
            if (configuracion.idConfiguracion == 0)
            {
                await _configRepo.AddAsync(configuracion);
            }
            else
            {
                _configRepo.Update(configuracion);
            }
            await _configRepo.SaveChangesAsync();
        }
    }
}
