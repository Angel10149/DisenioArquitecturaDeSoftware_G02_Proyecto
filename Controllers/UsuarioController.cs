using AppWeb.Models;
using AppWeb.Services;
using AppWeb.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    public class UsuarioController : Controller
    {
        private const string RolAdministrador = "Administrador";
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        private async Task CargarRolesAsync()
        {
            ViewBag.Roles = await _usuarioService.ObtenerRolesDisponiblesRegistroAsync();
        }

        private async Task<IActionResult?> ValidarAccesoRegistroAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return User.IsInRole(RolAdministrador) ? null : RedirectToAction(nameof(AccesoDenegado));
            }

            bool existeAdmin = await _usuarioService.ExisteAdministradorActivoAsync();
            return existeAdmin ? RedirectToAction(nameof(Login)) : null;
        }

        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            var accesoDenegado = await ValidarAccesoRegistroAsync();
            if (accesoDenegado != null) return accesoDenegado;

            await CargarRolesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(UsuarioVM usuarioVM)
        {
            var accesoDenegado = await ValidarAccesoRegistroAsync();
            if (accesoDenegado != null) return accesoDenegado;

            await CargarRolesAsync();
            if (!ModelState.IsValid)
            {
                return View(usuarioVM);
            }

            var rolesPermitidos = (IEnumerable<Rol>)ViewBag.Roles;
            if (!rolesPermitidos.Any(r => r.idRol == usuarioVM.IDROL))
            {
                ModelState.AddModelError(nameof(usuarioVM.IDROL), "El rol seleccionado no es válido");
                return View(usuarioVM);
            }

            string correo = usuarioVM.correo.Trim();
            if (await _usuarioService.ExisteCorreoAsync(correo))
            {
                ModelState.AddModelError(nameof(usuarioVM.correo), "Ya existe un usuario registrado con ese correo");
                return View(usuarioVM);
            }

            Usuario usuario = new Usuario()
            {
                nombreUsuario = usuarioVM.nombreUsuario.Trim(),
                telefono = usuarioVM.telefono.Trim(),
                correo = correo,
                estado = true,
                IDROL = usuarioVM.IDROL
            };

            try
            {
                await _usuarioService.RegistrarAsync(usuario, usuarioVM.password);
            }
            catch (DbUpdateException)
            {
                ViewData["Mensaje"] = "El usuario no pudo ser registrado";
                return View(usuarioVM);
            }

            if (User.Identity?.IsAuthenticated == true && User.IsInRole(RolAdministrador))
            {
                TempData["Mensaje"] = $"Usuario '{usuario.nombreUsuario}' registrado correctamente.";
                return RedirectToAction(nameof(Index));
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

            Usuario? usuario = await _usuarioService.ObtenerPorCorreoAsync(loginVM.correo);
            if (usuario == null || usuario.Rol == null)
            {
                ViewData["Mensaje"] = "No se encontraron usuarios";
                return View(loginVM);
            }

            if (!_usuarioService.VerificarPassword(usuario, loginVM.password, out bool requiereRehash))
            {
                ViewData["Mensaje"] = "No se encontraron usuarios";
                return View(loginVM);
            }

            if (!usuario.estado || !usuario.Rol.estado)
            {
                ViewData["Mensaje"] = "El usuario se encuentra inactivo. Contacte al administrador.";
                return View(loginVM);
            }

            if (requiereRehash)
            {
                usuario.password = _usuarioService.HashearPassword(usuario, loginVM.password);
                await _usuarioService.ActualizarAsync(usuario);
            }

            await RefrescarSesionAsync(usuario);

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

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioService.ListarConRolAsync();
            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id.ToString() == currentUserId)
            {
                TempData["Error"] = "No puede desactivar su propia cuenta de administrador.";
                return RedirectToAction(nameof(Index));
            }

            var usuario = await _usuarioService.CambiarEstadoAsync(id);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = usuario.estado
                ? $"Usuario '{usuario.nombreUsuario}' activado correctamente."
                : $"Usuario '{usuario.nombreUsuario}' desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarRolesEdicionAsync(int idRolActual)
        {
            ViewBag.Roles = await _usuarioService.ObtenerRolesEdicionAsync(idRolActual);
        }

        private async Task RefrescarSesionAsync(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.id.ToString()),
                new Claim(ClaimTypes.Name, usuario.nombreUsuario),
                new Claim(ClaimTypes.Email, usuario.correo),
                new Claim(ClaimTypes.Role, usuario.Rol?.nameRol ?? string.Empty)
            };

            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identidad);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Session.SetString("IdUsuario", usuario.id.ToString());
            HttpContext.Session.SetString("NombreUsuario", usuario.nombreUsuario);
            HttpContext.Session.SetString("CorreoUsuario", usuario.correo);
            HttpContext.Session.SetString("RolUsuario", usuario.Rol?.nameRol ?? string.Empty);
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdConRolAsync(id);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            await CargarRolesEdicionAsync(usuario.IDROL);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(
            int id,
            string nombreUsuario,
            string correo,
            string telefono,
            int IDROL,
            string? nuevaPassword)
        {
            var usuario = await _usuarioService.ObtenerPorIdConRolAsync(id);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            await CargarRolesEdicionAsync(usuario.IDROL);

            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(telefono))
            {
                ViewData["Error"] = "Nombre, correo y teléfono son obligatorios.";
                return View(usuario);
            }

            string correoLimpio = correo.Trim();
            if (await _usuarioService.ExisteCorreoAsync(correoLimpio, id))
            {
                ViewData["Error"] = "Ya existe otro usuario registrado con ese correo.";
                return View(usuario);
            }

            var rolDestino = await _usuarioService.ObtenerRolPorIdAsync(IDROL);
            if (rolDestino == null)
            {
                ViewData["Error"] = "El rol seleccionado no es válido.";
                return View(usuario);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuario.id.ToString() == currentUserId && rolDestino.nameRol != RolAdministrador)
            {
                ViewData["Error"] = "No puede cambiar su propio rol de Administrador.";
                return View(usuario);
            }

            usuario.nombreUsuario = nombreUsuario.Trim();
            usuario.correo = correoLimpio;
            usuario.telefono = telefono.Trim();
            usuario.IDROL = IDROL;
            usuario.Rol = rolDestino;

            if (!string.IsNullOrWhiteSpace(nuevaPassword))
            {
                usuario.password = _usuarioService.HashearPassword(usuario, nuevaPassword);
            }

            await _usuarioService.ActualizarAsync(usuario);

            if (usuario.id.ToString() == currentUserId)
            {
                await RefrescarSesionAsync(usuario);
            }

            TempData["Mensaje"] = $"Usuario '{usuario.nombreUsuario}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserId, out int idUsuario))
            {
                return RedirectToAction(nameof(Login));
            }

            var usuario = await _usuarioService.ObtenerPorIdConRolAsync(idUsuario);
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Perfil(
            string nombreUsuario,
            string? passwordActual,
            string? nuevaPassword,
            string? confirmarPassword)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserId, out int idUsuario))
            {
                return RedirectToAction(nameof(Login));
            }

            var usuario = await _usuarioService.ObtenerPorIdConRolAsync(idUsuario);
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                ViewData["Error"] = "El nombre de usuario es obligatorio.";
                return View(usuario);
            }

            bool quiereCambiarPassword =
                !string.IsNullOrWhiteSpace(passwordActual) ||
                !string.IsNullOrWhiteSpace(nuevaPassword) ||
                !string.IsNullOrWhiteSpace(confirmarPassword);

            if (quiereCambiarPassword)
            {
                if (string.IsNullOrWhiteSpace(passwordActual) || string.IsNullOrWhiteSpace(nuevaPassword))
                {
                    ViewData["Error"] = "Debe ingresar su contraseña actual y la nueva contraseña.";
                    return View(usuario);
                }

                if (nuevaPassword != confirmarPassword)
                {
                    ViewData["Error"] = "La nueva contraseña y su confirmación no coinciden.";
                    return View(usuario);
                }

                if (!_usuarioService.VerificarPassword(usuario, passwordActual, out _))
                {
                    ViewData["Error"] = "La contraseña actual es incorrecta.";
                    return View(usuario);
                }

                usuario.password = _usuarioService.HashearPassword(usuario, nuevaPassword);
            }

            usuario.nombreUsuario = nombreUsuario.Trim();
            await _usuarioService.ActualizarAsync(usuario);
            await RefrescarSesionAsync(usuario);

            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Perfil));
        }
    }
}
