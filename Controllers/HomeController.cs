using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AppWeb.Controllers
{
    public class DashboardDiaVenta
    {
        public string Dia { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class DashboardCategoriaVenta
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class DashboardVM
    {
        public decimal VentasHoy { get; set; }
        public int TotalProductosActivos { get; set; }
        public decimal CajaHoy { get; set; }
        public int MovimientosCajaHoy { get; set; }
        public int AlertasStockBajo { get; set; }
        public List<Producto> ProductosStockBajo { get; set; } = new();
        public List<DashboardDiaVenta> VentasUltimos7Dias { get; set; } = new();
        public List<DashboardCategoriaVenta> VentasPorCategoriaMes { get; set; } = new();
        public List<string> TopProductos { get; set; } = new();
    }

    [Authorize]
    public class HomeController : Controller
    {
        private readonly IReporteService _reporteService;

        public HomeController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);
            var hace7Dias = hoy.AddDays(-6);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var ventasHoy = await _reporteService.ObtenerTotalVentasRangoAsync(hoy, manana);
            var totalProductos = await _reporteService.ContarProductosActivosAsync();
            var pagosHoy = (await _reporteService.ListarPagosPorFechaAsync(hoy, manana)).ToList();
            var cajaHoy = pagosHoy.Sum(p => p.venta?.total ?? p.montoRecibido);

            var alertasStockBajo = await _reporteService.ContarProductosStockBajoAsync();
            var productosStockBajo = (await _reporteService.ListarTopProductosStockBajoAsync(5)).ToList();

            var ventas7DiasRaw = (await _reporteService.ListarVentasPorFechaAsync(hace7Dias, manana)).ToList();
            var ventas7Dias = Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var fecha = hace7Dias.AddDays(offset);
                    return new DashboardDiaVenta
                    {
                        Dia = fecha.ToString("dd/MM"),
                        Total = ventas7DiasRaw.Where(v => v.fechaVenta.Date == fecha).Sum(v => v.total)
                    };
                })
                .ToList();

            var detallesMes = await _reporteService.ListarDetallesVentaMesAsync(inicioMes, manana);
            var ventasPorCategoria = detallesMes
                .GroupBy(d => d.producto?.categoria?.nombre ?? "Otros")
                .Select(g => new DashboardCategoriaVenta
                {
                    Categoria = g.Key,
                    Total = g.Sum(x => x.subtotal)
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            var topProductos = (await _reporteService.ListarTopNombresProductosVendidosAsync(5)).ToList();

            var vm = new DashboardVM
            {
                VentasHoy = ventasHoy,
                TotalProductosActivos = totalProductos,
                CajaHoy = cajaHoy,
                MovimientosCajaHoy = pagosHoy.Count,
                AlertasStockBajo = alertasStockBajo,
                ProductosStockBajo = productosStockBajo,
                VentasUltimos7Dias = ventas7Dias,
                VentasPorCategoriaMes = ventasPorCategoria,
                TopProductos = topProductos
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarGlobal(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(Array.Empty<object>());
            }

            string filtro = q.Trim();

            var productos = (await _reporteService.BuscarProductosRapidoAsync(filtro, 4))
                .Select(p => new
                {
                    tipo = "Producto",
                    icono = "fa-box",
                    titulo = p.nombre,
                    subtitulo = $"Stock: {p.stockActual} · S/ {p.precioVenta:N2}",
                    url = Url.Action("Index", "Producto", new { buscar = p.nombre })
                });

            var clientes = (await _reporteService.BuscarClientesRapidoAsync(filtro, 4))
                .Select(c => new
                {
                    tipo = "Cliente",
                    icono = "fa-user",
                    titulo = string.IsNullOrWhiteSpace(c.razonSocial) ? c.nombres : c.razonSocial,
                    subtitulo = $"{c.tipoDocumento}: {c.numeroDocumento}",
                    url = Url.Action("Index", "Cliente", new { buscar = c.numeroDocumento })
                });

            var ventas = (await _reporteService.BuscarVentasRapidoAsync(filtro, 3))
                .Select(v => new
                {
                    tipo = "Venta",
                    icono = "fa-shopping-cart",
                    titulo = $"Venta #{v.idVenta}",
                    subtitulo = $"S/ {v.total:N2} · {v.estado}",
                    url = Url.Action("Detalle", "Venta", new { id = v.idVenta })
                });

            return Json(productos.Concat<object>(clientes).Concat(ventas));
        }

        [HttpGet]
        public async Task<IActionResult> Notificaciones()
        {
            var items = new List<object>();

            var stockBajo = await _reporteService.ListarTopProductosStockBajoAsync(3);
            foreach (var p in stockBajo)
            {
                items.Add(new
                {
                    icono = "fa-exclamation-triangle",
                    color = "#DC3545",
                    texto = $"Stock bajo: {p.nombre} ({p.stockActual} {p.unidadMedida?.abreviatura ?? "UNI"})",
                    tiempo = "Inventario",
                    url = Url.Action("Index", "Producto", new { buscar = p.nombre })
                });
            }

            var ultimasVentas = await _reporteService.ListarUltimasVentasAsync(2);
            foreach (var v in ultimasVentas)
            {
                items.Add(new
                {
                    icono = "fa-shopping-cart",
                    color = "#304D8A",
                    texto = $"Venta #{v.idVenta} por S/ {v.total:N2} ({v.estado})",
                    tiempo = v.fechaVenta.ToString("dd/MM HH:mm"),
                    url = Url.Action("Detalle", "Venta", new { id = v.idVenta })
                });
            }

            return Json(items);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
