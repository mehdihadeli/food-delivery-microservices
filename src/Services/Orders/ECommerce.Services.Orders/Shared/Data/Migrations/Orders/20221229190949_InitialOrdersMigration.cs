using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Services.Orders.Shared.Data.Migrations.Orders
{
    /// <inheritdoc />
    public partial class InitialOrdersMigration : Migration
    {
        /// <inheritdoc />
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
                    customername = table.Column<string>(name: "customer_name", type: "text", nullable: false),
                    customercustomerid = table.Column<long>(name: "customer_customer_id", type: "bigint", nullable: false),
                    productname = table.Column<string>(name: "product_name", type: "text", nullable: false),
                    productproductid = table.Column<long>(name: "product_product_id", type: "bigint", nullable: false),
                    productprice = table.Column<decimal>(name: "product_price", type: "numeric", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdby = table.Column<int>(name: "created_by", type: "integer", nullable: true),
                    originalversion = table.Column<long>(name: "original_version", type: "bigint", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orders",
                schema: "order");
        }
    }
}
