using AppWeb.Models;
using AppWeb.Validations;
using AppWeb.ViewModels;

namespace AppWeb.Tests
{
    public class ValidacionesTests
    {
        private sealed class ModeloPruebaFechasYDni
        {
            [CampoObligatorio]
            public string nombre { get; set; } = string.Empty;

            [DniPeruano]
            public string? dni { get; set; }

            [FechaRango(NoPasada = true)]
            public DateOnly? fechaEntrega { get; set; }

            [FechaRango(NoFutura = true, AnioMinimo = 2020, AnioMaximo = 2030)]
            public DateTime? fechaRegistro { get; set; }

            [NumeroPositivo]
            public decimal monto { get; set; } = 1m;
        }

        private static Cliente CrearClienteValido() => new()
        {
            tipoDocumento = "DNI",
            numeroDocumento = "72345678",
            nombres = "Juan Carlos Pérez",
            razonSocial = "Ferretería El Sol S.A.C.",
            telefono = "+51 956521578",
            direccion = "Av. Larco 123",
            email = "juan.perez@empresa.com"
        };

        private static Proveedor CrearProveedorValido() => new()
        {
            ruc = "20123456789",
            razonSocial = "Distribuidora Norte S.R.L.",
            contacto = "Ing. María López",
            telefono = "956-521-578",
            direccion = "Jr. Comercio 450",
            email = "ventas@norte.pe"
        };

        private static ConfiguracionEmpresa CrearConfiguracionValida() => new()
        {
            razonSocial = "Ferretería Central S.A.C.",
            ruc = "20555666777",
            direccion = "Av. España 300",
            telefono = "+51 987654321",
            logoUrl = "/pictures/logo-empresa.webp",
            moneda = "PEN",
            igv = 18m,
            serieBoleta = "B001",
            serieFactura = "F001",
            correo = "contacto@ferreteria.pe"
        };

        // 1. Campos obligatorios
        [Fact]
        public void CampoObligatorio_FallaCuandoEstaVacio_YPasaConTexto()
        {
            var invalido = new ModeloPruebaFechasYDni { nombre = "   ", monto = 10m };
            var resInvalido = ValidationHelper.Validar(invalido);
            Assert.False(resInvalido.EsValido);

            var valido = new ModeloPruebaFechasYDni { nombre = "Pedido 1", monto = 10m };
            var resValido = ValidationHelper.Validar(valido);
            Assert.True(resValido.EsValido);
        }

        // 2. DNI
        [Theory]
        [InlineData("72345678", true)]
        [InlineData("00112233", true)]
        [InlineData("1234567", false)]
        [InlineData("123456789", false)]
        [InlineData("7234567A", false)]
        public void DniPeruano_ValidaCasosValidosEInvalidos(string dni, bool esperado)
        {
            var modelo = new ModeloPruebaFechasYDni { nombre = "Test", dni = dni, monto = 5m };
            Assert.Equal(esperado, ValidationHelper.Validar(modelo).EsValido);

            var cliente = CrearClienteValido();
            cliente.tipoDocumento = "DNI";
            cliente.numeroDocumento = dni;
            Assert.Equal(esperado, ValidationHelper.Validar(cliente).EsValido);
            Assert.Equal(esperado, ValidationHelper.EsDocumentoValido("DNI", dni));
        }

        // 3. RUC
        [Theory]
        [InlineData("20123456789", true)]
        [InlineData("10456789012", true)]
        [InlineData("2012345678", false)]
        [InlineData("201234567890", false)]
        [InlineData("2012345678A", false)]
        public void RucPeruano_ValidaCasosValidosEInvalidos_YReutilizacionEnModelos(string ruc, bool esperado)
        {
            var proveedor = CrearProveedorValido();
            proveedor.ruc = ruc;
            Assert.Equal(esperado, ValidationHelper.Validar(proveedor).EsValido);

            var config = CrearConfiguracionValida();
            config.ruc = ruc;
            Assert.Equal(esperado, ValidationHelper.Validar(config).EsValido);

            var cliente = CrearClienteValido();
            cliente.tipoDocumento = "RUC";
            cliente.numeroDocumento = ruc;
            Assert.Equal(esperado, ValidationHelper.Validar(cliente).EsValido);
        }

        // 4. Correo electrónico
        [Theory]
        [InlineData("usuario@dominio.com", true)]
        [InlineData("ventas.peru@empresa.pe", true)]
        [InlineData("correo-sin-arroba", false)]
        [InlineData("usuario@", false)]
        public void CorreoElectronico_ValidaCasosValidosEInvalidos_EnMultiplesModelos(string correo, bool esperado)
        {
            var cliente = CrearClienteValido();
            cliente.email = correo;
            Assert.Equal(esperado, ValidationHelper.Validar(cliente).EsValido);

            var login = new LoginVM { correo = correo, password = "Password123" };
            Assert.Equal(esperado, ValidationHelper.Validar(login).EsValido);
        }

