using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Encargado de Inventario,Encargado de Compras")]
    public class RequerimientoCompraController : Controller
    {
        private readonly ICompraService _compraService;

        public RequerimientoCompraController(ICompraService compraService)
        {
            _compraService = compraService;
        }

        private async Task CargarProductosAsync()
        {
            ViewBag.Productos = await _compraService.ListarProductosActivosAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var requerimientos = await _compraService.ListarRequerimientosFiltradosAsync(buscar, estado);
            return View(requerimientos);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int? idProducto)
        {
            await CargarProductosAsync();

            var modelo = new RequerimientoCompra
            {
                idProducto = idProducto ?? 0,
                fechaRequerida = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                prioridad = "Media",
                estado = "Pendiente",
                cantidadRequerida = 1
            };

            if (idProducto.HasValue && idProducto.Value > 0)
            {
                var prod = await _compraService.ObtenerProductoPorIdAsync(idProducto.Value);
                if (prod != null)
                {
                    int sugerida = Math.Max(1, (prod.stockMinimo * 2) - prod.stockActual);
                    modelo.cantidadSugerida = sugerida;
                    modelo.cantidadRequerida = sugerida;
                }
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(RequerimientoCompra requerimiento)
        {
            try
            {
                await CargarProductosAsync();

                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int idUsuario))
                {
                    return RedirectToAction("Login", "Usuario");
                }

                if (!ModelState.IsValid)
                {
                    return View(requerimiento);
                }

                var producto = await _compraService.ObtenerProductoPorIdAsync(requerimiento.idProducto);
                if (producto == null)
                {
                    ModelState.AddModelError(nameof(requerimiento.idProducto), "El producto seleccionado no existe.");
                    return View(requerimiento);
                }

                if (requerimiento.cantidadSugerida <= 0)
                {
                    requerimiento.cantidadSugerida = Math.Max(1, (producto.stockMinimo * 2) - producto.stockActual);
                }

                requerimiento.idUsuario = idUsuario;
                requerimiento.estado = "Pendiente";
                requerimiento.fechaCreacion = DateTime.Now;
                requerimiento.motivo = requerimiento.motivo?.Trim() ?? string.Empty;

                await _compraService.CrearRequerimientoAsync(requerimiento);

                TempData["Mensaje"] = $"Requerimiento #{requerimiento.idRequerimiento} para '{producto.nombre}' registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, DbExceptionHelper.Traducir(ex));
                return View(requerimiento);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var requerimiento = await _compraService.ObtenerRequerimientoPorIdAsync(id);
            if (requerimiento == null)
            {
                TempData["Error"] = "Requerimiento no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (requerimiento.estado != "Pendiente")
            {
                TempData["Error"] = "Solo se pueden editar requerimientos en estado Pendiente.";
                return RedirectToAction(nameof(Index));
            }

            await CargarProductosAsync();
            return View(requerimiento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, RequerimientoCompra requerimiento)
        {
            try
            {
                if (id != requerimiento.idRequerimiento)
                {
                    return BadRequest();
                }

                await CargarProductosAsync();

                if (!ModelState.IsValid)
                {
                    return View(requerimiento);
                }

                var resultado = await _compraService.ActualizarRequerimientoAsync(id, requerimiento);
                if (!resultado.Ok)
                {
                    TempData["Error"] = resultado.Error;
                    return RedirectToAction(nameof(Index));
                }

                TempData["Mensaje"] = $"Requerimiento #{id} actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, DbExceptionHelper.Traducir(ex));
                return View(requerimiento);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            try
            {
                var resultado = await _compraService.CambiarEstadoRequerimientoAsync(id, nuevoEstado);
                if (!resultado.Ok)
                {
                    TempData["Error"] = resultado.Error;
                }
                else
                {
                    TempData["Mensaje"] = $"Requerimiento #{id} actualizado a estado '{nuevoEstado}'.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                
                TempData["Error"] = DbExceptionHelper.Traducir(ex);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
