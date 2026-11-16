using AppWeb.Models;

namespace AppWeb.Services
{
    public record ResumenSistemaDto(int TotalRoles, int TotalUsuarios, int TotalProductos, int TotalClientes, int TotalProveedores);

    public interface IConfiguracionService
    {
        Task<ConfiguracionEmpresa> ObtenerOCrearActualAsync();
        Task GuardarAsync(ConfiguracionEmpresa model);
        Task<ResumenSistemaDto> ObtenerResumenSistemaAsync();
    }
}
