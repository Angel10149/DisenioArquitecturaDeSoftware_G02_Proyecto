using AppWeb.Models;
using AppWeb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public class ReporteService : IReporteService
    {
        private readonly IRepository<Venta> _ventaRepo;
        private readonly IRepository<IngresoMercaderia> _ingresoRepo;
        private readonly IRepository<Pago> _pagoRepo;
        private readonly IRepository<Producto> _productoRepo;
        private readonly IRepository<DetalleIngreso> _detalleIngresoRepo;
        private readonly IRepository<DetalleVenta> _detalleVentaRepo;
        private readonly IRepository<Cliente> _clienteRepo;

        public ReporteService(
            IRepository<Venta> ventaRepo,
            IRepository<IngresoMercaderia> ingresoRepo,
            IRepository<Pago> pagoRepo,
            IRepository<Producto> productoRepo,
            IRepository<DetalleIngreso> detalleIngresoRepo,
            IRepository<DetalleVenta> detalleVentaRepo,
            IRepository<Cliente> clienteRepo)
        {
            _ventaRepo = ventaRepo;
            _ingresoRepo = ingresoRepo;
            _pagoRepo = pagoRepo;
            _productoRepo = productoRepo;
            _detalleIngresoRepo = detalleIngresoRepo;
            _detalleVentaRepo = detalleVentaRepo;
            _clienteRepo = clienteRepo;
        }

        public async Task<IEnumerable<Venta>> ListarVentasPorFechaAsync(DateTime inicio, DateTime finInclusivo)
        {
            return await _ventaRepo.Query()
                .Include(v => v.cliente)
                .Include(v => v.usuario)
                .Include(v => v.DetalleVentas)
                    .ThenInclude(d => d.producto)
                .Where(v => v.estado != "Anulada" && v.fechaVenta >= inicio && v.fechaVenta < finInclusivo)
                .OrderByDescending(v => v.fechaVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<IngresoMercaderia>> ListarIngresosPorFechaAsync(DateTime inicio, DateTime finInclusivo)
        {
            return await _ingresoRepo.Query()
                .Include(i => i.proveedor)
                .Include(i => i.ordenCompra)
                .Include(i => i.DetalleIngresos)
                .Where(i => i.fechaIngreso >= inicio && i.fechaIngreso < finInclusivo)
                .OrderByDescending(i => i.fechaIngreso)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> ListarPagosPorFechaAsync(DateTime inicio, DateTime finInclusivo)
        {
            return await _pagoRepo.Query()
                .Include(p => p.venta)
                    .ThenInclude(v => v!.cliente)
                .Include(p => p.usuario)
                .Where(p => p.fechaPago >= inicio && p.fechaPago < finInclusivo)
                .OrderByDescending(p => p.fechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> ListarProductosStockBajoAsync()
        {
            return await _productoRepo.Query()
                .Include(p => p.categoria)
                .Include(p => p.unidadMedida)
                .Where(p => p.estado && p.stockActual <= p.stockMinimo)
                .OrderBy(p => p.stockActual)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> ListarProductosActivosAsync()
        {
            return await _productoRepo.Query()
                .Include(p => p.categoria)
                .Include(p => p.unidadMedida)
                .Where(p => p.estado)
                .OrderBy(p => p.nombre)
                .ToListAsync();
        }

        public async Task<Dictionary<int, MovimientoProductoResumen>> ObtenerResumenComprasPorProductoAsync(DateTime inicio, DateTime finInclusivo)
        {
            return await _detalleIngresoRepo.Query()
                .Where(d => d.ingresoMercaderia != null &&
                            d.ingresoMercaderia.fechaIngreso >= inicio &&
                            d.ingresoMercaderia.fechaIngreso < finInclusivo)
                .GroupBy(d => d.idProducto)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => new MovimientoProductoResumen(g.Sum(x => x.cantidadRecibida), g.Sum(x => x.subtotal)));
        }

        public async Task<Dictionary<int, MovimientoProductoResumen>> ObtenerResumenVentasPorProductoAsync(DateTime inicio, DateTime finInclusivo)
        {
            return await _detalleVentaRepo.Query()
                .Where(d => d.venta != null &&
                            d.venta.estado != "Anulada" &&
                            d.venta.fechaVenta >= inicio &&
                            d.venta.fechaVenta < finInclusivo)
                .GroupBy(d => d.idproducto)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => new MovimientoProductoResumen(g.Sum(x => x.cantidad), g.Sum(x => x.subtotal)));
        }

        public async Task<decimal> ObtenerTotalVentasRangoAsync(DateTime inicio, DateTime finExclusivo)
        {
            return await _ventaRepo.Query()
                .Where(v => v.estado != "Anulada" && v.fechaVenta >= inicio && v.fechaVenta < finExclusivo)
                .SumAsync(v => (decimal?)v.total) ?? 0m;
        }

        public async Task<int> ContarProductosActivosAsync() =>
            await _productoRepo.Query().CountAsync(p => p.estado);

        public async Task<int> ContarProductosStockBajoAsync() =>
            await _productoRepo.Query().CountAsync(p => p.estado && p.stockActual <= p.stockMinimo);

        public async Task<IEnumerable<Producto>> ListarTopProductosStockBajoAsync(int cantidad)
        {
            return await _productoRepo.Query()
                .Include(p => p.unidadMedida)
                .Where(p => p.estado && p.stockActual <= p.stockMinimo)
                .OrderBy(p => p.stockActual)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetalleVenta>> ListarDetallesVentaMesAsync(DateTime inicioMes, DateTime finExclusivo)
        {
            return await _detalleVentaRepo.Query()
                .Include(d => d.venta)
                .Include(d => d.producto)
                    .ThenInclude(p => p.categoria)
                .Where(d => d.venta.estado != "Anulada" && d.venta.fechaVenta >= inicioMes && d.venta.fechaVenta < finExclusivo)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> ListarTopNombresProductosVendidosAsync(int cantidad)
        {
            return await _detalleVentaRepo.Query()
                .Include(d => d.venta)
                .Include(d => d.producto)
                .Where(d => d.venta.estado != "Anulada")
                .GroupBy(d => d.producto.nombre)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Sum(x => x.cantidad) })
                .OrderByDescending(x => x.Cantidad)
                .Take(cantidad)
                .Select(x => x.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> BuscarProductosRapidoAsync(string filtro, int limite)
        {
            return await _productoRepo.Query()
                .Where(p => p.estado && (p.nombre.Contains(filtro) || p.codigoBarras.Contains(filtro)))
                .OrderBy(p => p.nombre)
                .Take(limite)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> BuscarClientesRapidoAsync(string filtro, int limite)
        {
            return await _clienteRepo.Query()
                .Where(c => c.estado && (c.nombres.Contains(filtro) || c.razonSocial.Contains(filtro) || c.numeroDocumento.Contains(filtro)))
                .OrderBy(c => c.nombres)
                .Take(limite)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> BuscarVentasRapidoAsync(string filtro, int limite)
        {
            return await _ventaRepo.Query()
                .Include(v => v.cliente)
                .Where(v => v.idVenta.ToString() == filtro || v.cliente!.nombres.Contains(filtro) || v.cliente!.razonSocial.Contains(filtro))
                .OrderByDescending(v => v.idVenta)
                .Take(limite)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> ListarUltimasVentasAsync(int limite)
        {
            return await _ventaRepo.Query()
                .Where(v => v.estado != "Anulada")
                .OrderByDescending(v => v.idVenta)
                .Take(limite)
                .ToListAsync();
        }
    }
}
