using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Controllers
{
    [Authorize]
    public class IngresoMercaderiaController : Controller
    {
        private readonly AppDbContext _context;

        public IngresoMercaderiaController(AppDbContext context)
        {
            _context = context;
        }

        private async Task CargarOrdenesAsync()
        {
            ViewBag.OrdenesCompra = await _context.OrdenesCompra
                .Include(o => o.proveedor)
                .Where(o => o.estado == "Pendiente" || o.estado == "Emitida")
                .OrderByDescending(o => o.idOrdenCompra)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            var query = _context.IngresosMercaderia
                .Include(i => i.ordenCompra)
                .Include(i => i.proveedor)
                .Include(i => i.DetalleIngresos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(i => i.estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(i =>
                    i.proveedor!.razonSocial.Contains(filtro) ||
                    i.observaciones.Contains(filtro) ||
                    i.idOrdenCompra.ToString() == filtro ||
                    i.idIngresoMercaderia.ToString() == filtro);
            }

            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var ingresos = await query.OrderByDescending(i => i.idIngresoMercaderia).ToListAsync();
            return View(ingresos);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int? idOrdenCompra)
        {
            await CargarOrdenesAsync();

            ViewBag.IdOrdenSeleccionada = idOrdenCompra;
            if (idOrdenCompra.HasValue && idOrdenCompra.Value > 0)
            {
                var orden = await _context.OrdenesCompra
                    .Include(o => o.proveedor)
                    .Include(o => o.DetalleOrdenCompras)
                        .ThenInclude(d => d.producto)
                    .FirstOrDefaultAsync(o => o.idOrdenCompra == idOrdenCompra.Value);

                ViewBag.OrdenSeleccionada = orden;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            int idOrdenCompra,
            string? observaciones,
            List<int> idProductos,
            List<int> cantidadesRecibidas,
            List<int> cantidadesRechazadas,
            List<decimal> costosUnitarios)
        {
            await CargarOrdenesAsync();

            if (idOrdenCompra <= 0)
            {
                ViewData["Error"] = "Debe seleccionar una orden de compra válida.";
                return View();
            }

            var orden = await _context.OrdenesCompra
                .Include(o => o.proveedor)
                .Include(o => o.DetalleOrdenCompras)
                    .ThenInclude(d => d.producto)
                .FirstOrDefaultAsync(o => o.idOrdenCompra == idOrdenCompra);

            if (orden == null)
            {
                ViewData["Error"] = "La orden de compra seleccionada no existe.";
                return View();
            }

            if (orden.estado == "Recibida" || orden.estado == "Anulada")
            {
                ViewData["Error"] = $"La orden #{orden.idOrdenCompra} ya se encuentra en estado '{orden.estado}'.";
                return View();
            }

            ViewBag.IdOrdenSeleccionada = idOrdenCompra;
            ViewBag.OrdenSeleccionada = orden;

            var detalles = new List<DetalleIngreso>();
            if (idProductos != null && cantidadesRecibidas != null && cantidadesRechazadas != null && costosUnitarios != null)
            {
                int count = Math.Min(idProductos.Count,
                    Math.Min(cantidadesRecibidas.Count,
                    Math.Min(cantidadesRechazadas.Count, costosUnitarios.Count)));

                for (int i = 0; i < count; i++)
                {
                    int idProd = idProductos[i];
                    int rec = Math.Max(0, cantidadesRecibidas[i]);
                    int rech = Math.Max(0, cantidadesRechazadas[i]);
                    decimal costo = Math.Max(0, costosUnitarios[i]);

                    if (idProd > 0 && (rec > 0 || rech > 0))
                    {
                        detalles.Add(new DetalleIngreso
                        {
                            idProducto = idProd,
                            cantidadRecibida = rec,
                            cantidadRechazada = rech,
                            costoUnitario = costo,
                            subtotal = rec * costo
                        });
                    }
                }
            }

            if (!detalles.Any())
            {
                ViewData["Error"] = "Debe ingresar al menos una cantidad recibida o rechazada mayor a 0.";
                return View();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ingreso = new IngresoMercaderia
                {
                    fechaIngreso = DateTime.Now,
                    estado = "Registrado",
                    observaciones = observaciones?.Trim() ?? string.Empty,
                    idOrdenCompra = orden.idOrdenCompra,
                    idProveedor = orden.idProveedor,
                    DetalleIngresos = detalles
                };

                await _context.IngresosMercaderia.AddAsync(ingreso);

                foreach (var det in detalles)
                {
                    if (det.cantidadRecibida > 0)
                    {
                        var producto = await _context.Productos.FindAsync(det.idProducto);
                        if (producto != null)
                        {
                            producto.stockActual += det.cantidadRecibida;
                            if (det.costoUnitario > 0)
                            {
                                producto.precioCosto = det.costoUnitario;
                            }

                            await _context.Stocks.AddAsync(new Stock
                            {
                                idProducto = producto.idProducto,
                                cantidad = producto.stockActual,
                                fecha_actualizacion = DateTime.Now
                            });
                        }
                    }
                }

                orden.estado = "Recibida";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Mensaje"] = $"Ingreso #{ingreso.idIngresoMercaderia} registrado correctamente. Stock actualizado.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                ViewData["Error"] = "Ocurrió un error al registrar el ingreso de mercadería.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var ingreso = await _context.IngresosMercaderia
                .Include(i => i.ordenCompra)
                .Include(i => i.proveedor)
                .Include(i => i.DetalleIngresos)
                    .ThenInclude(d => d.producto)
                .FirstOrDefaultAsync(i => i.idIngresoMercaderia == id);

            if (ingreso == null)
            {
                TempData["Error"] = "Ingreso de mercadería no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(ingreso);
        }
    }
}
