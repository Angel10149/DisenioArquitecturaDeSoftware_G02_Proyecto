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

        public CompraService(
            IRepository<Proveedor> proveedorRepo,
            IRepository<RequerimientoCompra> requerimientoRepo,
            IRepository<OrdenCompra> ordenRepo,
            IRepository<IngresoMercaderia> ingresoRepo)
        {
            _proveedorRepo = proveedorRepo;
            _requerimientoRepo = requerimientoRepo;
            _ordenRepo = ordenRepo;
            _ingresoRepo = ingresoRepo;
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
    }
}
