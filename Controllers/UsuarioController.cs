using AppWeb.Models;
using AppWeb.Services;
using AppWeb.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            try
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
                await _usuarioService.RegistrarAsync(usuario, usuarioVM.password);

                if (User.Identity?.IsAuthenticated == true && User.IsInRole(RolAdministrador))
                {
                    TempData["Mensaje"] = $"Usuario '{usuario.nombreUsuario}' registrado correctamente";
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction("Login", "Usuario");

            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = DbExceptionHelper.Traducir(ex);
                return View(usuarioVM);
            }
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
            try
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
                    ViewData["Mensaje"] = "El usuario se encuentra inactivo. Contacte al administrador";
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
            catch (Exception ex)
            {
                ViewData["Mensaje"] = DbExceptionHelper.Traducir(ex);
                return View(loginVM);
            }
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
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (id.ToString() == currentUserId)
                {
                    TempData["Error"] = "No puede desactivar su propia cuenta de administrador";
                    return RedirectToAction(nameof(Index));
                }

                var usuario = await _usuarioService.CambiarEstadoAsync(id);
                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Mensaje"] = usuario.estado
                    ? $"Usuario '{usuario.nombreUsuario}' activado correctamente"
                    : $"Usuario '{usuario.nombreUsuario}' desactivado correctamente";

                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                TempData["Error"] = DbExceptionHelper.Traducir(ex);
                return RedirectToAction(nameof(Index));
            }

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
                TempData["Error"] = "Usuario no encontrado";
                return RedirectToAction(nameof(Index));
            }

            await CargarRolesEdicionAsync(usuario.IDROL);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(EditarUsuarioVM model)
        {
            Usuario? usuario = null;
            try
            {
                usuario = await _usuarioService.ObtenerPorIdConRolAsync(model.id);
                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                await CargarRolesEdicionAsync(usuario.IDROL);

                if (!ModelState.IsValid)
                {
                    ViewData["Error"] = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "Verifique los datos ingresados";
                    return View(usuario);
                }

                string nombreLimpio = model.nombreUsuario.Trim();
                string correoLimpio = model.correo.Trim();
                string telefonoLimpio = model.telefono.Trim();

                if (await _usuarioService.ExisteCorreoAsync(correoLimpio, model.id))
                {
                    ViewData["Error"] = "Ya existe otro usuario registrado con ese correo";
                    return View(usuario);
                }

                var rolDestino = await _usuarioService.ObtenerRolPorIdAsync(model.IDROL);
                if (rolDestino == null)
                {
                    ViewData["Error"] = "El rol seleccionado no es válido";
                    return View(usuario);
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (usuario.id.ToString() == currentUserId && rolDestino.nameRol != RolAdministrador)
                {
                    ViewData["Error"] = "No puede cambiar su propio rol de Administrador";
                    return View(usuario);
                }

                usuario.nombreUsuario = nombreLimpio;
                usuario.correo = correoLimpio;
                usuario.telefono = telefonoLimpio;
                usuario.IDROL = model.IDROL;
                usuario.Rol = rolDestino;

                if (!string.IsNullOrWhiteSpace(model.nuevaPassword))
                {
                    usuario.password = _usuarioService.HashearPassword(usuario, model.nuevaPassword);
                }

                await _usuarioService.ActualizarAsync(usuario);

                if (usuario.id.ToString() == currentUserId)
                {
                    await RefrescarSesionAsync(usuario);
                }

                TempData["Mensaje"] = $"Usuario '{usuario.nombreUsuario}' actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (usuario != null)
                {
                    ViewData["Error"] = DbExceptionHelper.Traducir(ex);
                    return View(usuario);
                }
                TempData["Error"] = DbExceptionHelper.Traducir(ex);
                return RedirectToAction(nameof(Index));
            }
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
        public async Task<IActionResult> Perfil(PerfilUsuarioVM model)
        {
            Usuario? usuario = null;
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(currentUserId, out int idUsuario))
                {
                    return RedirectToAction(nameof(Login));
                }

                usuario = await _usuarioService.ObtenerPorIdConRolAsync(idUsuario);
                if (usuario == null)
                {
                    return RedirectToAction(nameof(Login));
                }

                if (!ModelState.IsValid)
                {
                    ViewData["Error"] = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "Verifique los datos ingresados";
                    return View(usuario);
                }

                bool quiereCambiarPassword =
                    !string.IsNullOrWhiteSpace(model.passwordActual) ||
                    !string.IsNullOrWhiteSpace(model.nuevaPassword) ||
                    !string.IsNullOrWhiteSpace(model.confirmarPassword);

                if (quiereCambiarPassword)
                {
                    if (string.IsNullOrWhiteSpace(model.passwordActual) || string.IsNullOrWhiteSpace(model.nuevaPassword))
                    {
                        ViewData["Error"] = "Debe ingresar su contraseña actual y la nueva contraseña";
                        return View(usuario);
                    }

                    if (!_usuarioService.VerificarPassword(usuario, model.passwordActual, out _))
                    {
                        ViewData["Error"] = "La contraseña actual es incorrecta";
                        return View(usuario);
                    }

                    usuario.password = _usuarioService.HashearPassword(usuario, model.nuevaPassword);
                }

                usuario.nombreUsuario = model.nombreUsuario.Trim();
                await _usuarioService.ActualizarAsync(usuario);
                await RefrescarSesionAsync(usuario);

                TempData["Mensaje"] = "Perfil actualizado correctamente";
                return RedirectToAction(nameof(Perfil));
            }
            catch (Exception ex)
            {
                if (usuario != null)
                {
                    ViewData["Error"] = DbExceptionHelper.Traducir(ex);
                    return View(usuario);
                }
                return RedirectToAction(nameof(Perfil));
            }
        }
    }
}