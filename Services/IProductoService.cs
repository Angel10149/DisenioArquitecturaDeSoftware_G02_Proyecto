using AppWeb.Models;

namespace AppWeb.Services
{
    public interface IProductoService
    {
        Task<IEnumerable<Producto>> ListarAsync(string? buscar, int? idCategoria);
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Categoria>> ListarCategoriasActivasAsync();
        Task<IEnumerable<UnidadMedida>> ListarUnidadesActivasAsync();
        Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, int? excluirId = null);
        Task CrearConStockInicialAsync(Producto producto);
        Task<bool> ActualizarConStockAsync(int id, Producto producto);
        Task<Producto?> CambiarEstadoAsync(int id);
    }
}
