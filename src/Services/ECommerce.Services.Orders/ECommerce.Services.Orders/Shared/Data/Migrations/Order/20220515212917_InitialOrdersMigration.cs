using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Services.Orders.Shared.Data.Migrations.Order
{
    public partial class InitialOrdersMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "order");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "order",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_customer_id = table.Column<long>(type: "bigint", nullable: false),
                    product_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    product_product_id = table.Column<long>(type: "bigint", nullable: false),
                    product_price = table.Column<decimal>(type: "numeric", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    original_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_orders_id",
                schema: "order",
                table: "orders",
                column: "id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orders",
                schema: "order");
        }
    }
}
