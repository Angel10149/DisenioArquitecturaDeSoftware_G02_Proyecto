using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Encargado de Compras")]
    public class ProveedorController : Controller
    {
        private readonly ICompraService _compraService;

        public ProveedorController(ICompraService compraService)
        {
            _compraService = compraService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            ViewBag.Buscar = buscar;
            var proveedores = await _compraService.ListarProveedoresAsync(buscar);
            return View(proveedores);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Proveedor { estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Proveedor proveedor)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(proveedor);
                }

                if (await _compraService.ExisteRucProveedorAsync(proveedor.ruc))
                {
                    ModelState.AddModelError(nameof(proveedor.ruc), "Ya existe un proveedor registrado con ese RUC.");
                    return View(proveedor);
                }

                await _compraService.CrearProveedorAsync(proveedor);

                TempData["Mensaje"] = $"Proveedor '{proveedor.razonSocial}' registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, DbExceptionHelper.Traducir(ex));
                return View(proveedor);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var proveedor = await _compraService.ObtenerProveedorPorIdAsync(id);
            if (proveedor == null)
            {
                TempData["Error"] = "Proveedor no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Proveedor proveedor)
        {
            try
            {
                if (id != proveedor.idProveedor)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return View(proveedor);
                }

                if (await _compraService.ExisteRucProveedorAsync(proveedor.ruc, id))
                {
                    ModelState.AddModelError(nameof(proveedor.ruc), "Otro proveedor ya tiene registrado ese RUC.");
                    return View(proveedor);
                }

                bool actualizado = await _compraService.ActualizarProveedorAsync(id, proveedor);
                if (!actualizado)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Mensaje"] = $"Proveedor '{proveedor.razonSocial.Trim()}' actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, DbExceptionHelper.Traducir(ex));
                return View(proveedor);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var proveedor = await _compraService.CambiarEstadoProveedorAsync(id);
                if (proveedor == null)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Mensaje"] = proveedor.estado
                    ? $"Proveedor '{proveedor.razonSocial}' activado correctamente."
                    : $"Proveedor '{proveedor.razonSocial}' desactivado correctamente.";

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
