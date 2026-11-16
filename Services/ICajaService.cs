using AppWeb.Models;

namespace AppWeb.Services
{
    public record CobroResultado(bool Ok, string? Error, int? IdPago, string? SerieNumero);

    public interface ICajaService
    {
        Task<IEnumerable<Pago>> ListarPagosAsync(string? buscar, string? metodoPago);
        Task<IEnumerable<Venta>> ListarVentasPendientesAsync();
        Task<Venta?> ObtenerVentaPendientePorIdAsync(int idVenta);
        Task<Pago?> ObtenerComprobantePorPagoAsync(int idPago);
        Task<CobroResultado> RegistrarCobroAsync(int idVenta, int idUsuario, string metodoPago, decimal montoRecibido, string tipoComprobante);
    }
}
