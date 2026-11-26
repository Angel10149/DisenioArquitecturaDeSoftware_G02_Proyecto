using AppWeb.Data;
using AppWeb.Models;
using AppWeb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public class VentaService : IVentaService
    {
        private readonly IRepository<Venta> _ventaRepo;
        private readonly IRepository<Cliente> _clienteRepo;
        private readonly IRepository<Cotizacion> _cotizacionRepo;
        private readonly IRepository<Producto> _productoRepo;
        private readonly IRepository<Stock> _stockRepo;
        private readonly IRepository<ConfiguracionEmpresa> _configRepo;
        private readonly AppDbContext _context;

        public VentaService(
            IRepository<Venta> ventaRepo,
            IRepository<Cliente> clienteRepo,
            IRepository<Cotizacion> cotizacionRepo,
            IRepository<Producto> productoRepo,
            IRepository<Stock> stockRepo,
            IRepository<ConfiguracionEmpresa> configRepo,
            AppDbContext context)
        {
            _ventaRepo = ventaRepo;
            _clienteRepo = clienteRepo;
            _cotizacionRepo = cotizacionRepo;
            _productoRepo = productoRepo;
            _stockRepo = stockRepo;
            _configRepo = configRepo;
            _context = context;
        }

        public async Task<IEnumerable<Venta>> ListarAsync(string? buscar, string? estado)
        {
            var query = _ventaRepo.Query()
                .Include(v => v.cliente)
                .Include(v => v.usuario)
                .Include(v => v.cotizacion)
                .Include(v => v.DetalleVentas)
                .Include(v => v.Pagos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (string.Equals(estado, "Pagada", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(v => v.estado == "Pagada" || v.estado == "Completada");
                }
                else
                {
                    query = query.Where(v => v.estado == estado);
                }
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(v =>
                    v.cliente!.nombres.Contains(filtro) ||
                    v.cliente!.razonSocial.Contains(filtro) ||
                    v.cliente!.numeroDocumento.Contains(filtro) ||
                    v.usuario!.nombreUsuario.Contains(filtro) ||
                    v.idVenta.ToString() == filtro);
            }

            return await query.OrderByDescending(v => v.idVenta).ToListAsync();
        }

        public async Task<Venta?> ObtenerPorIdAsync(int id)
        {
            return await _ventaRepo.Query()
                .Include(v => v.cliente)
                .Include(v => v.usuario)
                .Include(v => v.cotizacion)
                .Include(v => v.DetalleVentas)
                    .ThenInclude(d => d.producto)
                .Include(v => v.Pagos)
                    .ThenInclude(p => p.Comprobantes)
                .FirstOrDefaultAsync(v => v.idVenta == id);
        }

        public async Task<IEnumerable<Cliente>> ListarClientesActivosAsync()
        {
            return await _clienteRepo.Query()
                .Where(c => c.estado)
                .OrderBy(c => c.nombres)
                .ThenBy(c => c.razonSocial)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> ListarProductosActivosAsync()
        {
            return await _productoRepo.Query()
                .Where(p => p.estado)
                .OrderBy(p => p.nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cotizacion>> ListarCotizacionesAprobadasAsync()
        {
            return await _cotizacionRepo.Query()
                .Include(c => c.cliente)
                .Where(c => c.estado == "Aprobada")
                .OrderByDescending(c => c.idCotizacion)
                .ToListAsync();
        }

        public async Task<Cotizacion?> ObtenerCotizacionAprobadaConDetalleAsync(int idCotizacion)
        {
            return await _cotizacionRepo.Query()
                .Include(c => c.cliente)
                .Include(c => c.DetalleCotizacions)
                    .ThenInclude(d => d.producto)
                .FirstOrDefaultAsync(c => c.idCotizacion == idCotizacion && c.estado == "Aprobada");
        }

        public async Task<VentaOperacionResultado> CrearVentaAsync(int idCliente, int idUsuario, int? idCotizacion, bool aplicarIgv, List<DetalleVenta> detalles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cantidadesPorProducto = detalles
                    .GroupBy(d => d.idproducto)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.cantidad));

                foreach (var kvp in cantidadesPorProducto)
                {
                    var producto = await _productoRepo.GetByIdAsync(kvp.Key);
                    if (producto == null || !producto.estado)
                    {
                        await transaction.RollbackAsync();
                        return new VentaOperacionResultado(false, "Uno de los productos seleccionados no existe o está inactivo.", null);
                    }

                    if (producto.stockActual < kvp.Value)
                    {
                        await transaction.RollbackAsync();
                        return new VentaOperacionResultado(false, $"Stock insuficiente para '{producto.nombre}'. Disponible: {producto.stockActual}, solicitado: {kvp.Value}.", null);
                    }

                    producto.stockActual -= kvp.Value;
                    _productoRepo.Update(producto);
                    await _stockRepo.AddAsync(new Stock
                    {
                        idProducto = producto.idProducto,
                        cantidad = producto.stockActual,
                        fecha_actualizacion = DateTime.Now
                    });
                }

                if (idCotizacion.HasValue && idCotizacion.Value > 0)
                {
                    var cotizacion = await _cotizacionRepo.GetByIdAsync(idCotizacion.Value);
                    if (cotizacion == null)
                    {
                        await transaction.RollbackAsync();
                        return new VentaOperacionResultado(false, "La cotización seleccionada no existe", null);
                    }
                    if (cotizacion.estado != "Aprobada")
                    {
                        await transaction.RollbackAsync();
                        return new VentaOperacionResultado(false, $"La cotización #{cotizacion.idCotizacion} no puede convertirse a venta porque su estado es '{cotizacion.estado}'", null);
                    }
                    cotizacion.estado = "Convertida";
                    _cotizacionRepo.Update(cotizacion);
                }

                var subtotalVenta = detalles.Sum(d => d.subtotal);
                decimal totalVenta = subtotalVenta;
                if (aplicarIgv)
                {
                    var cfg = await _configRepo.Query().FirstOrDefaultAsync();
                    var igvPct = cfg?.igv ?? 18m;
                    totalVenta = Math.Round(subtotalVenta * (1m + (igvPct / 100m)), 2);
                }

                var venta = new Venta
                {
                    fechaVenta = DateTime.Now,
                    estado = "Pendiente",
                    total = totalVenta,
                    idCliente = idCliente,
                    idUsuario = idUsuario,
                    idCotizacion = (idCotizacion.HasValue && idCotizacion.Value > 0) ? idCotizacion.Value : null,
                    DetalleVentas = detalles
                };

                await _ventaRepo.AddAsync(venta);
                await _ventaRepo.SaveChangesAsync();
                await transaction.CommitAsync();

                return new VentaOperacionResultado(true, null, venta.idVenta);
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return new VentaOperacionResultado(false, DbExceptionHelper.Traducir(ex), null);
            }
        }

        public async Task<VentaOperacionResultado> AnularVentaAsync(int idVenta)
        {
            var venta = await _ventaRepo.Query()
                .Include(v => v.DetalleVentas)
                .FirstOrDefaultAsync(v => v.idVenta == idVenta);

            if (venta == null)
                return new VentaOperacionResultado(false, "Venta no encontrada", null);

            if (venta.estado != "Pendiente")
                return new VentaOperacionResultado(false, $"Solo se pueden anular ventas en estado 'Pendiente'. Estado actual: '{venta.estado}'", null);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var det in venta.DetalleVentas)
                {
                    var producto = await _productoRepo.GetByIdAsync(det.idproducto);
                    if (producto != null)
                    {
                        producto.stockActual += det.cantidad;
                        _productoRepo.Update(producto);
                        await _stockRepo.AddAsync(new Stock
                        {
                            idProducto = producto.idProducto,
                            cantidad = producto.stockActual,
                            fecha_actualizacion = DateTime.Now
                        });
                    }
                }

                venta.estado = "Anulada";
                _ventaRepo.Update(venta);
                await _ventaRepo.SaveChangesAsync();
                await transaction.CommitAsync();

                return new VentaOperacionResultado(true, null, venta.idVenta);
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return new VentaOperacionResultado(false, DbExceptionHelper.Traducir(ex), null);
            }
        }

        public async Task<IEnumerable<Cliente>> ListarClientesAsync(string? buscar)
        {
            var query = _clienteRepo.Query().Include(c => c.Ventas).AsQueryable();
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(c =>
                    c.nombres.Contains(filtro) ||
                    c.razonSocial.Contains(filtro) ||
                    c.numeroDocumento.Contains(filtro) ||
                    c.email.Contains(filtro));
            }
            return await query.OrderByDescending(c => c.idCliente).ToListAsync();
        }

        public async Task<Cliente?> ObtenerClientePorIdAsync(int id) => await _clienteRepo.GetByIdAsync(id);

        public async Task<bool> ExisteDocumentoClienteAsync(string numeroDocumento, int? excluirId = null)
        {
            string doc = numeroDocumento.Trim();
            return await _clienteRepo.Query()
                .AnyAsync(c => c.numeroDocumento == doc && (!excluirId.HasValue || c.idCliente != excluirId.Value));
        }

        public async Task CrearClienteAsync(Cliente cliente)
        {
            cliente.tipoDocumento = cliente.tipoDocumento.Trim();
            cliente.numeroDocumento = cliente.numeroDocumento.Trim();
            cliente.nombres = cliente.nombres.Trim();
            cliente.razonSocial = cliente.razonSocial.Trim();
            cliente.telefono = cliente.telefono.Trim();
            cliente.direccion = cliente.direccion.Trim();
            cliente.email = cliente.email.Trim();
            cliente.estado = true;
            await _clienteRepo.AddAsync(cliente);
            await _clienteRepo.SaveChangesAsync();
        }

        public async Task<bool> ActualizarClienteAsync(int id, Cliente cliente)
        {
            var clienteDb = await _clienteRepo.GetByIdAsync(id);
            if (clienteDb == null) return false;

            clienteDb.tipoDocumento = cliente.tipoDocumento.Trim();
            clienteDb.numeroDocumento = cliente.numeroDocumento.Trim();
            clienteDb.nombres = cliente.nombres.Trim();
            clienteDb.razonSocial = cliente.razonSocial.Trim();
            clienteDb.telefono = cliente.telefono.Trim();
            clienteDb.direccion = cliente.direccion.Trim();
            clienteDb.email = cliente.email.Trim();
            _clienteRepo.Update(clienteDb);
            await _clienteRepo.SaveChangesAsync();
            return true;
        }

        public async Task<Cliente?> CambiarEstadoClienteAsync(int id)
        {
            var cliente = await _clienteRepo.GetByIdAsync(id);
            if (cliente == null) return null;

            cliente.estado = !cliente.estado;
            _clienteRepo.Update(cliente);
            await _clienteRepo.SaveChangesAsync();
            return cliente;
        }

        public async Task<IEnumerable<Cotizacion>> ListarCotizacionesAsync(string? buscar, string? estado)
        {
            var query = _cotizacionRepo.Query()
                .Include(c => c.cliente)
                .Include(c => c.usuario)
                .Include(c => c.DetalleCotizacions)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(c => c.estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(c =>
                    c.cliente.nombres.Contains(filtro) ||
                    c.cliente.razonSocial.Contains(filtro) ||
                    c.cliente.numeroDocumento.Contains(filtro) ||
                    c.usuario.nombreUsuario.Contains(filtro) ||
                    c.idCotizacion.ToString() == filtro);
            }

            return await query.OrderByDescending(c => c.idCotizacion).ToListAsync();
        }

        public async Task<Cotizacion?> ObtenerCotizacionDetalleAsync(int id)
        {
            return await _cotizacionRepo.Query()
                .Include(c => c.cliente)
                .Include(c => c.usuario)
                .Include(c => c.DetalleCotizacions)
                    .ThenInclude(d => d.producto)
                .FirstOrDefaultAsync(c => c.idCotizacion == id);
        }

        public async Task CrearCotizacionAsync(Cotizacion cotizacion)
        {
            await _cotizacionRepo.AddAsync(cotizacion);
            await _cotizacionRepo.SaveChangesAsync();
        }

        public async Task<VentaOperacionResultado> CambiarEstadoCotizacionAsync(int id, string nuevoEstado)
        {
            var cotizacion = await _cotizacionRepo.GetByIdAsync(id);
            if (cotizacion == null)
                return new VentaOperacionResultado(false, "Cotización no encontrada", null);

            if (cotizacion.estado == "Convertida" || cotizacion.estado == "Anulada")
                return new VentaOperacionResultado(false, $"La cotización #{cotizacion.idCotizacion} ya está en estado '{cotizacion.estado}' y no puede modificarse", id);

            if (nuevoEstado != "Aprobada" && nuevoEstado != "Anulada")
                return new VentaOperacionResultado(false, "Estado no válido", id);

            cotizacion.estado = nuevoEstado;
            _cotizacionRepo.Update(cotizacion);
            await _cotizacionRepo.SaveChangesAsync();
            return new VentaOperacionResultado(true, null, cotizacion.idCotizacion);
        }
    }
}
