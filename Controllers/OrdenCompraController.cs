using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Encargado de Compras")]
    public class OrdenCompraController : Controller
    {
        private readonly ICompraService _compraService;

        public OrdenCompraController(ICompraService compraService)
        {
            _compraService = compraService;
        }

        private async Task CargarListasAsync()
        {
            ViewBag.Proveedores = await _compraService.ListarProveedoresActivosAsync();
            ViewBag.Requerimientos = await _compraService.ListarRequerimientosAprobadosAsync();
            ViewBag.Productos = await _compraService.ListarProductosActivosAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            ViewBag.Buscar = buscar;
            ViewBag.Estado = estado;

            var ordenes = await _compraService.ListarOrdenesCompraFiltradasAsync(buscar, estado);
            return View(ordenes);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int? idRequerimiento)
        {
            await CargarListasAsync();

            ViewBag.IdRequerimientoSeleccionado = idRequerimiento;
            if (idRequerimiento.HasValue && idRequerimiento.Value > 0)
            {
                ViewBag.RequerimientoSeleccionado = await _compraService.ObtenerRequerimientoConProductoAsync(idRequerimiento.Value);
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
            try
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
                        int idProd = idProductos[i];
                        int cant = cantidades[i];
                        decimal precio = preciosUnitarios[i];

                        if (idProd <= 0) continue;

                        if (cant <= 0)
                        {
                            ViewData["Error"] = "La cantidad de cada producto debe ser mayor a 0";
                            return View();
                        }

                        if (precio < 0)
                        {
                            ViewData["Error"] = "El precio unitario no puede ser negativo";
                            return View();
                        }

                        detalles.Add(new DetalleOrdenCompra
                        {
                            idProducto = idProd,
                            cantidad = cant,
                            precioUnitario = precio,
                            subtotal = cant * precio
                        });
                    }
                }

                if (!detalles.Any())
                {
                    ViewData["Error"] = "Debe agregar al menos un producto con cantidad mayor a 0";
                    return View();
                }

                var resultado = await _compraService.CrearOrdenCompraAsync(idProveedor, idRequerimiento, idUsuario, detalles);
                if (!resultado.Ok || resultado.Orden == null)
                {
                    ViewData["Error"] = resultado.Error ?? "No se pudo crear la orden de compra";
                    return View();
                }
                TempData["Mensaje"] = $"Orden de Compra #{resultado.Orden!.idOrdenCompra} generada correctamente por S/ {resultado.Orden.total:N2}.";
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
            var orden = await _compraService.ObtenerOrdenCompraDetalleAsync(id);
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
            try
            {
                var resultado = await _compraService.CambiarEstadoOrdenCompraAsync(id, nuevoEstado);
                if (!resultado.Ok)
                {
                    TempData["Error"] = resultado.Error;
                }
                else
                {
                    TempData["Mensaje"] = $"Orden de Compra #{id} actualizada a '{nuevoEstado}'.";
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
