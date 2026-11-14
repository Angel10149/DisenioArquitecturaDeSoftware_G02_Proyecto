using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Encargado de Inventario")]
    public class ProductoController : Controller
    {
        private readonly AppDbContext _context;

        public ProductoController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CargarCatalogosAsync()
        {
            ViewBag.Categorias = await _context.Categorias
                .Where(c => c.estado)
                .OrderBy(c => c.nombre)
                .ToListAsync();

            ViewBag.UnidadesMedida = await _context.UnidadesMedida
                .Where(u => u.estado)
                .OrderBy(u => u.nombre)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, int? idCategoria)
        {
            ViewBag.Categorias = await _context.Categorias
                .Where(c => c.estado)
                .OrderBy(c => c.nombre)
                .ToListAsync();

            var query = _context.Productos
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

            ViewBag.Buscar = buscar;
            ViewBag.IdCategoria = idCategoria;

            var productos = await query.OrderByDescending(p => p.idProducto).ToListAsync();
            return View(productos);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarCatalogosAsync();
            return View(new Producto { estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Producto producto)
        {
            await CargarCatalogosAsync();

            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            string codigo = producto.codigoBarras.Trim();
            if (await _context.Productos.AnyAsync(p => p.codigoBarras == codigo))
            {
                ModelState.AddModelError(nameof(producto.codigoBarras), "Ya existe un producto registrado con ese código.");
                return View(producto);
            }

            producto.codigoBarras = codigo;
            producto.nombre = producto.nombre.Trim();
            producto.descripcion = producto.descripcion?.Trim() ?? string.Empty;
            producto.estado = true;

            await _context.Productos.AddAsync(producto);
            await _context.SaveChangesAsync();

            // Registrar stock inicial en la tabla Stocks
            await _context.Stocks.AddAsync(new Stock
            {
                idProducto = producto.idProducto,
                cantidad = producto.stockActual,
                fecha_actualizacion = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Producto '{producto.nombre}' registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            await CargarCatalogosAsync();
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Producto producto)
        {
            if (id != producto.idProducto)
            {
                return BadRequest();
            }

            await CargarCatalogosAsync();

            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            string codigo = producto.codigoBarras.Trim();
            if (await _context.Productos.AnyAsync(p => p.codigoBarras == codigo && p.idProducto != id))
            {
                ModelState.AddModelError(nameof(producto.codigoBarras), "Otro producto ya tiene registrado ese código.");
                return View(producto);
            }

            var productoDb = await _context.Productos.FindAsync(id);
            if (productoDb == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            bool cambioStock = productoDb.stockActual != producto.stockActual;

            productoDb.codigoBarras = codigo;
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
                await _context.Stocks.AddAsync(new Stock
                {
                    idProducto = productoDb.idProducto,
                    cantidad = productoDb.stockActual,
                    fecha_actualizacion = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Producto '{productoDb.nombre}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            producto.estado = !producto.estado;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = producto.estado
                ? $"Producto '{producto.nombre}' activado correctamente."
                : $"Producto '{producto.nombre}' desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
