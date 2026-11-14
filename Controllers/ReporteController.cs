using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AppWeb.Controllers
{
    public class TopProductoReporte
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
    }

    public class ReporteResumenVM
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<Venta> Ventas { get; set; } = new();
        public List<IngresoMercaderia> Ingresos { get; set; } = new();
        public List<Producto> ProductosStockBajo { get; set; } = new();
        public List<TopProductoReporte> TopProductos { get; set; } = new();
        public decimal TotalVentas { get; set; }
        public decimal TotalCompras { get; set; }
    }

    [Authorize]
    public class ReporteController : Controller
    {
        private readonly AppDbContext _context;

        public ReporteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            DateTime inicio = (fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            DateTime fin = (fechaFin ?? DateTime.Today).Date;
            if (fin < inicio) fin = inicio;
            DateTime finInclusivo = fin.AddDays(1);

            var ventas = await _context.Ventas
                .Include(v => v.cliente)
                .Include(v => v.usuario)
                .Include(v => v.DetalleVentas)
                    .ThenInclude(d => d.producto)
                .Where(v => v.estado != "Anulada" && v.fechaVenta >= inicio && v.fechaVenta < finInclusivo)
                .OrderByDescending(v => v.fechaVenta)
                .ToListAsync();

            var ingresos = await _context.IngresosMercaderia
                .Include(i => i.proveedor)
                .Include(i => i.DetalleIngresos)
                .Where(i => i.fechaIngreso >= inicio && i.fechaIngreso < finInclusivo)
                .OrderByDescending(i => i.fechaIngreso)
                .ToListAsync();

            var stockBajo = await _context.Productos
                .Include(p => p.categoria)
                .Include(p => p.unidadMedida)
                .Where(p => p.estado && p.stockActual <= p.stockMinimo)
                .OrderBy(p => p.stockActual)
                .ToListAsync();

            var topProductos = ventas
                .SelectMany(v => v.DetalleVentas)
                .GroupBy(d => new
                {
                    d.idproducto,
                    Nombre = d.producto?.nombre ?? "Producto",
                    Codigo = d.producto?.codigoBarras ?? "-"
                })
                .Select(g => new TopProductoReporte
                {
                    Nombre = g.Key.Nombre,
                    Codigo = g.Key.Codigo,
                    CantidadVendida = g.Sum(x => x.cantidad),
                    TotalVendido = g.Sum(x => x.subtotal)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .Take(10)
                .ToList();

            var vm = new ReporteResumenVM
            {
                FechaInicio = inicio,
                FechaFin = fin,
                Ventas = ventas,
                Ingresos = ingresos,
                ProductosStockBajo = stockBajo,
                TopProductos = topProductos,
                TotalVentas = ventas.Sum(v => v.total),
                TotalCompras = ingresos.Sum(i => i.DetalleIngresos?.Sum(d => d.subtotal) ?? 0)
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportarVentasCsv(DateTime? fechaInicio, DateTime? fechaFin)
        {
            DateTime inicio = (fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            DateTime fin = (fechaFin ?? DateTime.Today).Date;
            if (fin < inicio) fin = inicio;
            DateTime finInclusivo = fin.AddDays(1);

            var ventas = await _context.Ventas
                .Include(v => v.cliente)
                .Include(v => v.usuario)
                .Where(v => v.estado != "Anulada" && v.fechaVenta >= inicio && v.fechaVenta < finInclusivo)
                .OrderByDescending(v => v.fechaVenta)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("IdVenta;Fecha;Cliente;Documento;Vendedor;Estado;Total");
            foreach (var v in ventas)
            {
                var cli = !string.IsNullOrWhiteSpace(v.cliente?.razonSocial) ? v.cliente.razonSocial : (v.cliente?.nombres ?? "-");
                var doc = v.cliente?.numeroDocumento ?? "-";
                var vend = v.usuario?.nombreUsuario ?? "-";
                sb.AppendLine($"{v.idVenta};{v.fechaVenta:yyyy-MM-dd HH:mm};{cli};{doc};{vend};{v.estado};{v.total:0.00}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Reporte_Ventas_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.csv");
        }
    }
}
