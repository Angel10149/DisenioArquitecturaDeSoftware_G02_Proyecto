using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    [Authorize]
    public class RequerimientoCompraController : Controller
    {
        private readonly AppDbContext _context;

        public RequerimientoCompraController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CargarProductosAsync()
        {
            ViewBag.Productos = await _context.Productos
                .Where(p => p.estado)
                .OrderBy(p => p.nombre)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            var query = _context.RequerimientosCompra
                .Include(r => r.producto)
                .Include(r => r.usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(r => r.estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(r =>
                    r.producto!.nombre.Contains(filtro) ||
                    (r.motivo != null && r.motivo.Contains(filtro)) ||
                    r.usuario!.nombreUsuario.Contains(filtro));
            }

            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var requerimientos = await query.OrderByDescending(r => r.idRequerimiento).ToListAsync();
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
                var prod = await _context.Productos.FindAsync(idProducto.Value);
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

            var producto = await _context.Productos.FindAsync(requerimiento.idProducto);
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

            await _context.RequerimientosCompra.AddAsync(requerimiento);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Requerimiento #{requerimiento.idRequerimiento} para '{producto.nombre}' registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var requerimiento = await _context.RequerimientosCompra.FindAsync(id);
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
            if (id != requerimiento.idRequerimiento)
            {
                return BadRequest();
            }

            await CargarProductosAsync();

            if (!ModelState.IsValid)
            {
                return View(requerimiento);
            }

            var reqDb = await _context.RequerimientosCompra.FindAsync(id);
            if (reqDb == null)
            {
                TempData["Error"] = "Requerimiento no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (reqDb.estado != "Pendiente")
            {
                TempData["Error"] = "Solo se pueden editar requerimientos en estado Pendiente.";
                return RedirectToAction(nameof(Index));
            }

            reqDb.idProducto = requerimiento.idProducto;
            reqDb.cantidadSugerida = requerimiento.cantidadSugerida;
            reqDb.cantidadRequerida = requerimiento.cantidadRequerida;
            reqDb.fechaRequerida = requerimiento.fechaRequerida;
            reqDb.prioridad = requerimiento.prioridad;
            reqDb.motivo = requerimiento.motivo?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Requerimiento #{reqDb.idRequerimiento} actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var estadosPermitidos = new[] { "Pendiente", "Aprobado", "Rechazado", "Atendido" };
            if (!estadosPermitidos.Contains(nuevoEstado))
            {
                TempData["Error"] = "Estado no válido.";
                return RedirectToAction(nameof(Index));
            }

            var reqDb = await _context.RequerimientosCompra.FindAsync(id);
            if (reqDb == null)
            {
                TempData["Error"] = "Requerimiento no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            reqDb.estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Requerimiento #{reqDb.idRequerimiento} actualizado a estado '{nuevoEstado}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}
