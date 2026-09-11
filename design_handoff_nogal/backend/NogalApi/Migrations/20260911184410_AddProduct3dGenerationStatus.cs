using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NogalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProduct3dGenerationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Modelo3dError",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modelo3dEstado",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Modelo3dSolicitadoEn",
                table: "Products",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Modelo3dError",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Modelo3dEstado",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Modelo3dSolicitadoEn",
                table: "Products");
        }
    }
}
