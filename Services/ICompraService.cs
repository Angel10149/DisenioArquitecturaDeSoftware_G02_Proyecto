using AppWeb.Models;

namespace AppWeb.Services
{
    public interface ICompraService
    {
        Task<IEnumerable<Proveedor>> ListarProveedoresActivosAsync();
        Task<IEnumerable<RequerimientoCompra>> ListarRequerimientosAsync();
        Task<IEnumerable<OrdenCompra>> ListarOrdenesCompraAsync();
        Task<IEnumerable<IngresoMercaderia>> ListarIngresosAsync();

        Task<IEnumerable<Proveedor>> ListarProveedoresAsync(string? buscar);
        Task<Proveedor?> ObtenerProveedorPorIdAsync(int id);
        Task<bool> ExisteRucProveedorAsync(string ruc, int? excluirId = null);
        Task CrearProveedorAsync(Proveedor proveedor);
        Task<bool> ActualizarProveedorAsync(int id, Proveedor proveedor);
        Task<Proveedor?> CambiarEstadoProveedorAsync(int id);

        Task<IEnumerable<Producto>> ListarProductosActivosAsync();
        Task<Producto?> ObtenerProductoPorIdAsync(int id);
        Task<IEnumerable<RequerimientoCompra>> ListarRequerimientosFiltradosAsync(string? buscar, string? estado);
        Task<RequerimientoCompra?> ObtenerRequerimientoPorIdAsync(int id);
        Task CrearRequerimientoAsync(RequerimientoCompra requerimiento);
        Task<(bool Ok, string? Error)> ActualizarRequerimientoAsync(int id, RequerimientoCompra requerimiento);
        Task<(bool Ok, string? Error)> CambiarEstadoRequerimientoAsync(int id, string nuevoEstado);

        Task<IEnumerable<RequerimientoCompra>> ListarRequerimientosAprobadosAsync();
        Task<RequerimientoCompra?> ObtenerRequerimientoConProductoAsync(int idRequerimiento);
        Task<IEnumerable<OrdenCompra>> ListarOrdenesCompraFiltradasAsync(string? buscar, string? estado);
        Task<OrdenCompra?> ObtenerOrdenCompraDetalleAsync(int id);
        Task<(bool Ok, string? Error, OrdenCompra? Orden)> CrearOrdenCompraAsync(int idProveedor, int idRequerimiento, int idUsuario, List<DetalleOrdenCompra> detalles);
        Task<(bool Ok, string? Error)> CambiarEstadoOrdenCompraAsync(int id, string nuevoEstado);

        Task<IEnumerable<OrdenCompra>> ListarOrdenesPendientesOEmailAsync();
        Task<OrdenCompra?> ObtenerOrdenParaIngresoAsync(int idOrdenCompra);
        Task<IEnumerable<IngresoMercaderia>> ListarIngresosFiltradosAsync(string? buscar, string? estado);
        Task<IngresoMercaderia?> ObtenerIngresoDetalleAsync(int id);
        Task<(bool Ok, string? Error, int? IdIngreso)> RegistrarIngresoAsync(OrdenCompra orden, string? observaciones, List<DetalleIngreso> detalles);
    }
}
