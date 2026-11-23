using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Vendedor")]
    public class VentaController : Controller
    {
        private readonly IVentaService _ventaService;

        public VentaController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        private async Task CargarDatosCrearAsync(int? idCotizacion)
        {
            ViewBag.Clientes = await _ventaService.ListarClientesActivosAsync();
            ViewBag.Productos = await _ventaService.ListarProductosActivosAsync();
            ViewBag.CotizacionesAprobadas = await _ventaService.ListarCotizacionesAprobadasAsync();
            ViewBag.IdCotizacionSeleccionada = idCotizacion;

            if (idCotizacion.HasValue && idCotizacion.Value > 0)
            {
                ViewBag.CotizacionSeleccionada = await _ventaService.ObtenerCotizacionAprobadaConDetalleAsync(idCotizacion.Value);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var ventas = await _ventaService.ListarAsync(buscar, estado);
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
            bool aplicarIgv,
            List<int> idProductos,
            List<int> cantidades,
            List<decimal> preciosUnitarios,
            List<decimal> descuentos)
        {
            try
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

                var resultado = await _ventaService.CrearVentaAsync(idCliente, idUsuario, idCotizacion, aplicarIgv, detalles);
                if (!resultado.Ok)
                {
                    ViewData["Error"] = resultado.Error;
                    return View();
                }

                TempData["Mensaje"] = $"Venta #{resultado.IdVenta} registrada correctamente. Stock actualizado.";
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
            var venta = await _ventaService.ObtenerPorIdAsync(id);
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
            try
            {
                var resultado = await _ventaService.AnularVentaAsync(id);
                if (!resultado.Ok)
                {
                    TempData["Error"] = resultado.Error;
                }
                else
                {
                    TempData["Mensaje"] = $"Venta #{resultado.IdVenta} anulada y stock restaurado.";
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