        // 5. Teléfonos
        [Theory]
        [InlineData("+51 956521578", true)]
        [InlineData("956-521-578", true)]
        [InlineData("044251234", true)]
        [InlineData("12345", false)]
        [InlineData("abc956521578", false)]
        [InlineData("1234567890123456", false)]
        public void TelefonoPeruano_ValidaCasosValidosEInvalidos_YReutilizacion(string telefono, bool esperado)
        {
            var cliente = CrearClienteValido();
            cliente.telefono = telefono;
            Assert.Equal(esperado, ValidationHelper.Validar(cliente).EsValido);

            var proveedor = CrearProveedorValido();
            proveedor.telefono = telefono;
            Assert.Equal(esperado, ValidationHelper.Validar(proveedor).EsValido);

            var editarUsuario = new EditarUsuarioVM
            {
                id = 1,
                nombreUsuario = "Luis Angel",
                correo = "luis@empresa.pe",
                telefono = telefono,
                IDROL = 1
            };
            Assert.Equal(esperado, ValidationHelper.Validar(editarUsuario).EsValido);
        }

        // 6. Números y rangos
        [Fact]
        public void NumerosYRangos_RechazanNegativosYCeroCuandoCorresponde()
        {
            var productoValido = new Producto
            {
                codigoBarras = "SKU-001",
                nombre = "Cemento Sol 42.5kg",
                stockActual = 10,
                stockMinimo = 5,
                precioCosto = 20m,
                precioVenta = 28m,
                idCategoria = 1,
                idUnidadMedida = 1
            };
            Assert.True(ValidationHelper.Validar(productoValido).EsValido);

            productoValido.precioVenta = -1m;
            Assert.False(ValidationHelper.Validar(productoValido).EsValido);

            var detalleCero = new DetalleVenta { cantidad = 0, precioUnitario = 10m, descuento = 0m, subtotal = 0m };
            Assert.False(ValidationHelper.Validar(detalleCero).EsValido);

            var detalleValido = new DetalleVenta { cantidad = 2, precioUnitario = 10m, descuento = 0m, subtotal = 20m };
            Assert.True(ValidationHelper.Validar(detalleValido).EsValido);

            var cfgIgvInvalido = CrearConfiguracionValida();
            cfgIgvInvalido.igv = 45m;
            Assert.False(ValidationHelper.Validar(cfgIgvInvalido).EsValido);
        }

        // 7. Fechas
        [Fact]
        public void FechaRango_ValidaNoPasada_NoFutura_YAniosPermitidos()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var valido = new ModeloPruebaFechasYDni
            {
                nombre = "Orden",
                fechaEntrega = hoy.AddDays(2),
                fechaRegistro = DateTime.Today,
                monto = 100m
            };
            Assert.True(ValidationHelper.Validar(valido).EsValido);

            var fechaPasada = new ModeloPruebaFechasYDni
            {
                nombre = "Orden",
                fechaEntrega = hoy.AddDays(-1),
                monto = 100m
            };
            Assert.False(ValidationHelper.Validar(fechaPasada).EsValido);

            var fechaFutura = new ModeloPruebaFechasYDni
            {
                nombre = "Orden",
                fechaRegistro = DateTime.Today.AddDays(5),
                monto = 100m
            };
            Assert.False(ValidationHelper.Validar(fechaFutura).EsValido);

            var anioFueraDeRango = new ModeloPruebaFechasYDni
            {
                nombre = "Orden",
                fechaRegistro = new DateTime(1995, 5, 1),
                monto = 100m
            };
            Assert.False(ValidationHelper.Validar(anioFueraDeRango).EsValido);
        }

        // 8. Logo (URL o ruta local /pictures/)
        [Theory]
        [InlineData("/pictures/logo-empresa.webp", true)]
        [InlineData("https://ejemplo.com/logo.png", true)]
        [InlineData("texto-cualquiera", false)]
        [InlineData("/otra-carpeta/logo.png", false)]
        public void LogoRutaOUrl_ValidaRutasLocalesYUrlsExternas(string logoUrl, bool esperado)
        {
            var cfg = CrearConfiguracionValida();
            cfg.logoUrl = logoUrl;
            Assert.Equal(esperado, ValidationHelper.Validar(cfg).EsValido);
        }
    }
}
