using AppWeb.Models;

namespace AppWeb.Services
{
    public interface IVentaService
    {
        Task<IEnumerable<Venta>> ListarAsync(string? buscar, string? estado);
        Task<Venta?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Cliente>> ListarClientesActivosAsync();
        Task<IEnumerable<Cotizacion>> ListarCotizacionesAprobadasAsync();
    }
}
