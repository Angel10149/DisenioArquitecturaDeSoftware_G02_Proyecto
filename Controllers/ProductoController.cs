using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Encargado de Inventario")]
    public class ProductoController : Controller
    {
        private readonly IProductoService _productoService;

        public ProductoController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        private async Task CargarCatalogosAsync()
        {
            ViewBag.Categorias = await _productoService.ListarCategoriasActivasAsync();
            ViewBag.UnidadesMedida = await _productoService.ListarUnidadesActivasAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, int? idCategoria)
        {
            ViewBag.Categorias = await _productoService.ListarCategoriasActivasAsync();
            ViewBag.Buscar = buscar;
            ViewBag.IdCategoria = idCategoria;

            var productos = await _productoService.ListarAsync(buscar, idCategoria);
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
        public async Task<IActionResult> Crear(Producto producto, decimal? margenGanancia)
        {
            await CargarCatalogosAsync();

            if (margenGanancia.HasValue && margenGanancia.Value >= 0 && margenGanancia.Value < 100)
            {
                producto.precioVenta = Math.Round(producto.precioCosto / (1m - (margenGanancia.Value / 100m)), 2);
            }

            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            if (producto.precioVenta < producto.precioCosto)
            {
                ModelState.AddModelError(nameof(producto.precioVenta), "El precio de venta no puede ser menor al precio de costo.");
                return View(producto);
            }

            if (await _productoService.ExisteCodigoBarrasAsync(producto.codigoBarras))
            {
                ModelState.AddModelError(nameof(producto.codigoBarras), "Ya existe un producto registrado con ese código.");
                return View(producto);
            }

            await _productoService.CrearConStockInicialAsync(producto);

            TempData["Mensaje"] = $"Producto '{producto.nombre}' registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
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
        public async Task<IActionResult> Editar(int id, Producto producto, decimal? margenGanancia)
        {
            if (id != producto.idProducto)
            {
                return BadRequest();
            }

            await CargarCatalogosAsync();

            if (margenGanancia.HasValue && margenGanancia.Value >= 0 && margenGanancia.Value < 100)
            {
                producto.precioVenta = Math.Round(producto.precioCosto / (1m - (margenGanancia.Value / 100m)), 2);
            }

            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            if (producto.precioVenta < producto.precioCosto)
            {
                ModelState.AddModelError(nameof(producto.precioVenta), "El precio de venta no puede ser menor al precio de costo.");
                return View(producto);
            }

            if (await _productoService.ExisteCodigoBarrasAsync(producto.codigoBarras, id))
            {
                ModelState.AddModelError(nameof(producto.codigoBarras), "Otro producto ya tiene registrado ese código.");
                return View(producto);
            }

            bool actualizado = await _productoService.ActualizarConStockAsync(id, producto);
            if (!actualizado)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = $"Producto '{producto.nombre.Trim()}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var producto = await _productoService.CambiarEstadoAsync(id);
            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = producto.estado
                ? $"Producto '{producto.nombre}' activado correctamente."
                : $"Producto '{producto.nombre}' desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
