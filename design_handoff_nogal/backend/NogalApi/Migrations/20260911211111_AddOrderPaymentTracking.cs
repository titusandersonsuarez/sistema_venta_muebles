using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NogalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPaymentTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PagoActualizadoEn",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PagoProveedor",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PagoTransaccionId",
                table: "Orders",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PagoActualizadoEn",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PagoProveedor",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PagoTransaccionId",
                table: "Orders");
        }
    }
}
