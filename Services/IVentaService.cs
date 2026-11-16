using AppWeb.Models;

namespace AppWeb.Services
{
    public record VentaOperacionResultado(bool Ok, string? Error, int? IdVenta);

    public interface IVentaService
    {
        Task<IEnumerable<Venta>> ListarAsync(string? buscar, string? estado);
        Task<Venta?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Cliente>> ListarClientesActivosAsync();
        Task<IEnumerable<Producto>> ListarProductosActivosAsync();
        Task<IEnumerable<Cotizacion>> ListarCotizacionesAprobadasAsync();
        Task<Cotizacion?> ObtenerCotizacionAprobadaConDetalleAsync(int idCotizacion);
        Task<VentaOperacionResultado> CrearVentaAsync(int idCliente, int idUsuario, int? idCotizacion, bool aplicarIgv, List<DetalleVenta> detalles);
        Task<VentaOperacionResultado> AnularVentaAsync(int idVenta);

        Task<IEnumerable<Cliente>> ListarClientesAsync(string? buscar);
        Task<Cliente?> ObtenerClientePorIdAsync(int id);
        Task<bool> ExisteDocumentoClienteAsync(string numeroDocumento, int? excluirId = null);
        Task CrearClienteAsync(Cliente cliente);
        Task<bool> ActualizarClienteAsync(int id, Cliente cliente);
        Task<Cliente?> CambiarEstadoClienteAsync(int id);

        Task<IEnumerable<Cotizacion>> ListarCotizacionesAsync(string? buscar, string? estado);
        Task<Cotizacion?> ObtenerCotizacionDetalleAsync(int id);
        Task CrearCotizacionAsync(Cotizacion cotizacion);
        Task<VentaOperacionResultado> CambiarEstadoCotizacionAsync(int id, string nuevoEstado);
    }
}
