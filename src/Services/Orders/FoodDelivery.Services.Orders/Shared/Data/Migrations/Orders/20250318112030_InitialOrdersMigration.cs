﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDelivery.Services.Orders.Shared.Data.Migrations.Orders
{
    /// <inheritdoc />
    public partial class InitialOrdersMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "order");

            migrationBuilder.AlterDatabase().Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "order",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    customer_name = table.Column<string>(type: "text", nullable: false),
                    customer_customer_id = table.Column<long>(type: "bigint", nullable: false),
                    product_name = table.Column<string>(type: "text", nullable: false),
                    product_product_id = table.Column<long>(type: "bigint", nullable: false),
                    product_price = table.Column<decimal>(type: "numeric", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_orders_id",
                schema: "order",
                table: "orders",
                column: "id",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "orders", schema: "order");
        }
    }
}
