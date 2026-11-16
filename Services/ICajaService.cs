using AppWeb.Models;

namespace AppWeb.Services
{
    public interface ICajaService
    {
        Task<IEnumerable<Pago>> ListarPagosAsync(string? buscar, string? metodoPago);
        Task<IEnumerable<Venta>> ListarVentasPendientesAsync();
        Task<Venta?> ObtenerVentaPendientePorIdAsync(int idVenta);
    }
}
