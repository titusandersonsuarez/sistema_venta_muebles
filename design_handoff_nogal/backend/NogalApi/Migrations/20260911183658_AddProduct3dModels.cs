using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NogalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProduct3dModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Modelo3dUrl",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModeloUsdzUrl",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Modelo3dUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ModeloUsdzUrl",
                table: "Products");
        }
    }
}
