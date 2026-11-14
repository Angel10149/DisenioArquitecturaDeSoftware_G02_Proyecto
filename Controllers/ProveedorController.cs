using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Encargado de Compras")]
    public class ProveedorController : Controller
    {
        private readonly AppDbContext _context;

        public ProveedorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            var query = _context.Proveedores
                .Include(p => p.OrdenesCompra)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(p =>
                    p.ruc.Contains(filtro) ||
                    p.razonSocial.Contains(filtro) ||
                    p.contacto.Contains(filtro) ||
                    p.email.Contains(filtro));
            }

            ViewBag.Buscar = buscar;
            var proveedores = await query.OrderByDescending(p => p.idProveedor).ToListAsync();
            return View(proveedores);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Proveedor { estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Proveedor proveedor)
        {
            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            string ruc = proveedor.ruc.Trim();
            if (await _context.Proveedores.AnyAsync(p => p.ruc == ruc))
            {
                ModelState.AddModelError(nameof(proveedor.ruc), "Ya existe un proveedor registrado con ese RUC.");
                return View(proveedor);
            }

            proveedor.ruc = ruc;
            proveedor.razonSocial = proveedor.razonSocial.Trim();
            proveedor.contacto = proveedor.contacto.Trim();
            proveedor.telefono = proveedor.telefono.Trim();
            proveedor.direccion = proveedor.direccion.Trim();
            proveedor.email = proveedor.email.Trim();
            proveedor.estado = true;

            await _context.Proveedores.AddAsync(proveedor);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Proveedor '{proveedor.razonSocial}' registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
            {
                TempData["Error"] = "Proveedor no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Proveedor proveedor)
        {
            if (id != proveedor.idProveedor)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            string ruc = proveedor.ruc.Trim();
            if (await _context.Proveedores.AnyAsync(p => p.ruc == ruc && p.idProveedor != id))
            {
                ModelState.AddModelError(nameof(proveedor.ruc), "Otro proveedor ya tiene registrado ese RUC.");
                return View(proveedor);
            }

            var proveedorDb = await _context.Proveedores.FindAsync(id);
            if (proveedorDb == null)
            {
                TempData["Error"] = "Proveedor no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            proveedorDb.ruc = ruc;
            proveedorDb.razonSocial = proveedor.razonSocial.Trim();
            proveedorDb.contacto = proveedor.contacto.Trim();
            proveedorDb.telefono = proveedor.telefono.Trim();
            proveedorDb.direccion = proveedor.direccion.Trim();
            proveedorDb.email = proveedor.email.Trim();

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Proveedor '{proveedorDb.razonSocial}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
            {
                TempData["Error"] = "Proveedor no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            proveedor.estado = !proveedor.estado;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = proveedor.estado
                ? $"Proveedor '{proveedor.razonSocial}' activado correctamente."
                : $"Proveedor '{proveedor.razonSocial}' desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
