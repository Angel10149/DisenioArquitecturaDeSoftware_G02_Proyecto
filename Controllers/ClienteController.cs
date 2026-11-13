using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Controllers
{
    [Authorize]
    public class ClienteController : Controller
    {
        private readonly AppDbContext _context;

        public ClienteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            var query = _context.Clientes
                .Include(c => c.Ventas)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(c =>
                    c.nombres.Contains(filtro) ||
                    c.razonSocial.Contains(filtro) ||
                    c.numeroDocumento.Contains(filtro) ||
                    c.email.Contains(filtro));
            }

            ViewBag.Buscar = buscar;
            var clientes = await query.OrderByDescending(c => c.idCliente).ToListAsync();
            return View(clientes);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Cliente { estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            string doc = cliente.numeroDocumento.Trim();
            if (await _context.Clientes.AnyAsync(c => c.numeroDocumento == doc))
            {
                ModelState.AddModelError(nameof(cliente.numeroDocumento), "Ya existe un cliente registrado con ese número de documento.");
                return View(cliente);
            }

            cliente.tipoDocumento = cliente.tipoDocumento.Trim();
            cliente.numeroDocumento = doc;
            cliente.nombres = cliente.nombres.Trim();
            cliente.razonSocial = cliente.razonSocial.Trim();
            cliente.telefono = cliente.telefono.Trim();
            cliente.direccion = cliente.direccion.Trim();
            cliente.email = cliente.email.Trim();
            cliente.estado = true;

            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Cliente '{cliente.nombres}' registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Cliente cliente)
        {
            if (id != cliente.idCliente)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            string doc = cliente.numeroDocumento.Trim();
            if (await _context.Clientes.AnyAsync(c => c.numeroDocumento == doc && c.idCliente != id))
            {
                ModelState.AddModelError(nameof(cliente.numeroDocumento), "Otro cliente ya tiene registrado ese número de documento.");
                return View(cliente);
            }

            var clienteDb = await _context.Clientes.FindAsync(id);
            if (clienteDb == null)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            clienteDb.tipoDocumento = cliente.tipoDocumento.Trim();
            clienteDb.numeroDocumento = doc;
            clienteDb.nombres = cliente.nombres.Trim();
            clienteDb.razonSocial = cliente.razonSocial.Trim();
            clienteDb.telefono = cliente.telefono.Trim();
            clienteDb.direccion = cliente.direccion.Trim();
            clienteDb.email = cliente.email.Trim();

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Cliente '{clienteDb.nombres}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            cliente.estado = !cliente.estado;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = cliente.estado
                ? $"Cliente '{cliente.nombres}' activado correctamente."
                : $"Cliente '{cliente.nombres}' desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
