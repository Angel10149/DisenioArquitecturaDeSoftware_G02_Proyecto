using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly AppDbContext _context;

        public VentaController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CargarDatosCrearAsync(int? idCotizacion)
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

            ViewBag.CotizacionesAprobadas = await _context.Cotizaciones
                .Include(c => c.cliente)
                .Where(c => c.estado == "Aprobada")
                .OrderByDescending(c => c.idCotizacion)
                .ToListAsync();

            ViewBag.IdCotizacionSeleccionada = idCotizacion;
            if (idCotizacion.HasValue && idCotizacion.Value > 0)
            {
                ViewBag.CotizacionSeleccionada = await _context.Cotizaciones
                    .Include(c => c.cliente)
                    .Include(c => c.DetalleCotizacions)
                        .ThenInclude(d => d.producto)
                    .FirstOrDefaultAsync(c => c.idCotizacion == idCotizacion.Value && c.estado == "Aprobada");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            var query = _context.Ventas
                .Include(v => v.cliente)
                .Include(v => v.usuario)
                .Include(v => v.cotizacion)
                .Include(v => v.DetalleVentas)
                .Include(v => v.Pagos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(v => v.estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(v =>
                    v.cliente!.nombres.Contains(filtro) ||
                    v.cliente!.razonSocial.Contains(filtro) ||
                    v.cliente!.numeroDocumento.Contains(filtro) ||
                    v.usuario!.nombreUsuario.Contains(filtro) ||
                    v.idVenta.ToString() == filtro);
            }

            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var ventas = await query.OrderByDescending(v => v.idVenta).ToListAsync();
            return View(ventas);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int? idCotizacion)
        {
            await CargarDatosCrearAsync(idCotizacion);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            int idCliente,
            int? idCotizacion,
            List<int> idProductos,
            List<int> cantidades,
            List<decimal> preciosUnitarios,
            List<decimal> descuentos)
        {
            await CargarDatosCrearAsync(idCotizacion);

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

            var detalles = new List<DetalleVenta>();
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
                        detalles.Add(new DetalleVenta
                        {
                            idproducto = idProd,
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cantidadesPorProducto = detalles
                    .GroupBy(d => d.idproducto)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.cantidad));

                foreach (var kvp in cantidadesPorProducto)
                {
                    var producto = await _context.Productos.FindAsync(kvp.Key);
                    if (producto == null || !producto.estado)
                    {
                        await transaction.RollbackAsync();
                        ViewData["Error"] = "Uno de los productos seleccionados no existe o está inactivo.";
                        return View();
                    }

                    if (producto.stockActual < kvp.Value)
                    {
                        await transaction.RollbackAsync();
                        ViewData["Error"] = $"Stock insuficiente para '{producto.nombre}'. Disponible: {producto.stockActual}, solicitado: {kvp.Value}.";
                        return View();
                    }

                    producto.stockActual -= kvp.Value;
                    await _context.Stocks.AddAsync(new Stock
                    {
                        idProducto = producto.idProducto,
                        cantidad = producto.stockActual,
                        fecha_actualizacion = DateTime.Now
                    });
                }

                if (idCotizacion.HasValue && idCotizacion.Value > 0)
                {
                    var cotizacion = await _context.Cotizaciones.FindAsync(idCotizacion.Value);
                    if (cotizacion != null && cotizacion.estado == "Aprobada")
                    {
                        cotizacion.estado = "Convertida";
                    }
                }

                var venta = new Venta
                {
                    fechaVenta = DateTime.Now,
                    estado = "Pendiente",
                    total = detalles.Sum(d => d.subtotal),
                    idCliente = idCliente,
                    idUsuario = idUsuario,
                    idCotizacion = (idCotizacion.HasValue && idCotizacion.Value > 0) ? idCotizacion.Value : null,
                    DetalleVentas = detalles
                };

                await _context.Ventas.AddAsync(venta);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Mensaje"] = $"Venta #{venta.idVenta} registrada correctamente. Stock actualizado.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                ViewData["Error"] = "Ocurrió un error al registrar la venta.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.cliente)
                .Include(v => v.usuario)
                .Include(v => v.cotizacion)
                .Include(v => v.DetalleVentas)
                    .ThenInclude(d => d.producto)
                .Include(v => v.Pagos)
                    .ThenInclude(p => p.Comprobantes)
                .FirstOrDefaultAsync(v => v.idVenta == id);

            if (venta == null)
            {
                TempData["Error"] = "Venta no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            return View(venta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.DetalleVentas)
                .FirstOrDefaultAsync(v => v.idVenta == id);

            if (venta == null)
            {
                TempData["Error"] = "Venta no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            if (venta.estado != "Pendiente")
            {
                TempData["Error"] = $"Solo se pueden anular ventas en estado 'Pendiente'. Estado actual: '{venta.estado}'.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var det in venta.DetalleVentas)
                {
                    var producto = await _context.Productos.FindAsync(det.idproducto);
                    if (producto != null)
                    {
                        producto.stockActual += det.cantidad;
                        await _context.Stocks.AddAsync(new Stock
                        {
                            idProducto = producto.idProducto,
                            cantidad = producto.stockActual,
                            fecha_actualizacion = DateTime.Now
                        });
                    }
                }

                venta.estado = "Anulada";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Mensaje"] = $"Venta #{venta.idVenta} anulada y stock restaurado.";
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "No se pudo anular la venta.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
