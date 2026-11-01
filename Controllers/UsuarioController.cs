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
        private const string RolAdministrador = "Administrador";

        private readonly AppDbContext _context;
        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // Roles disponibles en el registro: el rol Administrador solo se ofrece
        // mientras no exista ningun administrador (primer usuario del sistema).
        private async Task CargarRolesAsync()
        {
            bool existeAdmin = await _context.Usuarios.AnyAsync(u => u.Rol!.nameRol == RolAdministrador);
            var roles = _context.Roles.AsQueryable();
            if (existeAdmin)
            {
                roles = roles.Where(r => r.nameRol != RolAdministrador);
            }
            ViewBag.Roles = await roles.ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            await CargarRolesAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(UsuarioVM usuarioVM)
        {
            await CargarRolesAsync();
            if (!ModelState.IsValid)
            {
                return View(usuarioVM);
            }

            //El rol enviado debe estar entre los roles permitidos en el registro
            var rolesPermitidos = (IEnumerable<Rol>)ViewBag.Roles;
            if (!rolesPermitidos.Any(r => r.idRol == usuarioVM.IDROL))
            {
                ModelState.AddModelError(nameof(usuarioVM.IDROL), "El rol seleccionado no es válido");
                return View(usuarioVM);
            }

            string correo = usuarioVM.correo.Trim();
            if (await _context.Usuarios.AnyAsync(u => u.correo == correo))
            {
                ModelState.AddModelError(nameof(usuarioVM.correo), "Ya existe un usuario registrado con ese correo");
                return View(usuarioVM);
            }

            Usuario usuario = new Usuario()
            {
                nombreUsuario = usuarioVM.nombreUsuario.Trim(),
                telefono = usuarioVM.telefono.Trim(),
                correo = correo,
                password = usuarioVM.password,
                IDROL = usuarioVM.IDROL
            };
            try
            {
                await _context.Usuarios.AddAsync(usuario);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ViewData["Mensaje"] = "El usuario no pudo ser registrado";
                return View(usuarioVM);
            }
            return RedirectToAction("Login", "Usuario");
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }
            Usuario? usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.correo == loginVM.correo && u.password == loginVM.password);
            if (usuario == null || usuario.Rol == null)
            {
                ViewData["Mensaje"] = "No se encontraron usuarios";
                return View(loginVM);
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
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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
