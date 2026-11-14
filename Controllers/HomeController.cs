using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);
            var hace7Dias = hoy.AddDays(-6);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var ventasHoy = await _context.Ventas
                .Where(v => v.estado != "Anulada" && v.fechaVenta >= hoy && v.fechaVenta < manana)
                .SumAsync(v => (decimal?)v.total) ?? 0m;

            var totalProductos = await _context.Productos.CountAsync(p => p.estado);

            var pagosHoy = await _context.Pagos
                .Include(p => p.venta)
                .Where(p => p.fechaPago >= hoy && p.fechaPago < manana)
                .ToListAsync();

            var cajaHoy = pagosHoy.Sum(p => p.venta?.total ?? p.montoRecibido);

            var stockBajoQuery = _context.Productos
                .Include(p => p.unidadMedida)
                .Where(p => p.estado && p.stockActual <= p.stockMinimo);

            var alertasStockBajo = await stockBajoQuery.CountAsync();
            var productosStockBajo = await stockBajoQuery
                .OrderBy(p => p.stockActual)
                .Take(5)
                .ToListAsync();

            var ventas7DiasRaw = await _context.Ventas
                .Where(v => v.estado != "Anulada" && v.fechaVenta >= hace7Dias && v.fechaVenta < manana)
                .ToListAsync();

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

            var detallesMes = await _context.DetalleVentas
                .Include(d => d.venta)
                .Include(d => d.producto)
                    .ThenInclude(p => p.categoria)
                .Where(d => d.venta.estado != "Anulada" && d.venta.fechaVenta >= inicioMes && d.venta.fechaVenta < manana)
                .ToListAsync();

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

            var topProductos = await _context.DetalleVentas
                .Include(d => d.venta)
                .Include(d => d.producto)
                .Where(d => d.venta.estado != "Anulada")
                .GroupBy(d => d.producto.nombre)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Sum(x => x.cantidad) })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .Select(x => x.Nombre)
                .ToListAsync();

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
