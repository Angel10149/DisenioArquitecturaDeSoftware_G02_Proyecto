using AppWeb.Data;
using AppWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{

    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CadenaSQL"));
});

//SESIONES
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//Autenticaciones con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Usuario/Login";
    options.AccessDeniedPath = "/Usuario/AccesoDenegado";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed idempotente de roles base y usuario Administrador predeterminado para despliegues nuevos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        var rolesBase = new[]
        {
            new Rol { nameRol = "Administrador", descripcion = "Acceso total al sistema y administración de usuarios", estado = true },
            new Rol { nameRol = "Vendedor", descripcion = "Gestión de cotizaciones, ventas y clientes", estado = true },
            new Rol { nameRol = "Cajero", descripcion = "Cobro de ventas y emisión de comprobantes en caja", estado = true }
        };

        bool rolesAgregados = false;
        foreach (var rolBase in rolesBase)
        {
            if (!db.Roles.Any(r => r.nameRol == rolBase.nameRol))
            {
                db.Roles.Add(rolBase);
                rolesAgregados = true;
            }
        }
        if (rolesAgregados)
        {
            db.SaveChanges();
        }

        var rolAdmin = db.Roles.FirstOrDefault(r => r.nameRol == "Administrador");
        const string correoAdminDefault = "administrador@gmail.com";
        bool existeAdmin = db.Usuarios.Any(u =>
            (u.estado && u.Rol != null && u.Rol.nameRol == "Administrador") ||
            u.correo == correoAdminDefault);

        if (rolAdmin != null && !existeAdmin)
        {
            var passwordHasher = new PasswordHasher<Usuario>();
            var adminDefault = new Usuario
            {
                nombreUsuario = "Administrador",
                correo = correoAdminDefault,
                telefono = "999999999",
                estado = true,
                IDROL = rolAdmin.idRol
            };
            adminDefault.password = passwordHasher.HashPassword(adminDefault, "administrador123");
            db.Usuarios.Add(adminDefault);
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudo ejecutar el seed inicial de roles/administrador.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
    }
    await next();
});

app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
