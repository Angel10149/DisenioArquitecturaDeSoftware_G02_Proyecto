using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppWeb.Migrations
{
    /// <inheritdoc />
    public partial class AjusteDeleteBehaviorsYValidaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comprobantes_Pagos_idPago",
                table: "Comprobantes");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Clientes_idCliente",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Usuario_idUsuario",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleCotizaciones_Productos_idProducto",
                table: "DetalleCotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVentas_Productos_idproducto",
                table: "DetalleVentas");

            migrationBuilder.DropForeignKey(
                name: "FK_IngresosMercaderia_OrdenesCompra_idOrdenCompra",
                table: "IngresosMercaderia");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_Proveedores_idProveedor",
                table: "OrdenesCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_RequerimientosCompra_idRequerimiento",
                table: "OrdenesCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_Usuario_idUsuario",
                table: "OrdenesCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Usuario_idUsuario",
                table: "Pagos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_idCategoria",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_UnidadesMedida_idUnidadMedida",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_RequerimientosCompra_Productos_idProducto",
                table: "RequerimientosCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_RequerimientosCompra_Usuario_idUsuario",
                table: "RequerimientosCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Roles_IDROL",
                table: "Usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Clientes_idCliente",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Usuario_idUsuario",
                table: "Ventas");

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

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                table: "Ventas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "correo",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comprobantes_Pagos_idPago",
                table: "Comprobantes",
                column: "idPago",
                principalTable: "Pagos",
                principalColumn: "idPago",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Clientes_idCliente",
                table: "Cotizaciones",
                column: "idCliente",
                principalTable: "Clientes",
                principalColumn: "idCliente",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Usuarios_idUsuario",
                table: "Cotizaciones",
                column: "idUsuario",
                principalTable: "Usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleCotizaciones_Productos_idProducto",
                table: "DetalleCotizaciones",
                column: "idProducto",
                principalTable: "Productos",
                principalColumn: "idProducto",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVentas_Productos_idproducto",
                table: "DetalleVentas",
                column: "idproducto",
                principalTable: "Productos",
                principalColumn: "idProducto",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IngresosMercaderia_OrdenesCompra_idOrdenCompra",
                table: "IngresosMercaderia",
                column: "idOrdenCompra",
                principalTable: "OrdenesCompra",
                principalColumn: "idOrdenCompra",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_Proveedores_idProveedor",
                table: "OrdenesCompra",
                column: "idProveedor",
                principalTable: "Proveedores",
                principalColumn: "idProveedor",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_RequerimientosCompra_idRequerimiento",
                table: "OrdenesCompra",
                column: "idRequerimiento",
                principalTable: "RequerimientosCompra",
                principalColumn: "idRequerimiento",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_Usuarios_idUsuario",
                table: "OrdenesCompra",
                column: "idUsuario",
                principalTable: "Usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Usuarios_idUsuario",
                table: "Pagos",
                column: "idUsuario",
                principalTable: "Usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_idCategoria",
                table: "Productos",
                column: "idCategoria",
                principalTable: "Categorias",
                principalColumn: "idCategoria",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_UnidadesMedida_idUnidadMedida",
                table: "Productos",
                column: "idUnidadMedida",
                principalTable: "UnidadesMedida",
                principalColumn: "idUnidadMedida",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequerimientosCompra_Productos_idProducto",
                table: "RequerimientosCompra",
                column: "idProducto",
                principalTable: "Productos",
                principalColumn: "idProducto",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequerimientosCompra_Usuarios_idUsuario",
                table: "RequerimientosCompra",
                column: "idUsuario",
                principalTable: "Usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Roles_IDROL",
                table: "Usuarios",
                column: "IDROL",
                principalTable: "Roles",
                principalColumn: "idRol",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Clientes_idCliente",
                table: "Ventas",
                column: "idCliente",
                principalTable: "Clientes",
                principalColumn: "idCliente",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Usuarios_idUsuario",
                table: "Ventas",
                column: "idUsuario",
                principalTable: "Usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comprobantes_Pagos_idPago",
                table: "Comprobantes");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Clientes_idCliente",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Usuarios_idUsuario",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleCotizaciones_Productos_idProducto",
                table: "DetalleCotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVentas_Productos_idproducto",
                table: "DetalleVentas");

            migrationBuilder.DropForeignKey(
                name: "FK_IngresosMercaderia_OrdenesCompra_idOrdenCompra",
                table: "IngresosMercaderia");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_Proveedores_idProveedor",
                table: "OrdenesCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_RequerimientosCompra_idRequerimiento",
                table: "OrdenesCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_Usuarios_idUsuario",
                table: "OrdenesCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Usuarios_idUsuario",
                table: "Pagos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_idCategoria",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_UnidadesMedida_idUnidadMedida",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_RequerimientosCompra_Productos_idProducto",
                table: "RequerimientosCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_RequerimientosCompra_Usuarios_idUsuario",
                table: "RequerimientosCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Roles_IDROL",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Clientes_idCliente",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Usuarios_idUsuario",
                table: "Ventas");

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

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                table: "Ventas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "Usuario",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "correo",
                table: "Usuario",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuario",
                table: "Usuario",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comprobantes_Pagos_idPago",
                table: "Comprobantes",
                column: "idPago",
                principalTable: "Pagos",
                principalColumn: "idPago",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Clientes_idCliente",
                table: "Cotizaciones",
                column: "idCliente",
                principalTable: "Clientes",
                principalColumn: "idCliente",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Usuario_idUsuario",
                table: "Cotizaciones",
                column: "idUsuario",
                principalTable: "Usuario",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleCotizaciones_Productos_idProducto",
                table: "DetalleCotizaciones",
                column: "idProducto",
                principalTable: "Productos",
                principalColumn: "idProducto",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVentas_Productos_idproducto",
                table: "DetalleVentas",
                column: "idproducto",
                principalTable: "Productos",
                principalColumn: "idProducto",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IngresosMercaderia_OrdenesCompra_idOrdenCompra",
                table: "IngresosMercaderia",
                column: "idOrdenCompra",
                principalTable: "OrdenesCompra",
                principalColumn: "idOrdenCompra",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_Proveedores_idProveedor",
                table: "OrdenesCompra",
                column: "idProveedor",
                principalTable: "Proveedores",
                principalColumn: "idProveedor",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_RequerimientosCompra_idRequerimiento",
                table: "OrdenesCompra",
                column: "idRequerimiento",
                principalTable: "RequerimientosCompra",
                principalColumn: "idRequerimiento",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_Usuario_idUsuario",
                table: "OrdenesCompra",
                column: "idUsuario",
                principalTable: "Usuario",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Usuario_idUsuario",
                table: "Pagos",
                column: "idUsuario",
                principalTable: "Usuario",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_idCategoria",
                table: "Productos",
                column: "idCategoria",
                principalTable: "Categorias",
                principalColumn: "idCategoria",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_UnidadesMedida_idUnidadMedida",
                table: "Productos",
                column: "idUnidadMedida",
                principalTable: "UnidadesMedida",
                principalColumn: "idUnidadMedida",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequerimientosCompra_Productos_idProducto",
                table: "RequerimientosCompra",
                column: "idProducto",
                principalTable: "Productos",
                principalColumn: "idProducto",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequerimientosCompra_Usuario_idUsuario",
                table: "RequerimientosCompra",
                column: "idUsuario",
                principalTable: "Usuario",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Roles_IDROL",
                table: "Usuario",
                column: "IDROL",
                principalTable: "Roles",
                principalColumn: "idRol",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Clientes_idCliente",
                table: "Ventas",
                column: "idCliente",
                principalTable: "Clientes",
                principalColumn: "idCliente",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Usuario_idUsuario",
                table: "Ventas",
                column: "idUsuario",
                principalTable: "Usuario",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
