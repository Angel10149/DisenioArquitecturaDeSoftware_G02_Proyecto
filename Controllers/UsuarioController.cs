using AppWeb.Data;
using AppWeb.Models;
using AppWeb.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registro(UsuarioVM usuarioVM)
        {
            ViewBag.Roles = _context.Roles.ToList();
            if (usuarioVM.password != usuarioVM.reppassword)
            {
                ViewData["Mensaje"] = "Las constraseñas no coinciden";
                return View();
            }
            Usuario usuario = new Usuario()
            {
                nombreUsuario = usuarioVM.nombreUsuario,
                telefono = usuarioVM.telefono,
                correo = usuarioVM.correo,
                password = usuarioVM.password,
                IDROL = usuarioVM.IDROL
            };
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
            if (usuario.id != 0)
            {
                return RedirectToAction("Login", "Usuario");
            }
            ViewData["Mensaje"] = "El usuario no pudo ser registrado";
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            Usuario? usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.correo == loginVM.correo && u.password == loginVM.password);
            if (usuario == null)
            {
                ViewData["Mensaje"] = "No se encontraron usuarios";
                return View();
            }
            //Claims
            var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier, usuario.id.ToString()),
                new Claim(ClaimTypes.Name, usuario.nombreUsuario),
                new Claim(ClaimTypes.Email, usuario.correo),
                new Claim(ClaimTypes.Role, usuario.Rol.nameRol)
            };
            //Identidad
            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //Principal
            var principal = new ClaimsPrincipal(identidad);
            //Cookies con autenticacion
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            //SESIONES
            HttpContext.Session.SetString("IdUsuario", usuario.id.ToString());
            HttpContext.Session.SetString("NombreUsuario", usuario.nombreUsuario);
            HttpContext.Session.SetString("CorreoUsuario", usuario.correo);
            HttpContext.Session.SetString("RolUsuario", usuario.Rol.nameRol);
            //Redireccion
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Usuario");
        }
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
