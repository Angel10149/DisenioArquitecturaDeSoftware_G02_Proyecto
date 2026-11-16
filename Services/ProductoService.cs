using AppWeb.Models;
using AppWeb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IRepository<Producto> _productoRepo;
        private readonly IRepository<Categoria> _categoriaRepo;
        private readonly IRepository<UnidadMedida> _unidadRepo;

        public ProductoService(
            IRepository<Producto> productoRepo,
            IRepository<Categoria> categoriaRepo,
            IRepository<UnidadMedida> unidadRepo)
        {
            _productoRepo = productoRepo;
            _categoriaRepo = categoriaRepo;
            _unidadRepo = unidadRepo;
        }

        public async Task<IEnumerable<Producto>> ListarAsync(string? buscar, int? idCategoria)
        {
            var query = _productoRepo.Query()
                .Include(p => p.categoria)
                .Include(p => p.unidadMedida)
                .AsQueryable();

            if (idCategoria.HasValue && idCategoria.Value > 0)
            {
                query = query.Where(p => p.idCategoria == idCategoria.Value);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(p =>
                    p.nombre.Contains(filtro) ||
                    p.codigoBarras.Contains(filtro) ||
                    (p.descripcion != null && p.descripcion.Contains(filtro)) ||
                    p.categoria!.nombre.Contains(filtro));
            }

            return await query.OrderByDescending(p => p.idProducto).ToListAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _productoRepo.Query()
                .Include(p => p.categoria)
                .Include(p => p.unidadMedida)
                .FirstOrDefaultAsync(p => p.idProducto == id);
        }

        public async Task<IEnumerable<Categoria>> ListarCategoriasActivasAsync()
        {
            return await _categoriaRepo.Query()
                .Where(c => c.estado)
                .OrderBy(c => c.nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<UnidadMedida>> ListarUnidadesActivasAsync()
        {
            return await _unidadRepo.Query()
                .Where(u => u.estado)
                .OrderBy(u => u.nombre)
                .ToListAsync();
        }

        public async Task CrearAsync(Producto producto)
        {
            await _productoRepo.AddAsync(producto);
            await _productoRepo.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Producto producto)
        {
            _productoRepo.Update(producto);
            await _productoRepo.SaveChangesAsync();
        }
    }
}
