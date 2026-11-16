using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Cajero")]
    public class CajaController : Controller
    {
        private readonly ICajaService _cajaService;

        public CajaController(ICajaService cajaService)
        {
            _cajaService = cajaService;
        }

        private async Task CargarVentasPendientesAsync(int? idVenta)
        {
            ViewBag.VentasPendientes = await _cajaService.ListarVentasPendientesAsync();
            ViewBag.IdVentaSeleccionada = idVenta;
            if (idVenta.HasValue && idVenta.Value > 0)
            {
                ViewBag.VentaSeleccionada = await _cajaService.ObtenerVentaPendientePorIdAsync(idVenta.Value);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar, string? metodoPago)
        {
            ViewBag.Buscar = buscar;
            ViewBag.MetodoPago = metodoPago;
            ViewBag.VentasPendientes = await _cajaService.ListarVentasPendientesAsync();

            var pagos = await _cajaService.ListarPagosAsync(buscar, metodoPago);
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

            var resultado = await _cajaService.RegistrarCobroAsync(idVenta, idUsuario, metodoPago, montoRecibido, tipoComprobante);
            if (!resultado.Ok)
            {
                ViewData["Error"] = resultado.Error;
                return View();
            }

            TempData["Mensaje"] = $"Pago #{resultado.IdPago} registrado. Comprobante {resultado.SerieNumero} emitido.";
            return RedirectToAction(nameof(Comprobante), new { id = resultado.IdPago });
        }

        [HttpGet]
        public async Task<IActionResult> Comprobante(int id)
        {
            var pago = await _cajaService.ObtenerComprobantePorPagoAsync(id);
            if (pago == null)
            {
                TempData["Error"] = "Pago o comprobante no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(pago);
        }
    }
}
