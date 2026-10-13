using AppWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<RequerimientoCompra> RequerimientosCompra { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IDROL);
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.IDCATEGORIA);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.UnidadMedida)
                .WithMany()
                .HasForeignKey(p => p.IDUNIDADMEDIDA);

            modelBuilder.Entity<RequerimientoCompra>()
                .HasOne(r => r.Producto)
                .WithMany()
                .HasForeignKey(r => r.IDPRODUCTO);

            modelBuilder.Entity<RequerimientoCompra>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.IDUSUARIO);

            modelBuilder.Entity<Cotizacion>()
                .HasOne(c => c.Cliente)
                .WithMany()
                .HasForeignKey(c => c.IDCLIENTE);

            modelBuilder.Entity<Cotizacion>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.IDUSUARIO);
            //Valores predeterminados
            modelBuilder.Entity<Rol>().HasData(
                new Rol { idRol = 1, nameRol = "Administrador", descripcion = "Encargado de la gestión del sistema" },
                new Rol { idRol = 2, nameRol = "Socio", descripcion = "Empleado que tiene un interés en el negocio" },
                new Rol { idRol = 3, nameRol = "Vendedor", descripcion = "Encargado de la venta de productos" },
                new Rol { idRol = 4, nameRol = "Cajero", descripcion = "Encargado de la caja y el cobro de los productos" },
                new Rol { idRol = 5, nameRol = "Encargado de Inventario", descripcion = "Encargado de la gestión del inventario" },
                new Rol { idRol = 6, nameRol = "Encargado de Compras", descripcion = "Encargado de la gestión de compras" }
            );
        }
    }
}
