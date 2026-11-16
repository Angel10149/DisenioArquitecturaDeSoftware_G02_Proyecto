using AppWeb.Models;
using AppWeb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Services
{
    public class CajaService : ICajaService
    {
        private readonly IRepository<Pago> _pagoRepo;
        private readonly IRepository<Venta> _ventaRepo;

        public CajaService(IRepository<Pago> pagoRepo, IRepository<Venta> ventaRepo)
        {
            _pagoRepo = pagoRepo;
            _ventaRepo = ventaRepo;
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
                    p.venta!.cliente!.numeroDocumento.Contains(filtro));
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
    }
}
