using AppWeb.Data;
using AppWeb.Models;
using AppWeb.Repositories;
using AppWeb.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Tests
{
    public class ReglasNegocioTests
    {
        private static (AppDbContext Context, SqliteConnection Connection) CrearBaseEnMemoria()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            SembrarDatosBase(context);
            return (context, connection);
        }

        private static void SembrarDatosBase(AppDbContext context)
        {
            var rol = new Rol { nameRol = "Administrador", descripcion = "Admin", estado = true };
            context.Roles.Add(rol);
            context.SaveChanges();

            var usuario = new Usuario
            {
                nombreUsuario = "Luis Angel",
                password = "HashPassword123",
                correo = "admin@test.pe",
                telefono = "+51 956521578",
                estado = true,
                IDROL = rol.idRol
            };
            context.Usuarios.Add(usuario);

            var cliente = new Cliente
            {
                tipoDocumento = "DNI",
                numeroDocumento = "72345678",
                nombres = "Carlos Ruiz",
                razonSocial = "Constructora Ruiz S.A.C.",
                telefono = "+51 987654321",
                direccion = "Av. Principal 100",
                email = "carlos@ruiz.pe",
                estado = true
            };
            context.Clientes.Add(cliente);

            var categoria = new Categoria { nombre = "Ferretería", descripcion = "General", estado = true };
            var unidad = new UnidadMedida { nombre = "Unidad", abreviatura = "UNI", estado = true };
            context.Categorias.Add(categoria);
            context.UnidadesMedida.Add(unidad);
            context.SaveChanges();

            var producto = new Producto
            {
                codigoBarras = "PROD-001",
                nombre = "Martillo Acero",
                stockActual = 10,
                stockMinimo = 2,
                precioCosto = 20m,
                precioVenta = 50m,
                estado = true,
                idCategoria = categoria.idCategoria,
                idUnidadMedida = unidad.idUnidadMedida
            };
            context.Productos.Add(producto);

            context.ConfiguracionesEmpresa.Add(new ConfiguracionEmpresa
            {
                razonSocial = "Ferretería Demo S.A.C.",
                ruc = "20123456789",
                moneda = "PEN",
                igv = 18m,
                serieBoleta = "B001",
                serieFactura = "F001"
            });

            context.SaveChanges();
        }

        private static VentaService CrearVentaService(AppDbContext context) => new(
            new Repository<Venta>(context),
            new Repository<Cliente>(context),
            new Repository<Cotizacion>(context),
            new Repository<Producto>(context),
            new Repository<Stock>(context),
            new Repository<ConfiguracionEmpresa>(context),
            context);

        private static CajaService CrearCajaService(AppDbContext context) => new(
            new Repository<Pago>(context),
            new Repository<Venta>(context),
            new Repository<Comprobante>(context),
            new Repository<ConfiguracionEmpresa>(context),
            context);

        [Fact]
        public async Task CrearVenta_ConStockSuficiente_DescuentaStockYAplicaIgv()
        {
            var (context, connection) = CrearBaseEnMemoria();
            using (connection)
            using (context)
            {
                var service = CrearVentaService(context);
                var cliente = context.Clientes.First();
                var usuario = context.Usuarios.First();
                var producto = context.Productos.First();

                var detalles = new List<DetalleVenta>
                {
                    new() { idproducto = producto.idProducto, cantidad = 2, precioUnitario = 50m, descuento = 0m, subtotal = 100m }
                };

                var resultado = await service.CrearVentaAsync(cliente.idCliente, usuario.id, null, aplicarIgv: true, detalles);

                Assert.True(resultado.Ok);
                Assert.NotNull(resultado.IdVenta);

                var venta = await context.Ventas.FindAsync(resultado.IdVenta);
                Assert.NotNull(venta);
                Assert.Equal(118m, venta!.total);
                Assert.Equal("Pendiente", venta.estado);

                var productoActualizado = await context.Productos.FindAsync(producto.idProducto);
                Assert.Equal(8, productoActualizado!.stockActual);
            }
        }

        [Fact]
        public async Task CrearVenta_ConStockInsuficiente_FallaYNoDescuentaStock()
        {
            var (context, connection) = CrearBaseEnMemoria();
            using (connection)
            using (context)
            {
                var service = CrearVentaService(context);
                var cliente = context.Clientes.First();
                var usuario = context.Usuarios.First();
                var producto = context.Productos.First();

                var detalles = new List<DetalleVenta>
                {
                    new() { idproducto = producto.idProducto, cantidad = 50, precioUnitario = 50m, descuento = 0m, subtotal = 2500m }
                };

                var resultado = await service.CrearVentaAsync(cliente.idCliente, usuario.id, null, aplicarIgv: false, detalles);

                Assert.False(resultado.Ok);
                Assert.Equal(0, await context.Ventas.CountAsync());
                Assert.Equal(10, (await context.Productos.FindAsync(producto.idProducto))!.stockActual);
            }
        }

        [Fact]
        public async Task CrearVenta_ConCotizacionConvertida_EsRechazada()
        {
            var (context, connection) = CrearBaseEnMemoria();
            using (connection)
            using (context)
            {
                var service = CrearVentaService(context);
                var cliente = context.Clientes.First();
                var usuario = context.Usuarios.First();
                var producto = context.Productos.First();

                var cotizacion = new Cotizacion
                {
                    fechaEmision = DateTime.Now,
                    estado = "Convertida",
                    observaciones = "Ya usada",
                    total = 50m,
                    idCliente = cliente.idCliente,
                    idUsuario = usuario.id
                };
                context.Cotizaciones.Add(cotizacion);
                await context.SaveChangesAsync();

                var detalles = new List<DetalleVenta>
                {
                    new() { idproducto = producto.idProducto, cantidad = 1, precioUnitario = 50m, descuento = 0m, subtotal = 50m }
                };

                var resultado = await service.CrearVentaAsync(cliente.idCliente, usuario.id, cotizacion.idCotizacion, aplicarIgv: false, detalles);

                Assert.False(resultado.Ok);
                Assert.Contains("Convertida", resultado.Error);
                Assert.Equal(0, await context.Ventas.CountAsync());
            }
        }

        [Fact]
        public async Task RegistrarCobro_ValidaMetodoPagoYTipoComprobante_YGeneraCorrelativo()
        {
            var (context, connection) = CrearBaseEnMemoria();
            using (connection)
            using (context)
            {
                var ventaService = CrearVentaService(context);
                var cajaService = CrearCajaService(context);
                var cliente = context.Clientes.First();
                var usuario = context.Usuarios.First();
                var producto = context.Productos.First();

                var detalles = new List<DetalleVenta>
                {
                    new() { idproducto = producto.idProducto, cantidad = 1, precioUnitario = 50m, descuento = 0m, subtotal = 50m }
                };
                var ventaRes = await ventaService.CrearVentaAsync(cliente.idCliente, usuario.id, null, aplicarIgv: false, detalles);
                int idVenta = ventaRes.IdVenta!.Value;

                // Casos inválidos
                var metodoInvalido = await cajaService.RegistrarCobroAsync(idVenta, usuario.id, "Bitcoin", 50m, "Boleta");
                Assert.False(metodoInvalido.Ok);

                var compInvalido = await cajaService.RegistrarCobroAsync(idVenta, usuario.id, "Efectivo", 50m, "NotaCredito");
                Assert.False(compInvalido.Ok);

                var montoMenor = await cajaService.RegistrarCobroAsync(idVenta, usuario.id, "Efectivo", 10m, "Boleta");
                Assert.False(montoMenor.Ok);

                // Caso válido
                var cobroOk = await cajaService.RegistrarCobroAsync(idVenta, usuario.id, "Efectivo", 100m, "Boleta");
                Assert.True(cobroOk.Ok);
                Assert.Equal("B001-00000001", cobroOk.SerieNumero);

                // Re-cobrar venta ya pagada debe fallar
                var cobroDuplicado = await cajaService.RegistrarCobroAsync(idVenta, usuario.id, "Efectivo", 100m, "Boleta");
                Assert.False(cobroDuplicado.Ok);
            }
        }
    }
}
