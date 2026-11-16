using AppWeb.Models;

namespace AppWeb.Services
{
    public interface IReporteService
    {
        Task<IEnumerable<Venta>> ListarVentasPorFechaAsync(DateTime inicio, DateTime finInclusivo);
        Task<IEnumerable<IngresoMercaderia>> ListarIngresosPorFechaAsync(DateTime inicio, DateTime finInclusivo);
        Task<IEnumerable<Pago>> ListarPagosPorFechaAsync(DateTime inicio, DateTime finInclusivo);
        Task<IEnumerable<Producto>> ListarProductosStockBajoAsync();
    }
}
