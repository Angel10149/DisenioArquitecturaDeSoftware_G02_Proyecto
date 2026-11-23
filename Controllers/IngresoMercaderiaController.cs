using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Encargado de Compras")]
    public class IngresoMercaderiaController : Controller
    {
        private readonly ICompraService _compraService;

        public IngresoMercaderiaController(ICompraService compraService)
        {
            _compraService = compraService;
        }

        private async Task CargarOrdenesAsync()
        {
            ViewBag.OrdenesCompra = await _compraService.ListarOrdenesPendientesOEmailAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var ingresos = await _compraService.ListarIngresosFiltradosAsync(buscar, estado);
            return View(ingresos);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int? idOrdenCompra)
        {
            await CargarOrdenesAsync();

            ViewBag.IdOrdenSeleccionada = idOrdenCompra;
            if (idOrdenCompra.HasValue && idOrdenCompra.Value > 0)
            {
                ViewBag.OrdenSeleccionada = await _compraService.ObtenerOrdenParaIngresoAsync(idOrdenCompra.Value);
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
            try
            {
                await CargarOrdenesAsync();

                if (idOrdenCompra <= 0)
                {
                    ViewData["Error"] = "Debe seleccionar una orden de compra válida.";
                    return View();
                }

                var orden = await _compraService.ObtenerOrdenParaIngresoAsync(idOrdenCompra);
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

                var resultado = await _compraService.RegistrarIngresoAsync(orden, observaciones, detalles);
                if (!resultado.Ok)
                {
                    ViewData["Error"] = resultado.Error;
                    return View();
                }

                TempData["Mensaje"] = $"Ingreso #{resultado.IdIngreso} registrado correctamente. Stock actualizado.";
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
            var ingreso = await _compraService.ObtenerIngresoDetalleAsync(id);
            if (ingreso == null)
            {
                TempData["Error"] = "Ingreso de mercadería no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(ingreso);
        }
    }
}
