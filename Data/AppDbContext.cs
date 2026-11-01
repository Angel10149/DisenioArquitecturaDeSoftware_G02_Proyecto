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
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Comprobante> Comprobantes { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<DetalleCotizacion> DetalleCotizaciones { get; set; }
        public DbSet<DetalleIngreso> DetalleIngresos { get; set; }
        public DbSet<DetalleOrdenCompra> DetalleOrdenesCompra { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<IngresoMercaderia> IngresosMercaderia { get; set; }
        public DbSet<OrdenCompra> OrdenesCompra { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<RequerimientoCompra> RequerimientosCompra { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<UnidadMedida> UnidadesMedida { get; set; }
        public DbSet<Venta> Ventas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SEGURIDAD Y USUARIOS
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IDROL)
                .OnDelete(DeleteBehavior.Restrict);

            // PRODUCTOS Y CATÁLOGOS MAESTROS
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.idCategoria)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.unidadMedida)
                .WithMany(u => u.Productos)
                .HasForeignKey(p => p.idUnidadMedida)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
                .HasOne(s => s.producto)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.idProducto)
                .OnDelete(DeleteBehavior.Cascade);

            // COTIZACIONES
            modelBuilder.Entity<Cotizacion>()
                .HasOne(c => c.cliente)
                .WithMany(cl => cl.Cotizacions)
                .HasForeignKey(c => c.idCliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cotizacion>()
                .HasOne(c => c.usuario)
                .WithMany(u => u.Cotizacions)
                .HasForeignKey(c => c.idUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleCotizacion>()
                .HasOne(d => d.cotizacion)
                .WithMany(c => c.DetalleCotizacions)
                .HasForeignKey(d => d.idCotizacion)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleCotizacion>()
                .HasOne(d => d.producto)
                .WithMany(p => p.DetalleCotizacions)
                .HasForeignKey(d => d.idProducto)
                .OnDelete(DeleteBehavior.Restrict);

            // VENTAS, DETALLE, PAGOS Y COMPROBANTES
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.idCliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(v => v.idUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.cotizacion)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.idCotizacion)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.venta)
                .WithMany(v => v.DetalleVentas)
                .HasForeignKey(d => d.idventa)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.producto)
                .WithMany(p => p.DetalleVentas)
                .HasForeignKey(d => d.idproducto)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.venta)
                .WithMany(v => v.Pagos)
                .HasForeignKey(p => p.idVenta)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.usuario)
                .WithMany(v => v.Pagos)
                .HasForeignKey(p => p.idUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comprobante>()
                .HasOne(c => c.pago)
                .WithMany(p => p.Comprobantes)
                .HasForeignKey(c => c.idPago)
                .OnDelete(DeleteBehavior.Restrict);

            // REQUERIMIENTOS Y COMPRAS
            modelBuilder.Entity<RequerimientoCompra>()
                .HasOne(r => r.producto)
                .WithMany(p => p.RequerimientoCompras)
                .HasForeignKey(r => r.idProducto)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RequerimientoCompra>()
                .HasOne(r => r.usuario)
                .WithMany(u => u.RequerimientoCompras)
                .HasForeignKey(r => r.idUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompra>()
                .HasOne(o => o.proveedor)
                .WithMany(p => p.OrdenesCompra)
                .HasForeignKey(o => o.idProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompra>()
                .HasOne(o => o.usuario)
                .WithMany(u => u.OrdenesCompra)
                .HasForeignKey(o => o.idUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompra>()
                .HasOne(o => o.requerimiento)
                .WithMany(r => r.OrdenesCompra)
                .HasForeignKey(o => o.idRequerimiento)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleOrdenCompra>()
                .HasOne(d => d.ordenCompra)
                .WithMany(o => o.DetalleOrdenCompras)
                .HasForeignKey(d => d.idOrdenCompra)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleOrdenCompra>()
                .HasOne(d => d.producto)
                .WithMany(p => p.DetalleOrdenCompras)
                .HasForeignKey(d => d.idProducto)
                .OnDelete(DeleteBehavior.Restrict);

            // INGRESO DE MERCADERÍA
            modelBuilder.Entity<IngresoMercaderia>()
                .HasOne(i => i.ordenCompra)
                .WithMany(o => o.IngresosMercaderia)
                .HasForeignKey(i => i.idOrdenCompra)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IngresoMercaderia>()
                .HasOne(i => i.proveedor)
                .WithMany(p => p.IngresosMercaderia)
                .HasForeignKey(i => i.idProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleIngreso>()
                .HasOne(d => d.ingresoMercaderia)
                .WithMany(i => i.DetalleIngresos)
                .HasForeignKey(d => d.idIngresoMercaderia)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleIngreso>()
                .HasOne(d => d.producto)
                .WithMany(p => p.DetalleIngresos)
                .HasForeignKey(d => d.idProducto)
                .OnDelete(DeleteBehavior.Restrict);

            // Datos Precargados para la tabla de Roles
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