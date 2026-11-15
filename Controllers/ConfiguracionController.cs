using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ConfiguracionController : Controller
    {
        private readonly AppDbContext _context;

        public ConfiguracionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await CargarResumenSistemaAsync();
            var config = await ObtenerOCrearConfiguracionAsync();
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

            var config = await _context.ConfiguracionesEmpresa.FirstOrDefaultAsync();
            if (config == null)
            {
                model.fechaActualizacion = DateTime.Now;
                _context.ConfiguracionesEmpresa.Add(model);
            }
            else
            {
                config.razonSocial = model.razonSocial.Trim();
                config.ruc = model.ruc.Trim();
                config.direccion = model.direccion?.Trim();
                config.telefono = model.telefono?.Trim();
                config.logoUrl = string.IsNullOrWhiteSpace(model.logoUrl) ? null : model.logoUrl.Trim();
                config.moneda = model.moneda.Trim();
                config.igv = model.igv;
                config.serieBoleta = model.serieBoleta.Trim();
                config.serieFactura = model.serieFactura.Trim();
                config.correo = string.IsNullOrWhiteSpace(model.correo) ? null : model.correo.Trim();
                config.mensajePie = string.IsNullOrWhiteSpace(model.mensajePie) ? null : model.mensajePie.Trim();
                config.fechaActualizacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = "Configuración de la empresa guardada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<ConfiguracionEmpresa> ObtenerOCrearConfiguracionAsync()
        {
            var config = await _context.ConfiguracionesEmpresa.FirstOrDefaultAsync();
            if (config != null) return config;

            config = new ConfiguracionEmpresa();
            _context.ConfiguracionesEmpresa.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }

        private async Task CargarResumenSistemaAsync()
        {
            ViewBag.TotalRoles = await _context.Roles.CountAsync(r => r.estado);
            ViewBag.TotalUsuarios = await _context.Usuarios.CountAsync(u => u.estado);
            ViewBag.TotalProductos = await _context.Productos.CountAsync(p => p.estado);
            ViewBag.TotalClientes = await _context.Clientes.CountAsync(c => c.estado);
            ViewBag.TotalProveedores = await _context.Proveedores.CountAsync(p => p.estado);
        }
    }
}
