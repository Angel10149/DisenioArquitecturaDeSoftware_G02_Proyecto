using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    [Authorize]
    public class CajaController : Controller
    {
        private readonly AppDbContext _context;

        public CajaController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CargarVentasPendientesAsync(int? idVenta)
        {
            ViewBag.VentasPendientes = await _context.Ventas
                .Include(v => v.cliente)
                .Where(v => v.estado == "Pendiente")
                .OrderByDescending(v => v.idVenta)
                .ToListAsync();

            ViewBag.IdVentaSeleccionada = idVenta;
            if (idVenta.HasValue && idVenta.Value > 0)
            {
                ViewBag.VentaSeleccionada = await _context.Ventas
                    .Include(v => v.cliente)
                    .Include(v => v.DetalleVentas)
                        .ThenInclude(d => d.producto)
                    .FirstOrDefaultAsync(v => v.idVenta == idVenta.Value && v.estado == "Pendiente");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? metodoPago)
        {
            var query = _context.Pagos
                .Include(p => p.venta)
                    .ThenInclude(v => v!.cliente)
                .Include(p => p.usuario)
                .Include(p => p.Comprobantes)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(metodoPago))
            {
                query = query.Where(p => p.metodoPago == metodoPago);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(p =>
                    p.venta!.cliente!.nombres.Contains(filtro) ||
                    p.venta!.cliente!.razonSocial.Contains(filtro) ||
                    p.venta!.cliente!.numeroDocumento.Contains(filtro) ||
                    p.idVenta.ToString() == filtro ||
                    p.idPago.ToString() == filtro);
            }

            ViewBag.Buscar = buscar;
            ViewBag.MetodoPago = metodoPago;
            ViewBag.VentasPendientes = await _context.Ventas
                .Include(v => v.cliente)
                .Where(v => v.estado == "Pendiente")
                .OrderByDescending(v => v.idVenta)
                .ToListAsync();

            var pagos = await query.OrderByDescending(p => p.idPago).ToListAsync();
            return View(pagos);
        }

        [HttpGet]
        public async Task<IActionResult> Cobrar(int? idVenta)
        {
            await CargarVentasPendientesAsync(idVenta);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cobrar(
            int idVenta,
            string metodoPago,
            decimal montoRecibido,
            string tipoComprobante)
        {
            await CargarVentasPendientesAsync(idVenta);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int idUsuario))
            {
                return RedirectToAction("Login", "Usuario");
            }

            var venta = await _context.Ventas
                .Include(v => v.cliente)
                .FirstOrDefaultAsync(v => v.idVenta == idVenta);

            if (venta == null)
            {
                ViewData["Error"] = "La venta seleccionada no existe.";
                return View();
            }

            if (venta.estado != "Pendiente")
            {
                ViewData["Error"] = $"La venta #{venta.idVenta} no está pendiente de pago (estado: '{venta.estado}').";
                return View();
            }

            if (string.IsNullOrWhiteSpace(metodoPago))
            {
                ViewData["Error"] = "Debe seleccionar un método de pago.";
                return View();
            }

            if (montoRecibido < venta.total)
            {
                ViewData["Error"] = $"El monto recibido (S/ {montoRecibido:N2}) no puede ser menor al total de la venta (S/ {venta.total:N2}).";
                return View();
            }

            string tipoComp = string.IsNullOrWhiteSpace(tipoComprobante) ? "Boleta" : tipoComprobante.Trim();
            string serie = tipoComp switch
            {
                "Factura" => "F001",
                "Ticket" => "T001",
                _ => "B001"
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int correlativo = await _context.Comprobantes.CountAsync(c => c.serie == serie) + 1;
                string numero = correlativo.ToString("D8");

                var pago = new Pago
                {
                    fechaPago = DateTime.Now,
                    metodoPago = metodoPago.Trim(),
                    montoRecibido = montoRecibido,
                    estado = "Registrado",
                    idVenta = venta.idVenta,
                    idUsuario = idUsuario
                };

                await _context.Pagos.AddAsync(pago);
                await _context.SaveChangesAsync();

                var comprobante = new Comprobante
                {
                    tipoComprobante = tipoComp,
                    serie = serie,
                    numero = numero,
                    archivoPdf = $"{serie}-{numero}.pdf",
                    estado = true,
                    idPago = pago.idPago
                };

                await _context.Comprobantes.AddAsync(comprobante);
                venta.estado = "Pagada";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Mensaje"] = $"Pago #{pago.idPago} registrado. Comprobante {serie}-{numero} emitido.";
                return RedirectToAction(nameof(Comprobante), new { id = pago.idPago });
            }
            catch
            {
                await transaction.RollbackAsync();
                ViewData["Error"] = "Ocurrió un error al registrar el pago y emitir el comprobante.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Comprobante(int id)
        {
            var pago = await _context.Pagos
                .Include(p => p.venta)
                    .ThenInclude(v => v!.cliente)
                .Include(p => p.venta)
                    .ThenInclude(v => v!.DetalleVentas)
                        .ThenInclude(d => d.producto)
                .Include(p => p.usuario)
                .Include(p => p.Comprobantes)
                .FirstOrDefaultAsync(p => p.idPago == id);

            if (pago == null)
            {
                TempData["Error"] = "Pago o comprobante no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(pago);
        }
    }
}
