using AppWeb.Models;
using AppWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWeb.Controllers
{
    [Authorize(Roles = "Administrador,Vendedor")]
    public class ClienteController : Controller
    {
        private readonly IVentaService _ventaService;

        public ClienteController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            ViewBag.Buscar = buscar;
            var clientes = await _ventaService.ListarClientesAsync(buscar);
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

            if (await _ventaService.ExisteDocumentoClienteAsync(cliente.numeroDocumento))
            {
                ModelState.AddModelError(nameof(cliente.numeroDocumento), "Ya existe un cliente registrado con ese número de documento.");
                return View(cliente);
            }

            await _ventaService.CrearClienteAsync(cliente);

            TempData["Mensaje"] = $"Cliente '{cliente.nombres}' registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _ventaService.ObtenerClientePorIdAsync(id);
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

            if (await _ventaService.ExisteDocumentoClienteAsync(cliente.numeroDocumento, id))
            {
                ModelState.AddModelError(nameof(cliente.numeroDocumento), "Otro cliente ya tiene registrado ese número de documento.");
                return View(cliente);
            }

            bool actualizado = await _ventaService.ActualizarClienteAsync(id, cliente);
            if (!actualizado)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = $"Cliente '{cliente.nombres.Trim()}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var cliente = await _ventaService.CambiarEstadoClienteAsync(id);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = cliente.estado
                ? $"Cliente '{cliente.nombres}' activado correctamente."
                : $"Cliente '{cliente.nombres}' desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
