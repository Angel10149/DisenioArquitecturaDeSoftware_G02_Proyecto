using AppWeb.Data;
using AppWeb.Models;
using AppWeb.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    public class UsuarioController : Controller
    {
        private const string RolAdministrador = "Administrador";

        private readonly AppDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // Roles disponibles en el registro: el rol Administrador solo se ofrece
        // mientras no exista ningun administrador (primer usuario del sistema).
        private async Task CargarRolesAsync()
        {
            bool existeAdmin = await _context.Usuarios.AnyAsync(u => u.estado && u.Rol!.nameRol == RolAdministrador);
            var roles = _context.Roles.Where(r => r.estado).AsQueryable();
            if (existeAdmin)
            {
                roles = roles.Where(r => r.nameRol != RolAdministrador);
            }
            ViewBag.Roles = await roles.ToListAsync();
        }

        // Solo el Administrador puede registrar nuevos usuarios una vez que ya existe uno.
        // Si no existe ningún administrador activo, se permite el registro inicial sin sesión.
        private async Task<IActionResult?> ValidarAccesoRegistroAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return User.IsInRole(RolAdministrador) ? null : RedirectToAction(nameof(AccesoDenegado));
            }

            bool existeAdmin = await _context.Usuarios.AnyAsync(u => u.estado && u.Rol!.nameRol == RolAdministrador);
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
                estado = true,
                IDROL = usuarioVM.IDROL
            };
            usuario.password = _passwordHasher.HashPassword(usuario, usuarioVM.password);
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
            string correo = loginVM.correo.Trim();
            Usuario? usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.correo == correo);
            if (usuario == null || usuario.Rol == null)
            {
                ViewData["Mensaje"] = "No se encontraron usuarios";
                return View(loginVM);
            }

            bool passwordValida = false;
            bool requiereRehash = false;
            try
            {
                var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.password, loginVM.password);
                if (resultado == PasswordVerificationResult.Success)
                {
                    passwordValida = true;
                }
                else if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    passwordValida = true;
                    requiereRehash = true;
                }
            }
            catch (FormatException)
            {
                // Contraseña heredada en texto plano
            }

            if (!passwordValida && usuario.password == loginVM.password)
            {
                passwordValida = true;
                requiereRehash = true;
            }

            if (!passwordValida)
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
                usuario.password = _passwordHasher.HashPassword(usuario, loginVM.password);
                await _context.SaveChangesAsync();
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

        [HttpGet]
        [Authorize(Roles = "Administrador")] // Solo los administradores deberían ver esta lista
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .ToListAsync();

            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuario.id.ToString() == currentUserId)
            {
                TempData["Error"] = "No puede desactivar su propia cuenta de administrador.";
                return RedirectToAction(nameof(Index));
            }

            usuario.estado = !usuario.estado;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = usuario.estado
                ? $"Usuario '{usuario.nombreUsuario}' activado correctamente."
                : $"Usuario '{usuario.nombreUsuario}' desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarRolesEdicionAsync(int idRolActual)
        {
            var roles = await _context.Roles
                .Where(r => r.estado || r.idRol == idRolActual)
                .ToListAsync();
            ViewBag.Roles = roles;
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
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.id == id);

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
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.id == id);

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
            if (await _context.Usuarios.AnyAsync(u => u.correo == correoLimpio && u.id != id))
            {
                ViewData["Error"] = "Ya existe otro usuario registrado con ese correo.";
                return View(usuario);
            }

            var rolDestino = await _context.Roles.FirstOrDefaultAsync(r => r.idRol == IDROL);
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
                usuario.password = _passwordHasher.HashPassword(usuario, nuevaPassword);
            }

            await _context.SaveChangesAsync();

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

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.id == idUsuario);

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

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.id == idUsuario);

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

                bool actualValida = false;
                try
                {
                    var res = _passwordHasher.VerifyHashedPassword(usuario, usuario.password, passwordActual);
                    actualValida = res == PasswordVerificationResult.Success || res == PasswordVerificationResult.SuccessRehashNeeded;
                }
                catch (FormatException)
                {
                    // Contraseña heredada en texto plano
                }

                if (!actualValida && usuario.password == passwordActual)
                {
                    actualValida = true;
                }

                if (!actualValida)
                {
                    ViewData["Error"] = "La contraseña actual es incorrecta.";
                    return View(usuario);
                }

                usuario.password = _passwordHasher.HashPassword(usuario, nuevaPassword);
            }

            usuario.nombreUsuario = nombreUsuario.Trim();
            await _context.SaveChangesAsync();
            await RefrescarSesionAsync(usuario);

            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Perfil));
        }
    }
}
