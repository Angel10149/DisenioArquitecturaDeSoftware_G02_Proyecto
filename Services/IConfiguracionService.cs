using AppWeb.Models;

namespace AppWeb.Services
{
    public interface IConfiguracionService
    {
        Task<ConfiguracionEmpresa?> ObtenerActualAsync();
        Task GuardarAsync(ConfiguracionEmpresa configuracion);
    }
}
