using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Services.Customers.Shared.Data.Migrations.Customer
{
    public partial class InitialCustomersMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customer");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "customers",
                schema: "customer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name_first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name_last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address_country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address_city = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    address_detail = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nationality = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    phone_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    original_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "restock_subscriptions",
                schema: "customer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    product_information_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    product_information_id = table.Column<long>(type: "bigint", nullable: false),
                    processed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    processed_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    original_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restock_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_restock_subscriptions_customers_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "customer",
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_customers_email",
                schema: "customer",
                table: "customers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customers_id",
                schema: "customer",
                table: "customers",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customers_identity_id",
                schema: "customer",
                table: "customers",
                column: "identity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customers_phone_number",
                schema: "customer",
                table: "customers",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_restock_subscriptions_customer_id",
                schema: "customer",
                table: "restock_subscriptions",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_restock_subscriptions_id",
                schema: "customer",
                table: "restock_subscriptions",
                column: "id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "restock_subscriptions",
                schema: "customer");

            migrationBuilder.DropTable(
                name: "customers",
                schema: "customer");
        }
    }
}
