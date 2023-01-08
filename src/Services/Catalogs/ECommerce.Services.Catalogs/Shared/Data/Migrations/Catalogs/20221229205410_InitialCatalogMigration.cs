using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ECommerce.Services.Catalogs.Shared.Data.Migrations.Catalogs
{
    /// <inheritdoc />
    public partial class InitialCatalogMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "brands",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(50)", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    createdby = table.Column<int>(name: "created_by", type: "integer", nullable: true),
                    originalversion = table.Column<long>(name: "original_version", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(50)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    createdby = table.Column<int>(name: "created_by", type: "integer", nullable: true),
                    originalversion = table.Column<long>(name: "original_version", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_views",
                schema: "catalog",
                columns: table => new
                {
                    productid = table.Column<long>(name: "product_id", type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    productname = table.Column<string>(name: "product_name", type: "text", nullable: false),
                    categoryid = table.Column<long>(name: "category_id", type: "bigint", nullable: false),
                    categoryname = table.Column<string>(name: "category_name", type: "text", nullable: false),
                    supplierid = table.Column<long>(name: "supplier_id", type: "bigint", nullable: false),
                    suppliername = table.Column<string>(name: "supplier_name", type: "text", nullable: false),
                    brandid = table.Column<long>(name: "brand_id", type: "bigint", nullable: false),
                    brandname = table.Column<string>(name: "brand_name", type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_views", x => x.productid);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(50)", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    createdby = table.Column<int>(name: "created_by", type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    color = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, defaultValue: "Black"),
                    productstatus = table.Column<string>(name: "product_status", type: "character varying(25)", maxLength: 25, nullable: false, defaultValue: "Available"),
                    categoryid = table.Column<long>(name: "category_id", type: "bigint", nullable: false),
                    supplierid = table.Column<long>(name: "supplier_id", type: "bigint", nullable: false),
                    brandid = table.Column<long>(name: "brand_id", type: "bigint", nullable: false),
                    size = table.Column<string>(type: "text", nullable: false),
                    stockavailable = table.Column<int>(name: "stock_available", type: "integer", nullable: false),
                    stockrestockthreshold = table.Column<int>(name: "stock_restock_threshold", type: "integer", nullable: false),
                    stockmaxstockthreshold = table.Column<int>(name: "stock_max_stock_threshold", type: "integer", nullable: false),
                    dimensionsheight = table.Column<int>(name: "dimensions_height", type: "integer", nullable: false),
                    dimensionswidth = table.Column<int>(name: "dimensions_width", type: "integer", nullable: false),
                    dimensionsdepth = table.Column<int>(name: "dimensions_depth", type: "integer", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    createdby = table.Column<int>(name: "created_by", type: "integer", nullable: true),
                    originalversion = table.Column<long>(name: "original_version", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_brands_brand_id",
                        column: x => x.brandid,
                        principalSchema: "catalog",
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.categoryid,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_products_suppliers_supplier_id",
                        column: x => x.supplierid,
                        principalSchema: "catalog",
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    imageurl = table.Column<string>(name: "image_url", type: "text", nullable: false),
                    ismain = table.Column<bool>(name: "is_main", type: "boolean", nullable: false),
                    productid = table.Column<long>(name: "product_id", type: "bigint", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdby = table.Column<int>(name: "created_by", type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_images_products_product_id",
                        column: x => x.productid,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_brands_id",
                schema: "catalog",
                table: "brands",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_id",
                schema: "catalog",
                table: "categories",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_images_id",
                schema: "catalog",
                table: "product_images",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_images_product_id",
                schema: "catalog",
                table: "product_images",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_views_product_id",
                schema: "catalog",
                table: "product_views",
                column: "product_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_brand_id",
                schema: "catalog",
                table: "products",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                schema: "catalog",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_id",
                schema: "catalog",
                table: "products",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_supplier_id",
                schema: "catalog",
                table: "products",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_id",
                schema: "catalog",
                table: "suppliers",
                column: "id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_images",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_views",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "products",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "brands",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "suppliers",
                schema: "catalog");
        }
    }
}
