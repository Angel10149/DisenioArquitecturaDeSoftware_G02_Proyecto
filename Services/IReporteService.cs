using AppWeb.Models;

namespace AppWeb.Services
{
    public record MovimientoProductoResumen(int Cantidad, decimal Total);

    public interface IReporteService
    {
        Task<IEnumerable<Venta>> ListarVentasPorFechaAsync(DateTime inicio, DateTime finInclusivo);
        Task<IEnumerable<IngresoMercaderia>> ListarIngresosPorFechaAsync(DateTime inicio, DateTime finInclusivo);
        Task<IEnumerable<Pago>> ListarPagosPorFechaAsync(DateTime inicio, DateTime finInclusivo);
        Task<IEnumerable<Producto>> ListarProductosStockBajoAsync();
        Task<IEnumerable<Producto>> ListarProductosActivosAsync();
        Task<Dictionary<int, MovimientoProductoResumen>> ObtenerResumenComprasPorProductoAsync(DateTime inicio, DateTime finInclusivo);
        Task<Dictionary<int, MovimientoProductoResumen>> ObtenerResumenVentasPorProductoAsync(DateTime inicio, DateTime finInclusivo);
    }
}
