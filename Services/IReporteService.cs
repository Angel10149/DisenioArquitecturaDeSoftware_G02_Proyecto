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

        Task<decimal> ObtenerTotalVentasRangoAsync(DateTime inicio, DateTime finExclusivo);
        Task<int> ContarProductosActivosAsync();
        Task<int> ContarProductosStockBajoAsync();
        Task<IEnumerable<Producto>> ListarTopProductosStockBajoAsync(int cantidad);
        Task<IEnumerable<DetalleVenta>> ListarDetallesVentaMesAsync(DateTime inicioMes, DateTime finExclusivo);
        Task<IEnumerable<string>> ListarTopNombresProductosVendidosAsync(int cantidad);
        Task<IEnumerable<Producto>> BuscarProductosRapidoAsync(string filtro, int limite);
        Task<IEnumerable<Cliente>> BuscarClientesRapidoAsync(string filtro, int limite);
        Task<IEnumerable<Venta>> BuscarVentasRapidoAsync(string filtro, int limite);
        Task<IEnumerable<Venta>> ListarUltimasVentasAsync(int limite);
    }
}
