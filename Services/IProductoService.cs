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

        Task<IEnumerable<Categoria>> ListarCategoriasAsync(string? buscar);
        Task<Categoria?> ObtenerCategoriaPorIdAsync(int id);
        Task<bool> ExisteNombreCategoriaAsync(string nombre, int? excluirId = null);
        Task CrearCategoriaAsync(Categoria categoria);
        Task<bool> ActualizarCategoriaAsync(int id, Categoria categoria);
        Task<Categoria?> CambiarEstadoCategoriaAsync(int id);

        Task<IEnumerable<UnidadMedida>> ListarUnidadesAsync(string? buscar);
        Task<UnidadMedida?> ObtenerUnidadPorIdAsync(int id);
        Task<bool> ExisteNombreUnidadAsync(string nombre, int? excluirId = null);
        Task<bool> ExisteAbreviaturaUnidadAsync(string abreviatura, int? excluirId = null);
        Task CrearUnidadAsync(UnidadMedida unidad);
        Task<bool> ActualizarUnidadAsync(int id, UnidadMedida unidad);
        Task<UnidadMedida?> CambiarEstadoUnidadAsync(int id);
    }
}
