using System.Data;
using AppWeb.Data;
using AppWeb.Models;
using AppWeb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public class CajaService : ICajaService
    {
        private readonly IRepository<Pago> _pagoRepo;
        private readonly IRepository<Venta> _ventaRepo;
        private readonly IRepository<Comprobante> _comprobanteRepo;
        private readonly IRepository<ConfiguracionEmpresa> _configRepo;
        private readonly AppDbContext _context;

        public CajaService(
            IRepository<Pago> pagoRepo,
            IRepository<Venta> ventaRepo,
            IRepository<Comprobante> comprobanteRepo,
            IRepository<ConfiguracionEmpresa> configRepo,
            AppDbContext context)
        {
            _pagoRepo = pagoRepo;
            _ventaRepo = ventaRepo;
            _comprobanteRepo = comprobanteRepo;
            _configRepo = configRepo;
            _context = context;
        }

        public async Task<IEnumerable<Pago>> ListarPagosAsync(string? buscar, string? metodoPago)
        {
            var query = _pagoRepo.Query()
                .Include(p => p.venta)
                    .ThenInclude(v => v!.cliente)
                .Include(p => p.usuario)
                .Include(p => p.Comprobantes)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(metodoPago))
            {
                query = query.Where(p => p.metodoPago == metodoPago);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(p =>
                    p.venta!.cliente!.nombres.Contains(filtro) ||
                    p.venta!.cliente!.razonSocial.Contains(filtro) ||
                    p.venta!.cliente!.numeroDocumento.Contains(filtro) ||
                    p.idVenta.ToString() == filtro ||
                    p.idPago.ToString() == filtro);
            }

            return await query.OrderByDescending(p => p.idPago).ToListAsync();
        }

        public async Task<IEnumerable<Venta>> ListarVentasPendientesAsync()
        {
            return await _ventaRepo.Query()
                .Include(v => v.cliente)
                .Where(v => v.estado == "Pendiente")
                .OrderByDescending(v => v.idVenta)
                .ToListAsync();
        }

        public async Task<Venta?> ObtenerVentaPendientePorIdAsync(int idVenta)
        {
            return await _ventaRepo.Query()
                .Include(v => v.cliente)
                .Include(v => v.DetalleVentas)
                    .ThenInclude(d => d.producto)
                .FirstOrDefaultAsync(v => v.idVenta == idVenta && v.estado == "Pendiente");
        }

        public async Task<Pago?> ObtenerComprobantePorPagoAsync(int idPago)
        {
            return await _pagoRepo.Query()
                .Include(p => p.venta)
                    .ThenInclude(v => v!.cliente)
                .Include(p => p.venta)
                    .ThenInclude(v => v!.DetalleVentas)
                        .ThenInclude(d => d.producto)
                .Include(p => p.usuario)
                .Include(p => p.Comprobantes)
                .FirstOrDefaultAsync(p => p.idPago == idPago);
        }

        private static readonly string[] MetodosPagoPermitidos =
            { "Efectivo", "Tarjeta", "Transferencia", "Yape / Plin" };

        private static readonly string[] TiposComprobantePermitidos =
            { "Boleta", "Factura", "Ticket" };

        public async Task<CobroResultado> RegistrarCobroAsync(int idVenta, int idUsuario, string metodoPago, decimal montoRecibido, string tipoComprobante)
        {
            var venta = await _ventaRepo.Query()
                .Include(v => v.cliente)
                .FirstOrDefaultAsync(v => v.idVenta == idVenta);

            if (venta == null)
                return new CobroResultado(false, "La venta seleccionada no existe", null, null);

            if (venta.estado != "Pendiente")
                return new CobroResultado(false, $"La venta #{venta.idVenta} no está pendiente de pago (estado: '{venta.estado}')", null, null);

            if (string.IsNullOrWhiteSpace(metodoPago))
                return new CobroResultado(false, "Debe seleccionar un método de pago", null, null);

            var metodoNormalizado = MetodosPagoPermitidos.FirstOrDefault(
                m => string.Equals(m, metodoPago.Trim(), StringComparison.OrdinalIgnoreCase));
            if (metodoNormalizado == null)
                return new CobroResultado(false, $"El método de pago '{metodoPago.Trim()}' no es válido. Métodos permitidos: {string.Join(", ", MetodosPagoPermitidos)}", null, null);

            if (montoRecibido < venta.total)
                return new CobroResultado(false, $"El monto recibido (S/ {montoRecibido:N2}) no puede ser menor al total de la venta (S/ {venta.total:N2})", null, null);

            string tipoSolicitado = string.IsNullOrWhiteSpace(tipoComprobante) ? "Boleta" : tipoComprobante.Trim();
            var tipoComp = TiposComprobantePermitidos.FirstOrDefault(
                t => string.Equals(t, tipoSolicitado, StringComparison.OrdinalIgnoreCase));
            if (tipoComp == null)
                return new CobroResultado(false, $"El tipo de comprobante '{tipoSolicitado}' no es válido. Valores permitidos: {string.Join(", ", TiposComprobantePermitidos)}", null, null);

            var cfg = await _configRepo.Query().FirstOrDefaultAsync();
            var serieBoleta = !string.IsNullOrWhiteSpace(cfg?.serieBoleta) ? cfg.serieBoleta : "B001";
            var serieFactura = !string.IsNullOrWhiteSpace(cfg?.serieFactura) ? cfg.serieFactura : "F001";

            string serie = tipoComp switch
            {
                "Factura" => serieFactura,
                "Ticket" => "T001",
                _ => serieBoleta
            };

            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                int correlativo = await _comprobanteRepo.Query().CountAsync(c => c.serie == serie) + 1;
                string numero = correlativo.ToString("D8");

                var pago = new Pago
                {
                    fechaPago = DateTime.Now,
                    metodoPago = metodoNormalizado,
                    montoRecibido = montoRecibido,
                    estado = "Registrado",
                    idVenta = venta.idVenta,
                    idUsuario = idUsuario
                };

                await _pagoRepo.AddAsync(pago);
                await _pagoRepo.SaveChangesAsync();

                var comprobante = new Comprobante
                {
                    tipoComprobante = tipoComp,
                    serie = serie,
                    numero = numero,
                    archivoPdf = $"{serie}-{numero}.pdf",
                    estado = true,
                    idPago = pago.idPago
                };

                await _comprobanteRepo.AddAsync(comprobante);
                venta.estado = "Pagada";
                _ventaRepo.Update(venta);

                await _comprobanteRepo.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CobroResultado(true, null, pago.idPago, $"{serie}-{numero}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new CobroResultado(false, DbExceptionHelper.Traducir(ex), null, null);
            }
        }
    }
}
