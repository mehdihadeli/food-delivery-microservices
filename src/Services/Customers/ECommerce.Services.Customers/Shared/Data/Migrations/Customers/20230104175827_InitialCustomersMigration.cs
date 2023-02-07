using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Services.Customers.Shared.Data.Migrations.Customers
{
    /// <inheritdoc />
    public partial class InitialCustomersMigration : Migration
    {
        /// <inheritdoc />
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
                    identityid = table.Column<Guid>(name: "identity_id", type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    namefirstname = table.Column<string>(name: "name_first_name", type: "text", nullable: false),
                    namelastname = table.Column<string>(name: "name_last_name", type: "text", nullable: false),
                    addresscountry = table.Column<string>(name: "address_country", type: "character varying(50)", maxLength: 50, nullable: true),
                    addresscity = table.Column<string>(name: "address_city", type: "character varying(25)", maxLength: 25, nullable: true),
                    addressdetail = table.Column<string>(name: "address_detail", type: "character varying(50)", maxLength: 50, nullable: true),
                    addresspostalcode = table.Column<string>(name: "address_postal_code", type: "text", nullable: true),
                    nationality = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    birthdate = table.Column<DateTime>(name: "birth_date", type: "timestamp with time zone", nullable: true),
                    phonenumber = table.Column<string>(name: "phone_number", type: "character varying(15)", maxLength: 15, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    createdby = table.Column<int>(name: "created_by", type: "integer", nullable: true),
                    originalversion = table.Column<long>(name: "original_version", type: "bigint", nullable: false)
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
                    customerid = table.Column<long>(name: "customer_id", type: "bigint", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    productinformationname = table.Column<string>(name: "product_information_name", type: "text", nullable: false),
                    productinformationid = table.Column<long>(name: "product_information_id", type: "bigint", nullable: true),
                    processed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    processedtime = table.Column<DateTime>(name: "processed_time", type: "timestamp with time zone", nullable: true),
                    isdeleted = table.Column<bool>(name: "is_deleted", type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    createdby = table.Column<int>(name: "created_by", type: "integer", nullable: true),
                    originalversion = table.Column<long>(name: "original_version", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restock_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_restock_subscriptions_customers_customer_id",
                        column: x => x.customerid,
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

        /// <inheritdoc />
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
