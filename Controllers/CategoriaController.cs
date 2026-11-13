using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Controllers
{
    [Authorize]
    public class CategoriaController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            var query = _context.Categorias
                .Include(c => c.Productos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string filtro = buscar.Trim();
                query = query.Where(c =>
                    c.nombre.Contains(filtro) ||
                    c.descripcion.Contains(filtro));
            }

            ViewBag.Buscar = buscar;
            var categorias = await query.OrderByDescending(c => c.idCategoria).ToListAsync();
            return View(categorias);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Categoria { estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            string nombre = categoria.nombre.Trim();
            if (await _context.Categorias.AnyAsync(c => c.nombre == nombre))
            {
                ModelState.AddModelError(nameof(categoria.nombre), "Ya existe una categoría registrada con ese nombre.");
                return View(categoria);
            }

            categoria.nombre = nombre;
            categoria.descripcion = categoria.descripcion.Trim();
            categoria.estado = true;

            await _context.Categorias.AddAsync(categoria);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Categoría '{categoria.nombre}' registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                TempData["Error"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Categoria categoria)
        {
            if (id != categoria.idCategoria)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            string nombre = categoria.nombre.Trim();
            if (await _context.Categorias.AnyAsync(c => c.nombre == nombre && c.idCategoria != id))
            {
                ModelState.AddModelError(nameof(categoria.nombre), "Otra categoría ya tiene registrado ese nombre.");
                return View(categoria);
            }

            var categoriaDb = await _context.Categorias.FindAsync(id);
            if (categoriaDb == null)
            {
                TempData["Error"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            categoriaDb.nombre = nombre;
            categoriaDb.descripcion = categoria.descripcion.Trim();

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Categoría '{categoriaDb.nombre}' actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                TempData["Error"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            categoria.estado = !categoria.estado;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = categoria.estado
                ? $"Categoría '{categoria.nombre}' activada correctamente."
                : $"Categoría '{categoria.nombre}' desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
