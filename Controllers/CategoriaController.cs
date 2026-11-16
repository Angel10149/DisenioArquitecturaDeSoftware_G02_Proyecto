using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Encargado de Inventario")]
    public class CategoriaController : Controller
    {
        private readonly IProductoService _productoService;

        public CategoriaController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            ViewBag.Buscar = buscar;
            var categorias = await _productoService.ListarCategoriasAsync(buscar);
            return View(categorias);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Categoria { estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            if (await _productoService.ExisteNombreCategoriaAsync(categoria.nombre))
            {
                ModelState.AddModelError(nameof(categoria.nombre), "Ya existe una categoría registrada con ese nombre.");
                return View(categoria);
            }

            await _productoService.CrearCategoriaAsync(categoria);

            TempData["Mensaje"] = $"Categoría '{categoria.nombre}' registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var categoria = await _productoService.ObtenerCategoriaPorIdAsync(id);
            if (categoria == null)
            {
                TempData["Error"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Categoria categoria)
        {
            if (id != categoria.idCategoria)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            if (await _productoService.ExisteNombreCategoriaAsync(categoria.nombre, id))
            {
                ModelState.AddModelError(nameof(categoria.nombre), "Otra categoría ya tiene registrado ese nombre.");
                return View(categoria);
            }

            bool actualizado = await _productoService.ActualizarCategoriaAsync(id, categoria);
            if (!actualizado)
            {
                TempData["Error"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = $"Categoría '{categoria.nombre.Trim()}' actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var categoria = await _productoService.CambiarEstadoCategoriaAsync(id);
            if (categoria == null)
            {
                TempData["Error"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = categoria.estado
                ? $"Categoría '{categoria.nombre}' activada correctamente."
                : $"Categoría '{categoria.nombre}' desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
