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

        public VentaService(
            IRepository<Venta> ventaRepo,
            IRepository<Cliente> clienteRepo,
            IRepository<Cotizacion> cotizacionRepo)
        {
            _ventaRepo = ventaRepo;
            _clienteRepo = clienteRepo;
            _cotizacionRepo = cotizacionRepo;
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
                query = query.Where(v => v.estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(v =>
                    v.cliente!.nombres.Contains(filtro) ||
                    v.cliente!.razonSocial.Contains(filtro) ||
                    v.cliente!.numeroDocumento.Contains(filtro));
            }

            return await query.OrderByDescending(v => v.idVenta).ToListAsync();
        }

        public async Task<Venta?> ObtenerPorIdAsync(int id)
        {
            return await _ventaRepo.Query()
                .Include(v => v.cliente)
                .Include(v => v.usuario)
                .Include(v => v.DetalleVentas)
                    .ThenInclude(d => d.producto)
                .Include(v => v.Pagos)
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

        public async Task<IEnumerable<Cotizacion>> ListarCotizacionesAprobadasAsync()
        {
            return await _cotizacionRepo.Query()
                .Include(c => c.cliente)
                .Where(c => c.estado == "Aprobada")
                .OrderByDescending(c => c.idCotizacion)
                .ToListAsync();
        }
    }
}
