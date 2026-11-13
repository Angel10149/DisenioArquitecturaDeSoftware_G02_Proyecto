using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    [Authorize]
    public class CotizacionesController : Controller
    {
        private readonly AppDbContext _context;

        public CotizacionesController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CargarDatosCrearAsync()
        {
            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.estado)
                .OrderBy(c => c.nombres)
                .ThenBy(c => c.razonSocial)
                .ToListAsync();

            ViewBag.Productos = await _context.Productos
                .Where(p => p.estado)
                .OrderBy(p => p.nombre)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            var query = _context.Cotizaciones
                .Include(c => c.cliente)
                .Include(c => c.usuario)
                .Include(c => c.DetalleCotizacions)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(c => c.estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(c =>
                    c.cliente.nombres.Contains(filtro) ||
                    c.cliente.razonSocial.Contains(filtro) ||
                    c.cliente.numeroDocumento.Contains(filtro) ||
                    c.usuario.nombreUsuario.Contains(filtro) ||
                    c.idCotizacion.ToString() == filtro);
            }

            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var cotizaciones = await query.OrderByDescending(c => c.idCotizacion).ToListAsync();
            return View(cotizaciones);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarDatosCrearAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            int idCliente,
            string? observaciones,
            List<int> idProductos,
            List<int> cantidades,
            List<decimal> preciosUnitarios,
            List<decimal> descuentos)
        {
            await CargarDatosCrearAsync();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int idUsuario))
            {
                return RedirectToAction("Login", "Usuario");
            }

            if (idCliente <= 0)
            {
                ViewData["Error"] = "Debe seleccionar un cliente válido.";
                return View();
            }

            var detalles = new List<DetalleCotizacion>();
            if (idProductos != null && cantidades != null && preciosUnitarios != null)
            {
                int count = Math.Min(idProductos.Count, Math.Min(cantidades.Count, preciosUnitarios.Count));
                for (int i = 0; i < count; i++)
                {
                    int idProd = idProductos[i];
                    int cant = cantidades[i];
                    decimal precio = Math.Max(0, preciosUnitarios[i]);
                    decimal desc = (descuentos != null && i < descuentos.Count) ? Math.Max(0, descuentos[i]) : 0;

                    if (idProd > 0 && cant > 0)
                    {
                        decimal subtotal = Math.Max(0, (cant * precio) - desc);
                        detalles.Add(new DetalleCotizacion
                        {
                            idProducto = idProd,
                            cantidad = cant,
                            precioUnitario = precio,
                            descuento = desc,
                            subtotal = subtotal
                        });
                    }
                }
            }

            if (!detalles.Any())
            {
                ViewData["Error"] = "Debe agregar al menos un producto con cantidad mayor a 0.";
                return View();
            }

            var cotizacion = new Cotizacion
            {
                fechaEmision = DateTime.Now,
                estado = "Pendiente",
                observaciones = observaciones?.Trim() ?? string.Empty,
                total = detalles.Sum(d => d.subtotal),
                idCliente = idCliente,
                idUsuario = idUsuario,
                DetalleCotizacions = detalles
            };

            await _context.Cotizaciones.AddAsync(cotizacion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Cotización #{cotizacion.idCotizacion} registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.cliente)
                .Include(c => c.usuario)
                .Include(c => c.DetalleCotizacions)
                    .ThenInclude(d => d.producto)
                .FirstOrDefaultAsync(c => c.idCotizacion == id);

            if (cotizacion == null)
            {
                TempData["Error"] = "Cotización no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            return View(cotizacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var cotizacion = await _context.Cotizaciones.FindAsync(id);
            if (cotizacion == null)
            {
                TempData["Error"] = "Cotización no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            if (cotizacion.estado == "Convertida" || cotizacion.estado == "Anulada")
            {
                TempData["Error"] = $"La cotización #{cotizacion.idCotizacion} ya está en estado '{cotizacion.estado}' y no puede modificarse.";
                return RedirectToAction(nameof(Index));
            }

            if (nuevoEstado != "Aprobada" && nuevoEstado != "Anulada")
            {
                TempData["Error"] = "Estado no válido.";
                return RedirectToAction(nameof(Index));
            }

            cotizacion.estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Cotización #{cotizacion.idCotizacion} actualizada a '{nuevoEstado}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}
