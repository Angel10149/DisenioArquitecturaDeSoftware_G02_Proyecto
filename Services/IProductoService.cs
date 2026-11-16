using AppWeb.Models;

namespace AppWeb.Services
{
    public interface IProductoService
    {
        Task<IEnumerable<Producto>> ListarAsync(string? buscar, int? idCategoria);
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Categoria>> ListarCategoriasActivasAsync();
        Task<IEnumerable<UnidadMedida>> ListarUnidadesActivasAsync();
        Task CrearAsync(Producto producto);
        Task ActualizarAsync(Producto producto);
    }
}
