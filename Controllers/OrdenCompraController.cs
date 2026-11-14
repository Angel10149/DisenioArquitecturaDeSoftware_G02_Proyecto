using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Encargado de Compras")]
    public class OrdenCompraController : Controller
    {
        private readonly AppDbContext _context;

        public OrdenCompraController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CargarListasAsync()
        {
            ViewBag.Proveedores = await _context.Proveedores
                .Where(p => p.estado)
                .OrderBy(p => p.razonSocial)
                .ToListAsync();

            ViewBag.Requerimientos = await _context.RequerimientosCompra
                .Include(r => r.producto)
                .Where(r => r.estado == "Aprobado")
                .OrderByDescending(r => r.idRequerimiento)
                .ToListAsync();

            ViewBag.Productos = await _context.Productos
                .Where(p => p.estado)
                .OrderBy(p => p.nombre)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            var query = _context.OrdenesCompra
                .Include(o => o.proveedor)
                .Include(o => o.usuario)
                .Include(o => o.requerimiento)
                    .ThenInclude(r => r!.producto)
                .Include(o => o.DetalleOrdenCompras)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(o => o.estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(o =>
                    o.proveedor!.razonSocial.Contains(filtro) ||
                    o.usuario!.nombreUsuario.Contains(filtro) ||
                    o.idOrdenCompra.ToString() == filtro);
            }

            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var ordenes = await query.OrderByDescending(o => o.idOrdenCompra).ToListAsync();
            return View(ordenes);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int? idRequerimiento)
        {
            await CargarListasAsync();

            ViewBag.IdRequerimientoSeleccionado = idRequerimiento;
            if (idRequerimiento.HasValue && idRequerimiento.Value > 0)
            {
                var req = await _context.RequerimientosCompra
                    .Include(r => r.producto)
                    .FirstOrDefaultAsync(r => r.idRequerimiento == idRequerimiento.Value);
                ViewBag.RequerimientoSeleccionado = req;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            int idProveedor,
            int idRequerimiento,
            List<int> idProductos,
            List<int> cantidades,
            List<decimal> preciosUnitarios)
        {
            await CargarListasAsync();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int idUsuario))
            {
                return RedirectToAction("Login", "Usuario");
            }

            if (idProveedor <= 0)
            {
                ViewData["Error"] = "Debe seleccionar un proveedor válido.";
                return View();
            }

            if (idRequerimiento <= 0)
            {
                ViewData["Error"] = "Debe seleccionar un requerimiento aprobado.";
                return View();
            }

            var detalles = new List<DetalleOrdenCompra>();
            if (idProductos != null && cantidades != null && preciosUnitarios != null)
            {
                int count = Math.Min(idProductos.Count, Math.Min(cantidades.Count, preciosUnitarios.Count));
                for (int i = 0; i < count; i++)
                {
                    if (idProductos[i] > 0 && cantidades[i] > 0 && preciosUnitarios[i] >= 0)
                    {
                        decimal subtotal = cantidades[i] * preciosUnitarios[i];
                        detalles.Add(new DetalleOrdenCompra
                        {
                            idProducto = idProductos[i],
                            cantidad = cantidades[i],
                            precioUnitario = preciosUnitarios[i],
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

            var orden = new OrdenCompra
            {
                fechaCompra = DateTime.Now,
                estado = "Pendiente",
                idProveedor = idProveedor,
                idRequerimiento = idRequerimiento,
                idUsuario = idUsuario,
                total = detalles.Sum(d => d.subtotal),
                DetalleOrdenCompras = detalles
            };

            await _context.OrdenesCompra.AddAsync(orden);

            var req = await _context.RequerimientosCompra.FindAsync(idRequerimiento);
            if (req != null && req.estado == "Aprobado")
            {
                req.estado = "Atendido";
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Orden de Compra #{orden.idOrdenCompra} generada correctamente por S/ {orden.total:N2}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var orden = await _context.OrdenesCompra
                .Include(o => o.proveedor)
                .Include(o => o.usuario)
                .Include(o => o.requerimiento)
                    .ThenInclude(r => r!.producto)
                .Include(o => o.DetalleOrdenCompras)
                    .ThenInclude(d => d.producto)
                .Include(o => o.IngresosMercaderia)
                .FirstOrDefaultAsync(o => o.idOrdenCompra == id);

            if (orden == null)
            {
                TempData["Error"] = "Orden de compra no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            return View(orden);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var estadosPermitidos = new[] { "Pendiente", "Emitida", "Recibida", "Anulada" };
            if (!estadosPermitidos.Contains(nuevoEstado))
            {
                TempData["Error"] = "Estado no válido.";
                return RedirectToAction(nameof(Index));
            }

            var orden = await _context.OrdenesCompra.FindAsync(id);
            if (orden == null)
            {
                TempData["Error"] = "Orden de compra no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            orden.estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Orden de Compra #{orden.idOrdenCompra} actualizada a '{nuevoEstado}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}
