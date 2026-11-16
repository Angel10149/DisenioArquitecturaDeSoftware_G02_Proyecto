using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Encargado de Inventario")]
    public class UnidadMedidaController : Controller
    {
        private readonly IProductoService _productoService;

        public UnidadMedidaController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            ViewBag.Buscar = buscar;
            var unidades = await _productoService.ListarUnidadesAsync(buscar);
            return View(unidades);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new UnidadMedida { estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UnidadMedida unidad)
        {
            if (!ModelState.IsValid)
            {
                return View(unidad);
            }

            if (await _productoService.ExisteNombreUnidadAsync(unidad.nombre))
            {
                ModelState.AddModelError(nameof(unidad.nombre), "Ya existe una unidad de medida con ese nombre.");
                return View(unidad);
            }
            if (await _productoService.ExisteAbreviaturaUnidadAsync(unidad.abreviatura))
            {
                ModelState.AddModelError(nameof(unidad.abreviatura), "Ya existe una unidad de medida con esa abreviatura.");
                return View(unidad);
            }

            await _productoService.CrearUnidadAsync(unidad);

            TempData["Mensaje"] = $"Unidad de medida '{unidad.nombre}' ({unidad.abreviatura}) registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var unidad = await _productoService.ObtenerUnidadPorIdAsync(id);
            if (unidad == null)
            {
                TempData["Error"] = "Unidad de medida no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            return View(unidad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, UnidadMedida unidad)
        {
            if (id != unidad.idUnidadMedida)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(unidad);
            }

            if (await _productoService.ExisteNombreUnidadAsync(unidad.nombre, id))
            {
                ModelState.AddModelError(nameof(unidad.nombre), "Otra unidad de medida ya tiene ese nombre.");
                return View(unidad);
            }
            if (await _productoService.ExisteAbreviaturaUnidadAsync(unidad.abreviatura, id))
            {
                ModelState.AddModelError(nameof(unidad.abreviatura), "Otra unidad de medida ya tiene esa abreviatura.");
                return View(unidad);
            }

            bool actualizado = await _productoService.ActualizarUnidadAsync(id, unidad);
            if (!actualizado)
            {
                TempData["Error"] = "Unidad de medida no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = $"Unidad de medida '{unidad.nombre.Trim()}' actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var unidad = await _productoService.CambiarEstadoUnidadAsync(id);
            if (unidad == null)
            {
                TempData["Error"] = "Unidad de medida no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = unidad.estado
                ? $"Unidad de medida '{unidad.nombre}' activada correctamente."
                : $"Unidad de medida '{unidad.nombre}' desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
