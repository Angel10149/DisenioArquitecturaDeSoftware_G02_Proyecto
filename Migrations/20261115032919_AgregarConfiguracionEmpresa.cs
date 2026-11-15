using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppWeb.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConfiguracionEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "motivo",
                table: "RequerimientosCompra",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "Productos",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateTable(
                name: "ConfiguracionesEmpresa",
                columns: table => new
                {
                    idConfiguracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    razonSocial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ruc = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    logoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    igv = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    serieBoleta = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    serieFactura = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    correo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    mensajePie = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    fechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesEmpresa", x => x.idConfiguracion);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesEmpresa");

            migrationBuilder.AlterColumn<string>(
                name: "motivo",
                table: "RequerimientosCompra",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "Productos",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
