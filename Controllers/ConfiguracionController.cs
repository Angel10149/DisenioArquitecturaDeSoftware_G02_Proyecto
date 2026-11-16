using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ConfiguracionController : Controller
    {
        private readonly IConfiguracionService _configuracionService;

        public ConfiguracionController(IConfiguracionService configuracionService)
        {
            _configuracionService = configuracionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await CargarResumenSistemaAsync();
            var config = await _configuracionService.ObtenerOCrearActualAsync();
            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ConfiguracionEmpresa model)
        {
            if (!ModelState.IsValid)
            {
                await CargarResumenSistemaAsync();
                return View(model);
            }

            await _configuracionService.GuardarAsync(model);
            TempData["MensajeExito"] = "Configuración de la empresa guardada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarResumenSistemaAsync()
        {
            var resumen = await _configuracionService.ObtenerResumenSistemaAsync();
            ViewBag.TotalRoles = resumen.TotalRoles;
            ViewBag.TotalUsuarios = resumen.TotalUsuarios;
            ViewBag.TotalProductos = resumen.TotalProductos;
            ViewBag.TotalClientes = resumen.TotalClientes;
            ViewBag.TotalProveedores = resumen.TotalProveedores;
        }
    }
}
