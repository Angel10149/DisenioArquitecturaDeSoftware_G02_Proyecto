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
        private readonly IRepository<Stock> _stockRepo;

        public ProductoService(
            IRepository<Producto> productoRepo,
            IRepository<Categoria> categoriaRepo,
            IRepository<UnidadMedida> unidadRepo,
            IRepository<Stock> stockRepo)
        {
            _productoRepo = productoRepo;
            _categoriaRepo = categoriaRepo;
            _unidadRepo = unidadRepo;
            _stockRepo = stockRepo;
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

        public async Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, int? excluirId = null)
        {
            string codigo = codigoBarras.Trim();
            return await _productoRepo.Query()
                .AnyAsync(p => p.codigoBarras == codigo && (!excluirId.HasValue || p.idProducto != excluirId.Value));
        }

        public async Task CrearConStockInicialAsync(Producto producto)
        {
            producto.codigoBarras = producto.codigoBarras.Trim();
            producto.nombre = producto.nombre.Trim();
            producto.descripcion = producto.descripcion?.Trim() ?? string.Empty;
            producto.estado = true;

            await _productoRepo.AddAsync(producto);
            await _productoRepo.SaveChangesAsync();

            await _stockRepo.AddAsync(new Stock
            {
                idProducto = producto.idProducto,
                cantidad = producto.stockActual,
                fecha_actualizacion = DateTime.Now
            });
            await _stockRepo.SaveChangesAsync();
        }

        public async Task<bool> ActualizarConStockAsync(int id, Producto producto)
        {
            var productoDb = await _productoRepo.GetByIdAsync(id);
            if (productoDb == null) return false;

            bool cambioStock = productoDb.stockActual != producto.stockActual;

            productoDb.codigoBarras = producto.codigoBarras.Trim();
            productoDb.nombre = producto.nombre.Trim();
            productoDb.descripcion = producto.descripcion?.Trim() ?? string.Empty;
            productoDb.stockActual = producto.stockActual;
            productoDb.stockMinimo = producto.stockMinimo;
            productoDb.precioCosto = producto.precioCosto;
            productoDb.precioVenta = producto.precioVenta;
            productoDb.idCategoria = producto.idCategoria;
            productoDb.idUnidadMedida = producto.idUnidadMedida;

            if (cambioStock)
            {
                await _stockRepo.AddAsync(new Stock
                {
                    idProducto = productoDb.idProducto,
                    cantidad = productoDb.stockActual,
                    fecha_actualizacion = DateTime.Now
                });
            }

            _productoRepo.Update(productoDb);
            await _productoRepo.SaveChangesAsync();
            return true;
        }

        public async Task<Producto?> CambiarEstadoAsync(int id)
        {
            var producto = await _productoRepo.GetByIdAsync(id);
            if (producto == null) return null;

            producto.estado = !producto.estado;
            _productoRepo.Update(producto);
            await _productoRepo.SaveChangesAsync();
            return producto;
        }

        public async Task<IEnumerable<Categoria>> ListarCategoriasAsync(string? buscar)
        {
            var query = _categoriaRepo.Query().Include(c => c.Productos).AsQueryable();
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(c => c.nombre.Contains(filtro) || c.descripcion.Contains(filtro));
            }
            return await query.OrderByDescending(c => c.idCategoria).ToListAsync();
        }

        public async Task<Categoria?> ObtenerCategoriaPorIdAsync(int id) => await _categoriaRepo.GetByIdAsync(id);

        public async Task<bool> ExisteNombreCategoriaAsync(string nombre, int? excluirId = null)
        {
            string norm = nombre.Trim();
            return await _categoriaRepo.Query()
                .AnyAsync(c => c.nombre == norm && (!excluirId.HasValue || c.idCategoria != excluirId.Value));
        }

        public async Task CrearCategoriaAsync(Categoria categoria)
        {
            categoria.nombre = categoria.nombre.Trim();
            categoria.descripcion = categoria.descripcion.Trim();
            categoria.estado = true;
            await _categoriaRepo.AddAsync(categoria);
            await _categoriaRepo.SaveChangesAsync();
        }

        public async Task<bool> ActualizarCategoriaAsync(int id, Categoria categoria)
        {
            var categoriaDb = await _categoriaRepo.GetByIdAsync(id);
            if (categoriaDb == null) return false;

            categoriaDb.nombre = categoria.nombre.Trim();
            categoriaDb.descripcion = categoria.descripcion.Trim();
            _categoriaRepo.Update(categoriaDb);
            await _categoriaRepo.SaveChangesAsync();
            return true;
        }

        public async Task<Categoria?> CambiarEstadoCategoriaAsync(int id)
        {
            var categoria = await _categoriaRepo.GetByIdAsync(id);
            if (categoria == null) return null;

            categoria.estado = !categoria.estado;
            _categoriaRepo.Update(categoria);
            await _categoriaRepo.SaveChangesAsync();
            return categoria;
        }

        public async Task<IEnumerable<UnidadMedida>> ListarUnidadesAsync(string? buscar)
        {
            var query = _unidadRepo.Query().Include(u => u.Productos).AsQueryable();
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(u => u.nombre.Contains(filtro) || u.abreviatura.Contains(filtro));
            }
            return await query.OrderByDescending(u => u.idUnidadMedida).ToListAsync();
        }

        public async Task<UnidadMedida?> ObtenerUnidadPorIdAsync(int id) => await _unidadRepo.GetByIdAsync(id);

        public async Task<bool> ExisteNombreUnidadAsync(string nombre, int? excluirId = null)
        {
            string norm = nombre.Trim();
            return await _unidadRepo.Query()
                .AnyAsync(u => u.nombre == norm && (!excluirId.HasValue || u.idUnidadMedida != excluirId.Value));
        }

        public async Task<bool> ExisteAbreviaturaUnidadAsync(string abreviatura, int? excluirId = null)
        {
            string norm = abreviatura.Trim().ToUpper();
            return await _unidadRepo.Query()
                .AnyAsync(u => u.abreviatura == norm && (!excluirId.HasValue || u.idUnidadMedida != excluirId.Value));
        }

        public async Task CrearUnidadAsync(UnidadMedida unidad)
        {
            unidad.nombre = unidad.nombre.Trim();
            unidad.abreviatura = unidad.abreviatura.Trim().ToUpper();
            unidad.estado = true;
            await _unidadRepo.AddAsync(unidad);
            await _unidadRepo.SaveChangesAsync();
        }

        public async Task<bool> ActualizarUnidadAsync(int id, UnidadMedida unidad)
        {
            var unidadDb = await _unidadRepo.GetByIdAsync(id);
            if (unidadDb == null) return false;

            unidadDb.nombre = unidad.nombre.Trim();
            unidadDb.abreviatura = unidad.abreviatura.Trim().ToUpper();
            _unidadRepo.Update(unidadDb);
            await _unidadRepo.SaveChangesAsync();
            return true;
        }

        public async Task<UnidadMedida?> CambiarEstadoUnidadAsync(int id)
        {
            var unidad = await _unidadRepo.GetByIdAsync(id);
            if (unidad == null) return null;

            unidad.estado = !unidad.estado;
            _unidadRepo.Update(unidad);
            await _unidadRepo.SaveChangesAsync();
            return unidad;
        }
    }
}
