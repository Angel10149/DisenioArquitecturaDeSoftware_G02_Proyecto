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
        private readonly IWebHostEnvironment _env;

        public ConfiguracionController(IConfiguracionService configuracionService, IWebHostEnvironment env)
        {
            _configuracionService = configuracionService;
            _env = env;
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
        public async Task<IActionResult> Index(ConfiguracionEmpresa model, IFormFile? logoArchivo)
        {
            if (logoArchivo != null && logoArchivo.Length > 0)
            {
                var extensionesPermitidas = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var ext = Path.GetExtension(logoArchivo.FileName).ToLowerInvariant();

                if (!extensionesPermitidas.Contains(ext))
                {
                    ModelState.AddModelError(string.Empty, "El logo debe ser una imagen en formato PNG, JPG, JPEG o WEBP.");
                }
                else if (logoArchivo.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(string.Empty, "La imagen del logo no puede superar los 2 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarResumenSistemaAsync();
                return View(model);
            }

            try
            {
                if (logoArchivo != null && logoArchivo.Length > 0)
                {
                    var ext = Path.GetExtension(logoArchivo.FileName).ToLowerInvariant();
                    var carpetaPictures = Path.Combine(_env.WebRootPath, "pictures");
                    Directory.CreateDirectory(carpetaPictures);

                    var nombreArchivo = $"logo-empresa{ext}";
                    var rutaFisica = Path.Combine(carpetaPictures, nombreArchivo);

                    using (var stream = new FileStream(rutaFisica, FileMode.Create))
                    {
                        await logoArchivo.CopyToAsync(stream);
                    }

                    model.logoUrl = $"/pictures/{nombreArchivo}?v={DateTime.Now.Ticks}";
                }

                await _configuracionService.GuardarAsync(model);
                TempData["MensajeExito"] = "Configuración de la empresa guardada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, DbExceptionHelper.Traducir(ex));
                await CargarResumenSistemaAsync();
                return View(model);
            }
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
