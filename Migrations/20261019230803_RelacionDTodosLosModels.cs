using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppWeb.Migrations
{
    /// <inheritdoc />
    public partial class RelacionDTodosLosModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Roles_IDROL",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "Usuario");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_IDROL",
                table: "Usuario",
                newName: "IX_Usuario_IDROL");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuario",
                table: "Usuario",
                column: "id");

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    idCategoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.idCategoria);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    idCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipoDocumento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    numeroDocumento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    razonSocial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.idCliente);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    idProveedor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ruc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    razonSocial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    contacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.idProveedor);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesMedida",
                columns: table => new
                {
                    idUnidadMedida = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    abreviatura = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesMedida", x => x.idUnidadMedida);
                });

            migrationBuilder.CreateTable(
                name: "Cotizaciones",
                columns: table => new
                {
                    idCotizacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    observaciones = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    idCliente = table.Column<int>(type: "int", nullable: false),
                    idUsuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizaciones", x => x.idCotizacion);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Clientes_idCliente",
                        column: x => x.idCliente,
                        principalTable: "Clientes",
                        principalColumn: "idCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Usuario_idUsuario",
                        column: x => x.idUsuario,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    idProducto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigoBarras = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    stockActual = table.Column<int>(type: "int", nullable: false),
                    stockMinimo = table.Column<int>(type: "int", nullable: false),
                    precioCosto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    precioVenta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estado = table.Column<bool>(type: "bit", nullable: false),
                    idCategoria = table.Column<int>(type: "int", nullable: false),
                    idUnidadMedida = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.idProducto);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_idCategoria",
                        column: x => x.idCategoria,
                        principalTable: "Categorias",
                        principalColumn: "idCategoria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productos_UnidadesMedida_idUnidadMedida",
                        column: x => x.idUnidadMedida,
                        principalTable: "UnidadesMedida",
                        principalColumn: "idUnidadMedida",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    idVenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fechaVenta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    idCliente = table.Column<int>(type: "int", nullable: false),
                    idUsuario = table.Column<int>(type: "int", nullable: false),
                    idCotizacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.idVenta);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_idCliente",
                        column: x => x.idCliente,
                        principalTable: "Clientes",
                        principalColumn: "idCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ventas_Cotizaciones_idCotizacion",
                        column: x => x.idCotizacion,
                        principalTable: "Cotizaciones",
                        principalColumn: "idCotizacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Usuario_idUsuario",
                        column: x => x.idUsuario,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleCotizaciones",
                columns: table => new
                {
                    idDetalleCotizacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    precioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    idCotizacion = table.Column<int>(type: "int", nullable: false),
                    idProducto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleCotizaciones", x => x.idDetalleCotizacion);
                    table.ForeignKey(
                        name: "FK_DetalleCotizaciones_Cotizaciones_idCotizacion",
                        column: x => x.idCotizacion,
                        principalTable: "Cotizaciones",
                        principalColumn: "idCotizacion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleCotizaciones_Productos_idProducto",
                        column: x => x.idProducto,
                        principalTable: "Productos",
                        principalColumn: "idProducto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequerimientosCompra",
                columns: table => new
                {
                    idRequerimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cantidadSugerida = table.Column<int>(type: "int", nullable: false),
                    cantidadRequerida = table.Column<int>(type: "int", nullable: false),
                    fechaRequerida = table.Column<DateOnly>(type: "date", nullable: false),
                    prioridad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    motivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    idProducto = table.Column<int>(type: "int", nullable: false),
                    idUsuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequerimientosCompra", x => x.idRequerimiento);
                    table.ForeignKey(
                        name: "FK_RequerimientosCompra_Productos_idProducto",
                        column: x => x.idProducto,
                        principalTable: "Productos",
                        principalColumn: "idProducto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequerimientosCompra_Usuario_idUsuario",
                        column: x => x.idUsuario,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    idStock = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    idProducto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.idStock);
                    table.ForeignKey(
                        name: "FK_Stocks_Productos_idProducto",
                        column: x => x.idProducto,
                        principalTable: "Productos",
                        principalColumn: "idProducto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleVentas",
                columns: table => new
                {
                    idDetalleVenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    precioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    idventa = table.Column<int>(type: "int", nullable: false),
                    idproducto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleVentas", x => x.idDetalleVenta);
                    table.ForeignKey(
                        name: "FK_DetalleVentas_Productos_idproducto",
                        column: x => x.idproducto,
                        principalTable: "Productos",
                        principalColumn: "idProducto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleVentas_Ventas_idventa",
                        column: x => x.idventa,
                        principalTable: "Ventas",
                        principalColumn: "idVenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    idPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    metodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    montoRecibido = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    idVenta = table.Column<int>(type: "int", nullable: false),
                    idUsuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.idPago);
                    table.ForeignKey(
                        name: "FK_Pagos_Usuario_idUsuario",
                        column: x => x.idUsuario,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagos_Ventas_idVenta",
                        column: x => x.idVenta,
                        principalTable: "Ventas",
                        principalColumn: "idVenta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesCompra",
                columns: table => new
                {
                    idOrdenCompra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fechaCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    idProveedor = table.Column<int>(type: "int", nullable: false),
                    idUsuario = table.Column<int>(type: "int", nullable: false),
                    idRequerimiento = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompra", x => x.idOrdenCompra);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Proveedores_idProveedor",
                        column: x => x.idProveedor,
                        principalTable: "Proveedores",
                        principalColumn: "idProveedor",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_RequerimientosCompra_idRequerimiento",
                        column: x => x.idRequerimiento,
                        principalTable: "RequerimientosCompra",
                        principalColumn: "idRequerimiento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Usuario_idUsuario",
                        column: x => x.idUsuario,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comprobantes",
                columns: table => new
                {
                    idComprobante = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipoComprobante = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    serie = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    archivoPdf = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    estado = table.Column<bool>(type: "bit", nullable: false),
                    idPago = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comprobantes", x => x.idComprobante);
                    table.ForeignKey(
                        name: "FK_Comprobantes_Pagos_idPago",
                        column: x => x.idPago,
                        principalTable: "Pagos",
                        principalColumn: "idPago",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleOrdenesCompra",
                columns: table => new
                {
                    idDetalleOrden = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    precioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    idOrdenCompra = table.Column<int>(type: "int", nullable: false),
                    idProducto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleOrdenesCompra", x => x.idDetalleOrden);
                    table.ForeignKey(
                        name: "FK_DetalleOrdenesCompra_OrdenesCompra_idOrdenCompra",
                        column: x => x.idOrdenCompra,
                        principalTable: "OrdenesCompra",
                        principalColumn: "idOrdenCompra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleOrdenesCompra_Productos_idProducto",
                        column: x => x.idProducto,
                        principalTable: "Productos",
                        principalColumn: "idProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IngresosMercaderia",
                columns: table => new
                {
                    idIngresoMercaderia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    observaciones = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    idOrdenCompra = table.Column<int>(type: "int", nullable: false),
                    idProveedor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngresosMercaderia", x => x.idIngresoMercaderia);
                    table.ForeignKey(
                        name: "FK_IngresosMercaderia_OrdenesCompra_idOrdenCompra",
                        column: x => x.idOrdenCompra,
                        principalTable: "OrdenesCompra",
                        principalColumn: "idOrdenCompra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngresosMercaderia_Proveedores_idProveedor",
                        column: x => x.idProveedor,
                        principalTable: "Proveedores",
                        principalColumn: "idProveedor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleIngresos",
                columns: table => new
                {
                    idDetalleIngreso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cantidadRecibida = table.Column<int>(type: "int", nullable: false),
                    cantidadRechazada = table.Column<int>(type: "int", nullable: false),
                    costoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    idIngresoMercaderia = table.Column<int>(type: "int", nullable: false),
                    idProducto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleIngresos", x => x.idDetalleIngreso);
                    table.ForeignKey(
                        name: "FK_DetalleIngresos_IngresosMercaderia_idIngresoMercaderia",
                        column: x => x.idIngresoMercaderia,
                        principalTable: "IngresosMercaderia",
                        principalColumn: "idIngresoMercaderia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleIngresos_Productos_idProducto",
                        column: x => x.idProducto,
                        principalTable: "Productos",
                        principalColumn: "idProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_idPago",
                table: "Comprobantes",
                column: "idPago");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_idCliente",
                table: "Cotizaciones",
                column: "idCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_idUsuario",
                table: "Cotizaciones",
                column: "idUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleCotizaciones_idCotizacion",
                table: "DetalleCotizaciones",
                column: "idCotizacion");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleCotizaciones_idProducto",
                table: "DetalleCotizaciones",
                column: "idProducto");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleIngresos_idIngresoMercaderia",
                table: "DetalleIngresos",
                column: "idIngresoMercaderia");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleIngresos_idProducto",
                table: "DetalleIngresos",
                column: "idProducto");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleOrdenesCompra_idOrdenCompra",
                table: "DetalleOrdenesCompra",
                column: "idOrdenCompra");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleOrdenesCompra_idProducto",
                table: "DetalleOrdenesCompra",
                column: "idProducto");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleVentas_idproducto",
                table: "DetalleVentas",
                column: "idproducto");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleVentas_idventa",
                table: "DetalleVentas",
                column: "idventa");

            migrationBuilder.CreateIndex(
                name: "IX_IngresosMercaderia_idOrdenCompra",
                table: "IngresosMercaderia",
                column: "idOrdenCompra");

            migrationBuilder.CreateIndex(
                name: "IX_IngresosMercaderia_idProveedor",
                table: "IngresosMercaderia",
                column: "idProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_idProveedor",
                table: "OrdenesCompra",
                column: "idProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_idRequerimiento",
                table: "OrdenesCompra",
                column: "idRequerimiento");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_idUsuario",
                table: "OrdenesCompra",
                column: "idUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_idUsuario",
                table: "Pagos",
                column: "idUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_idVenta",
                table: "Pagos",
                column: "idVenta");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_idCategoria",
                table: "Productos",
                column: "idCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_idUnidadMedida",
                table: "Productos",
                column: "idUnidadMedida");

            migrationBuilder.CreateIndex(
                name: "IX_RequerimientosCompra_idProducto",
                table: "RequerimientosCompra",
                column: "idProducto");

            migrationBuilder.CreateIndex(
                name: "IX_RequerimientosCompra_idUsuario",
                table: "RequerimientosCompra",
                column: "idUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_idProducto",
                table: "Stocks",
                column: "idProducto");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_idCliente",
                table: "Ventas",
                column: "idCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_idCotizacion",
                table: "Ventas",
                column: "idCotizacion");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_idUsuario",
                table: "Ventas",
                column: "idUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Roles_IDROL",
                table: "Usuario",
                column: "IDROL",
                principalTable: "Roles",
                principalColumn: "idRol",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Roles_IDROL",
                table: "Usuario");

            migrationBuilder.DropTable(
                name: "Comprobantes");

            migrationBuilder.DropTable(
                name: "DetalleCotizaciones");

            migrationBuilder.DropTable(
                name: "DetalleIngresos");

            migrationBuilder.DropTable(
                name: "DetalleOrdenesCompra");

            migrationBuilder.DropTable(
                name: "DetalleVentas");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "IngresosMercaderia");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "OrdenesCompra");

            migrationBuilder.DropTable(
                name: "Cotizaciones");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "RequerimientosCompra");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "UnidadesMedida");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuario",
                table: "Usuario");

            migrationBuilder.RenameTable(
                name: "Usuario",
                newName: "Usuarios");

            migrationBuilder.RenameIndex(
                name: "IX_Usuario_IDROL",
                table: "Usuarios",
                newName: "IX_Usuarios_IDROL");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Roles_IDROL",
                table: "Usuarios",
                column: "IDROL",
                principalTable: "Roles",
                principalColumn: "idRol",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
