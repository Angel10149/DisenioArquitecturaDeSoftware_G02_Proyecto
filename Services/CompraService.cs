using AppWeb.Data;
using AppWeb.Models;
using AppWeb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public class CompraService : ICompraService
    {
        private readonly IRepository<Proveedor> _proveedorRepo;
        private readonly IRepository<RequerimientoCompra> _requerimientoRepo;
        private readonly IRepository<OrdenCompra> _ordenRepo;
        private readonly IRepository<IngresoMercaderia> _ingresoRepo;
        private readonly IRepository<Producto> _productoRepo;
        private readonly IRepository<Stock> _stockRepo;
        private readonly AppDbContext _context;

        public CompraService(
            IRepository<Proveedor> proveedorRepo,
            IRepository<RequerimientoCompra> requerimientoRepo,
            IRepository<OrdenCompra> ordenRepo,
            IRepository<IngresoMercaderia> ingresoRepo,
            IRepository<Producto> productoRepo,
            IRepository<Stock> stockRepo,
            AppDbContext context)
        {
            _proveedorRepo = proveedorRepo;
            _requerimientoRepo = requerimientoRepo;
            _ordenRepo = ordenRepo;
            _ingresoRepo = ingresoRepo;
            _productoRepo = productoRepo;
            _stockRepo = stockRepo;
            _context = context;
        }

        public async Task<IEnumerable<Proveedor>> ListarProveedoresActivosAsync()
        {
            return await _proveedorRepo.Query()
                .Where(p => p.estado)
                .OrderBy(p => p.razonSocial)
                .ToListAsync();
        }

        public async Task<IEnumerable<RequerimientoCompra>> ListarRequerimientosAsync()
        {
            return await _requerimientoRepo.Query()
                .Include(r => r.producto)
                .Include(r => r.usuario)
                .OrderByDescending(r => r.idRequerimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrdenCompra>> ListarOrdenesCompraAsync()
        {
            return await _ordenRepo.Query()
                .Include(o => o.proveedor)
                .Include(o => o.usuario)
                .OrderByDescending(o => o.idOrdenCompra)
                .ToListAsync();
        }

        public async Task<IEnumerable<IngresoMercaderia>> ListarIngresosAsync()
        {
            return await _ingresoRepo.Query()
                .Include(i => i.ordenCompra)
                    .ThenInclude(o => o!.proveedor)
                .OrderByDescending(i => i.idIngresoMercaderia)
                .ToListAsync();
        }

        public async Task<IEnumerable<Proveedor>> ListarProveedoresAsync(string? buscar)
        {
            var query = _proveedorRepo.Query().Include(p => p.OrdenesCompra).AsQueryable();
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(p =>
                    p.ruc.Contains(filtro) ||
                    p.razonSocial.Contains(filtro) ||
                    p.contacto.Contains(filtro) ||
                    p.email.Contains(filtro));
            }
            return await query.OrderByDescending(p => p.idProveedor).ToListAsync();
        }

        public async Task<Proveedor?> ObtenerProveedorPorIdAsync(int id) => await _proveedorRepo.GetByIdAsync(id);

        public async Task<bool> ExisteRucProveedorAsync(string ruc, int? excluirId = null)
        {
            string norm = ruc.Trim();
            return await _proveedorRepo.Query()
                .AnyAsync(p => p.ruc == norm && (!excluirId.HasValue || p.idProveedor != excluirId.Value));
        }

        public async Task CrearProveedorAsync(Proveedor proveedor)
        {
            proveedor.ruc = proveedor.ruc.Trim();
            proveedor.razonSocial = proveedor.razonSocial.Trim();
            proveedor.contacto = proveedor.contacto.Trim();
            proveedor.telefono = proveedor.telefono.Trim();
            proveedor.direccion = proveedor.direccion.Trim();
            proveedor.email = proveedor.email.Trim();
            proveedor.estado = true;
            await _proveedorRepo.AddAsync(proveedor);
            await _proveedorRepo.SaveChangesAsync();
        }

        public async Task<bool> ActualizarProveedorAsync(int id, Proveedor proveedor)
        {
            var proveedorDb = await _proveedorRepo.GetByIdAsync(id);
            if (proveedorDb == null) return false;

            proveedorDb.ruc = proveedor.ruc.Trim();
            proveedorDb.razonSocial = proveedor.razonSocial.Trim();
            proveedorDb.contacto = proveedor.contacto.Trim();
            proveedorDb.telefono = proveedor.telefono.Trim();
            proveedorDb.direccion = proveedor.direccion.Trim();
            proveedorDb.email = proveedor.email.Trim();
            _proveedorRepo.Update(proveedorDb);
            await _proveedorRepo.SaveChangesAsync();
            return true;
        }

        public async Task<Proveedor?> CambiarEstadoProveedorAsync(int id)
        {
            var proveedor = await _proveedorRepo.GetByIdAsync(id);
            if (proveedor == null) return null;

            proveedor.estado = !proveedor.estado;
            _proveedorRepo.Update(proveedor);
            await _proveedorRepo.SaveChangesAsync();
            return proveedor;
        }

        public async Task<IEnumerable<Producto>> ListarProductosActivosAsync()
        {
            return await _productoRepo.Query()
                .Where(p => p.estado)
                .OrderBy(p => p.nombre)
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerProductoPorIdAsync(int id) => await _productoRepo.GetByIdAsync(id);

        public async Task<IEnumerable<RequerimientoCompra>> ListarRequerimientosFiltradosAsync(string? buscar, string? estado)
        {
            var query = _requerimientoRepo.Query()
                .Include(r => r.producto)
                .Include(r => r.usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(r => r.estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(r =>
                    r.producto!.nombre.Contains(filtro) ||
                    (r.motivo != null && r.motivo.Contains(filtro)) ||
                    r.usuario!.nombreUsuario.Contains(filtro));
            }

            return await query.OrderByDescending(r => r.idRequerimiento).ToListAsync();
        }

        public async Task<RequerimientoCompra?> ObtenerRequerimientoPorIdAsync(int id) =>
            await _requerimientoRepo.GetByIdAsync(id);

        public async Task CrearRequerimientoAsync(RequerimientoCompra requerimiento)
        {
            await _requerimientoRepo.AddAsync(requerimiento);
            await _requerimientoRepo.SaveChangesAsync();
        }

        public async Task<(bool Ok, string? Error)> ActualizarRequerimientoAsync(int id, RequerimientoCompra requerimiento)
        {
            var reqDb = await _requerimientoRepo.GetByIdAsync(id);
            if (reqDb == null) return (false, "Requerimiento no encontrado.");
            if (reqDb.estado != "Pendiente") return (false, "Solo se pueden editar requerimientos en estado Pendiente.");

            reqDb.idProducto = requerimiento.idProducto;
            reqDb.cantidadSugerida = requerimiento.cantidadSugerida;
            reqDb.cantidadRequerida = requerimiento.cantidadRequerida;
            reqDb.fechaRequerida = requerimiento.fechaRequerida;
            reqDb.prioridad = requerimiento.prioridad;
            reqDb.motivo = requerimiento.motivo?.Trim() ?? string.Empty;

            _requerimientoRepo.Update(reqDb);
            await _requerimientoRepo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Ok, string? Error)> CambiarEstadoRequerimientoAsync(int id, string nuevoEstado)
        {
            var estadosPermitidos = new[] { "Pendiente", "Aprobado", "Rechazado", "Atendido" };
            if (!estadosPermitidos.Contains(nuevoEstado))
                return (false, "Estado no válido.");

            var reqDb = await _requerimientoRepo.GetByIdAsync(id);
            if (reqDb == null) return (false, "Requerimiento no encontrado.");

            reqDb.estado = nuevoEstado;
            _requerimientoRepo.Update(reqDb);
            await _requerimientoRepo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<IEnumerable<RequerimientoCompra>> ListarRequerimientosAprobadosAsync()
        {
            return await _requerimientoRepo.Query()
                .Include(r => r.producto)
                .Where(r => r.estado == "Aprobado")
                .OrderByDescending(r => r.idRequerimiento)
                .ToListAsync();
        }

        public async Task<RequerimientoCompra?> ObtenerRequerimientoConProductoAsync(int idRequerimiento)
        {
            return await _requerimientoRepo.Query()
                .Include(r => r.producto)
                .FirstOrDefaultAsync(r => r.idRequerimiento == idRequerimiento);
        }

        public async Task<IEnumerable<OrdenCompra>> ListarOrdenesCompraFiltradasAsync(string? buscar, string? estado)
        {
            var query = _ordenRepo.Query()
                .Include(o => o.proveedor)
                .Include(o => o.usuario)
                .Include(o => o.requerimiento)
                    .ThenInclude(r => r!.producto)
                .Include(o => o.DetalleOrdenCompras)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(o => o.estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(o =>
                    o.proveedor!.razonSocial.Contains(filtro) ||
                    o.usuario!.nombreUsuario.Contains(filtro) ||
                    o.idOrdenCompra.ToString() == filtro);
            }

            return await query.OrderByDescending(o => o.idOrdenCompra).ToListAsync();
        }

        public async Task<OrdenCompra?> ObtenerOrdenCompraDetalleAsync(int id)
        {
            return await _ordenRepo.Query()
                .Include(o => o.proveedor)
                .Include(o => o.usuario)
                .Include(o => o.requerimiento)
                    .ThenInclude(r => r!.producto)
                .Include(o => o.DetalleOrdenCompras)
                    .ThenInclude(d => d.producto)
                .Include(o => o.IngresosMercaderia)
                .FirstOrDefaultAsync(o => o.idOrdenCompra == id);
        }

        public async Task<(bool Ok, string? Error, OrdenCompra? Orden)> CrearOrdenCompraAsync(int idProveedor, int idRequerimiento, int idUsuario, List<DetalleOrdenCompra> detalles)
        {
            var orden = new OrdenCompra
            {
                fechaCompra = DateTime.Now,
                estado = "Pendiente",
                idProveedor = idProveedor,
                idRequerimiento = idRequerimiento,
                idUsuario = idUsuario,
                total = detalles.Sum(d => d.subtotal),
                DetalleOrdenCompras = detalles
            };

            await _ordenRepo.AddAsync(orden);

            var req = await _requerimientoRepo.GetByIdAsync(idRequerimiento);
            if (req != null && req.estado == "Aprobado")
            {
                req.estado = "Atendido";
                _requerimientoRepo.Update(req);
            }

            await _ordenRepo.SaveChangesAsync();
            return (true, null, orden);
        }

        public async Task<(bool Ok, string? Error)> CambiarEstadoOrdenCompraAsync(int id, string nuevoEstado)
        {
            var estadosPermitidos = new[] { "Pendiente", "Emitida", "Recibida", "Anulada" };
            if (!estadosPermitidos.Contains(nuevoEstado))
                return (false, "Estado no válido.");

            var orden = await _ordenRepo.GetByIdAsync(id);
            if (orden == null) return (false, "Orden de compra no encontrada.");

            orden.estado = nuevoEstado;
            _ordenRepo.Update(orden);
            await _ordenRepo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<IEnumerable<OrdenCompra>> ListarOrdenesPendientesOEmailAsync()
        {
            return await _ordenRepo.Query()
                .Include(o => o.proveedor)
                .Where(o => o.estado == "Pendiente" || o.estado == "Emitida")
                .OrderByDescending(o => o.idOrdenCompra)
                .ToListAsync();
        }

        public async Task<OrdenCompra?> ObtenerOrdenParaIngresoAsync(int idOrdenCompra)
        {
            return await _ordenRepo.Query()
                .Include(o => o.proveedor)
                .Include(o => o.DetalleOrdenCompras)
                    .ThenInclude(d => d.producto)
                .FirstOrDefaultAsync(o => o.idOrdenCompra == idOrdenCompra);
        }

        public async Task<IEnumerable<IngresoMercaderia>> ListarIngresosFiltradosAsync(string? buscar, string? estado)
        {
            var query = _ingresoRepo.Query()
                .Include(i => i.ordenCompra)
                .Include(i => i.proveedor)
                .Include(i => i.DetalleIngresos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(i => i.estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(i =>
                    i.proveedor!.razonSocial.Contains(filtro) ||
                    i.observaciones.Contains(filtro) ||
                    i.idOrdenCompra.ToString() == filtro ||
                    i.idIngresoMercaderia.ToString() == filtro);
            }

            return await query.OrderByDescending(i => i.idIngresoMercaderia).ToListAsync();
        }

        public async Task<IngresoMercaderia?> ObtenerIngresoDetalleAsync(int id)
        {
            return await _ingresoRepo.Query()
                .Include(i => i.ordenCompra)
                .Include(i => i.proveedor)
                .Include(i => i.DetalleIngresos)
                    .ThenInclude(d => d.producto)
                .FirstOrDefaultAsync(i => i.idIngresoMercaderia == id);
        }

        public async Task<(bool Ok, string? Error, int? IdIngreso)> RegistrarIngresoAsync(OrdenCompra orden, string? observaciones, List<DetalleIngreso> detalles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ingreso = new IngresoMercaderia
                {
                    fechaIngreso = DateTime.Now,
                    estado = "Registrado",
                    observaciones = observaciones?.Trim() ?? string.Empty,
                    idOrdenCompra = orden.idOrdenCompra,
                    idProveedor = orden.idProveedor,
                    DetalleIngresos = detalles
                };

                await _ingresoRepo.AddAsync(ingreso);

                foreach (var det in detalles)
                {
                    if (det.cantidadRecibida > 0)
                    {
                        var producto = await _productoRepo.GetByIdAsync(det.idProducto);
                        if (producto != null)
                        {
                            producto.stockActual += det.cantidadRecibida;
                            if (det.costoUnitario > 0)
                            {
                                producto.precioCosto = det.costoUnitario;
                            }
                            _productoRepo.Update(producto);

                            await _stockRepo.AddAsync(new Stock
                            {
                                idProducto = producto.idProducto,
                                cantidad = producto.stockActual,
                                fecha_actualizacion = DateTime.Now
                            });
                        }
                    }
                }

                orden.estado = "Recibida";
                _ordenRepo.Update(orden);

                await _ingresoRepo.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, null, ingreso.idIngresoMercaderia);
            }
            catch
            {
                await transaction.RollbackAsync();
                return (false, "Ocurrió un error al registrar el ingreso de mercadería.", null);
            }
        }
    }
}
