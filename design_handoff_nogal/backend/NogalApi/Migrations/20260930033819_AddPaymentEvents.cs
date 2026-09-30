using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NogalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Proveedor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EventoId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    PayloadRaw = table.Column<string>(type: "text", nullable: false),
                    FirmaVerificada = table.Column<bool>(type: "boolean", nullable: false),
                    PedidoCodigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TransaccionId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    EstadoResultado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EventoEmitidoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecibidoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentEvents_Proveedor_EventoId",
                table: "PaymentEvents",
                columns: new[] { "Proveedor", "EventoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentEvents");
        }
    }
}
