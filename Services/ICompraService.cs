using AppWeb.Models;

namespace AppWeb.Services
{
    public interface ICompraService
    {
        Task<IEnumerable<Proveedor>> ListarProveedoresActivosAsync();
        Task<IEnumerable<RequerimientoCompra>> ListarRequerimientosAsync();
        Task<IEnumerable<OrdenCompra>> ListarOrdenesCompraAsync();
        Task<IEnumerable<IngresoMercaderia>> ListarIngresosAsync();
    }
}
