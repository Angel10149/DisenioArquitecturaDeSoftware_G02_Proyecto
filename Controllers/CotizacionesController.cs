using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Vendedor")]
    public class CotizacionesController : Controller
    {
        private readonly IVentaService _ventaService;

        public CotizacionesController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        private async Task CargarDatosCrearAsync()
        {
            ViewBag.Clientes = await _ventaService.ListarClientesActivosAsync();
            ViewBag.Productos = await _ventaService.ListarProductosActivosAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var cotizaciones = await _ventaService.ListarCotizacionesAsync(buscar, estado);
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
            try
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

                await _ventaService.CrearCotizacionAsync(cotizacion);

                TempData["Mensaje"] = $"Cotización #{cotizacion.idCotizacion} registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["Error"] = DbExceptionHelper.Traducir(ex);
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var cotizacion = await _ventaService.ObtenerCotizacionDetalleAsync(id);
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
            try
            {
                var resultado = await _ventaService.CambiarEstadoCotizacionAsync(id, nuevoEstado);
                if (!resultado.Ok)
                {
                    TempData["Error"] = resultado.Error;
                }
                else
                {
                    TempData["Mensaje"] = $"Cotización #{resultado.IdVenta} actualizada a '{nuevoEstado}'.";
                }

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
