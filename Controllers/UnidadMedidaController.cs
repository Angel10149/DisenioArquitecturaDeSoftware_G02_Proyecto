using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Controllers
{
    [Authorize]
    public class UnidadMedidaController : Controller
    {
        private readonly AppDbContext _context;

        public UnidadMedidaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            var query = _context.UnidadesMedida
                .Include(u => u.Productos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(u =>
                    u.nombre.Contains(filtro) ||
                    u.abreviatura.Contains(filtro));
            }

            ViewBag.Buscar = buscar;
            var unidades = await query.OrderByDescending(u => u.idUnidadMedida).ToListAsync();
            return View(unidades);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new UnidadMedida { estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UnidadMedida unidad)
        {
            if (!ModelState.IsValid)
            {
                return View(unidad);
            }

            string nombre = unidad.nombre.Trim();
            string abreviatura = unidad.abreviatura.Trim().ToUpper();

            if (await _context.UnidadesMedida.AnyAsync(u => u.nombre == nombre))
            {
                ModelState.AddModelError(nameof(unidad.nombre), "Ya existe una unidad de medida con ese nombre.");
                return View(unidad);
            }
            if (await _context.UnidadesMedida.AnyAsync(u => u.abreviatura == abreviatura))
            {
                ModelState.AddModelError(nameof(unidad.abreviatura), "Ya existe una unidad de medida con esa abreviatura.");
                return View(unidad);
            }

            unidad.nombre = nombre;
            unidad.abreviatura = abreviatura;
            unidad.estado = true;

            await _context.UnidadesMedida.AddAsync(unidad);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Unidad de medida '{unidad.nombre}' ({unidad.abreviatura}) registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var unidad = await _context.UnidadesMedida.FindAsync(id);
            if (unidad == null)
            {
                TempData["Error"] = "Unidad de medida no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            return View(unidad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, UnidadMedida unidad)
        {
            if (id != unidad.idUnidadMedida)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(unidad);
            }

            string nombre = unidad.nombre.Trim();
            string abreviatura = unidad.abreviatura.Trim().ToUpper();

            if (await _context.UnidadesMedida.AnyAsync(u => u.nombre == nombre && u.idUnidadMedida != id))
            {
                ModelState.AddModelError(nameof(unidad.nombre), "Otra unidad de medida ya tiene ese nombre.");
                return View(unidad);
            }
            if (await _context.UnidadesMedida.AnyAsync(u => u.abreviatura == abreviatura && u.idUnidadMedida != id))
            {
                ModelState.AddModelError(nameof(unidad.abreviatura), "Otra unidad de medida ya tiene esa abreviatura.");
                return View(unidad);
            }

            var unidadDb = await _context.UnidadesMedida.FindAsync(id);
            if (unidadDb == null)
            {
                TempData["Error"] = "Unidad de medida no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            unidadDb.nombre = nombre;
            unidadDb.abreviatura = abreviatura;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Unidad de medida '{unidadDb.nombre}' actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var unidad = await _context.UnidadesMedida.FindAsync(id);
            if (unidad == null)
            {
                TempData["Error"] = "Unidad de medida no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            unidad.estado = !unidad.estado;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = unidad.estado
                ? $"Unidad de medida '{unidad.nombre}' activada correctamente."
                : $"Unidad de medida '{unidad.nombre}' desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
