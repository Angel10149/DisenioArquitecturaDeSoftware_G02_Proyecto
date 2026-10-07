using AppWeb.Data;
using Microsoft.AspNetCore.Mvc;

namespace AppWeb.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;
        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }
    }
}
