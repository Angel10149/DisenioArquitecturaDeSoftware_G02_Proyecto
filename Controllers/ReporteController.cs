using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public class ProductoMovimientoReporte
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public int CantidadComprada { get; set; }
        public decimal TotalComprado { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }

    public class ReporteResumenVM
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<Venta> Ventas { get; set; } = new();
        public List<IngresoMercaderia> Ingresos { get; set; } = new();
        public List<Pago> Pagos { get; set; } = new();
        public List<Producto> ProductosStockBajo { get; set; } = new();
        public List<TopProductoReporte> TopProductos { get; set; } = new();
        public List<ProductoMovimientoReporte> MovimientosProductos { get; set; } = new();
        public decimal TotalVentas { get; set; }
        public decimal TotalCompras { get; set; }
        public decimal TotalCobradoCaja { get; set; }
    }

    [Authorize(Roles = "Administrador")]
    public class ReporteController : Controller
    {
        private readonly IReporteService _reporteService;

        public ReporteController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            DateTime inicio = (fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            DateTime fin = (fechaFin ?? DateTime.Today).Date;
            if (fin < inicio) fin = inicio;
            DateTime finInclusivo = fin.AddDays(1);

            var ventas = (await _reporteService.ListarVentasPorFechaAsync(inicio, finInclusivo)).ToList();
            var ingresos = (await _reporteService.ListarIngresosPorFechaAsync(inicio, finInclusivo)).ToList();
            var pagos = (await _reporteService.ListarPagosPorFechaAsync(inicio, finInclusivo)).ToList();
            var stockBajo = (await _reporteService.ListarProductosStockBajoAsync()).ToList();

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

            var productosActivos = (await _reporteService.ListarProductosActivosAsync()).ToList();

            var comprasPorProd = ingresos
                .SelectMany(i => i.DetalleIngresos)
                .GroupBy(d => d.idProducto)
                .ToDictionary(
                    g => g.Key,
                    g => (Cantidad: g.Sum(x => x.cantidadRecibida), Total: g.Sum(x => x.subtotal)));

            var ventasPorProd = ventas
                .SelectMany(v => v.DetalleVentas)
                .GroupBy(d => d.idproducto)
                .ToDictionary(
                    g => g.Key,
                    g => (Cantidad: g.Sum(x => x.cantidad), Total: g.Sum(x => x.subtotal)));

            var movimientosProductos = productosActivos
                .Select(p =>
                {
                    comprasPorProd.TryGetValue(p.idProducto, out var comp);
                    ventasPorProd.TryGetValue(p.idProducto, out var vend);
                    return new ProductoMovimientoReporte
                    {
                        IdProducto = p.idProducto,
                        Codigo = p.codigoBarras,
                        Nombre = p.nombre,
                        Categoria = p.categoria?.nombre ?? "-",
                        Unidad = p.unidadMedida?.abreviatura ?? "-",
                        CantidadComprada = comp.Cantidad,
                        TotalComprado = comp.Total,
                        CantidadVendida = vend.Cantidad,
                        TotalVendido = vend.Total,
                        StockActual = p.stockActual,
                        StockMinimo = p.stockMinimo
                    };
                })
                .OrderByDescending(m => m.CantidadComprada + m.CantidadVendida)
                .ThenBy(m => m.Nombre)
                .ToList();

            var vm = new ReporteResumenVM
            {
                FechaInicio = inicio,
                FechaFin = fin,
                Ventas = ventas,
                Ingresos = ingresos,
                Pagos = pagos,
                ProductosStockBajo = stockBajo,
                TopProductos = topProductos,
                MovimientosProductos = movimientosProductos,
                TotalVentas = ventas.Sum(v => v.total),
                TotalCompras = ingresos.Sum(i => i.DetalleIngresos?.Sum(d => d.subtotal) ?? 0),
                TotalCobradoCaja = pagos.Sum(p => p.venta?.total ?? p.montoRecibido)
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

            var ventas = await _reporteService.ListarVentasPorFechaAsync(inicio, finInclusivo);

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

        [HttpGet]
        public async Task<IActionResult> ExportarComprasCsv(DateTime? fechaInicio, DateTime? fechaFin)
        {
            DateTime inicio = (fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            DateTime fin = (fechaFin ?? DateTime.Today).Date;
            if (fin < inicio) fin = inicio;
            DateTime finInclusivo = fin.AddDays(1);

            var ingresos = await _reporteService.ListarIngresosPorFechaAsync(inicio, finInclusivo);

            var sb = new StringBuilder();
            sb.AppendLine("IdIngreso;Fecha;Proveedor;RUC;OrdenCompra;Items;Estado;Total");
            foreach (var ing in ingresos)
            {
                var prov = ing.proveedor?.razonSocial ?? "-";
                var ruc = ing.proveedor?.ruc ?? "-";
                var oc = $"OC-{ing.idOrdenCompra}";
                var items = ing.DetalleIngresos?.Count ?? 0;
                var total = ing.DetalleIngresos?.Sum(d => d.subtotal) ?? 0m;
                sb.AppendLine($"{ing.idIngresoMercaderia};{ing.fechaIngreso:yyyy-MM-dd HH:mm};{prov};{ruc};{oc};{items};{ing.estado};{total:0.00}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Reporte_Compras_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportarInventarioCsv()
        {
            var productos = await _reporteService.ListarProductosActivosAsync();

            var sb = new StringBuilder();
            sb.AppendLine("IdProducto;CodigoBarras;Producto;Categoria;Unidad;StockActual;StockMinimo;EstadoStock;PrecioCosto;PrecioVenta");
            foreach (var p in productos)
            {
                var cat = p.categoria?.nombre ?? "-";
                var uni = p.unidadMedida?.abreviatura ?? "-";
                var estadoStock = p.stockActual <= p.stockMinimo ? "Stock Bajo" : "Normal";
                sb.AppendLine($"{p.idProducto};{p.codigoBarras};{p.nombre};{cat};{uni};{p.stockActual};{p.stockMinimo};{estadoStock};{p.precioCosto:0.00};{p.precioVenta:0.00}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Reporte_Inventario_{DateTime.Today:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportarCajaCsv(DateTime? fechaInicio, DateTime? fechaFin)
        {
            DateTime inicio = (fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            DateTime fin = (fechaFin ?? DateTime.Today).Date;
            if (fin < inicio) fin = inicio;
            DateTime finInclusivo = fin.AddDays(1);

            var pagos = await _reporteService.ListarPagosPorFechaAsync(inicio, finInclusivo);

            var sb = new StringBuilder();
            sb.AppendLine("IdPago;Fecha;IdVenta;Cliente;MetodoPago;Cajero;MontoRecibido;TotalVenta");
            foreach (var p in pagos)
            {
                var cli = !string.IsNullOrWhiteSpace(p.venta?.cliente?.razonSocial)
                    ? p.venta.cliente.razonSocial
                    : (p.venta?.cliente?.nombres ?? "-");
                var cajero = p.usuario?.nombreUsuario ?? "-";
                var totalVenta = p.venta?.total ?? p.montoRecibido;
                sb.AppendLine($"{p.idPago};{p.fechaPago:yyyy-MM-dd HH:mm};{p.idVenta};{cli};{p.metodoPago};{cajero};{p.montoRecibido:0.00};{totalVenta:0.00}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Reporte_Caja_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportarProductosMovimientoCsv(DateTime? fechaInicio, DateTime? fechaFin)
        {
            DateTime inicio = (fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            DateTime fin = (fechaFin ?? DateTime.Today).Date;
            if (fin < inicio) fin = inicio;
            DateTime finInclusivo = fin.AddDays(1);

            var productosActivos = await _reporteService.ListarProductosActivosAsync();

            var comprasPorProd = await _reporteService.ObtenerResumenComprasPorProductoAsync(inicio, finInclusivo);

            var ventasPorProd = await _reporteService.ObtenerResumenVentasPorProductoAsync(inicio, finInclusivo);

            var sb = new StringBuilder();
            sb.AppendLine("IdProducto;CodigoBarras;Producto;Categoria;Unidad;CantComprada;TotalComprado;CantVendida;TotalVendido;StockActual;StockMinimo");
            foreach (var p in productosActivos)
            {
                comprasPorProd.TryGetValue(p.idProducto, out var comp);
                ventasPorProd.TryGetValue(p.idProducto, out var vend);
                int cantComp = comp?.Cantidad ?? 0;
                decimal totComp = comp?.Total ?? 0m;
                int cantVend = vend?.Cantidad ?? 0;
                decimal totVend = vend?.Total ?? 0m;
                string cat = p.categoria?.nombre ?? "-";
                string uni = p.unidadMedida?.abreviatura ?? "-";
                sb.AppendLine($"{p.idProducto};{p.codigoBarras};{p.nombre};{cat};{uni};{cantComp};{totComp:0.00};{cantVend};{totVend:0.00};{p.stockActual};{p.stockMinimo}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Reporte_Cuadre_Productos_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.csv");
        }

        [HttpGet]
        public Task<IActionResult> ExportarCsv(string tipo, DateTime? fechaInicio, DateTime? fechaFin)
        {
            return (tipo?.Trim().ToLowerInvariant()) switch
            {
                "compras" => ExportarComprasCsv(fechaInicio, fechaFin),
                "inventario" => ExportarInventarioCsv(),
                "productos_cuadre" => ExportarProductosMovimientoCsv(fechaInicio, fechaFin),
                "caja" => ExportarCajaCsv(fechaInicio, fechaFin),
                _ => ExportarVentasCsv(fechaInicio, fechaFin)
            };
        }
    }
}
