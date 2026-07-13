using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(@"
                WITH distinct_categories AS (
                    SELECT
                        (
                            substr(md5(w.user_id::text || ':' || t.category), 1, 8) || '-' ||
                            substr(md5(w.user_id::text || ':' || t.category), 9, 4) || '-' ||
                            substr(md5(w.user_id::text || ':' || t.category), 13, 4) || '-' ||
                            substr(md5(w.user_id::text || ':' || t.category), 17, 4) || '-' ||
                            substr(md5(w.user_id::text || ':' || t.category), 21, 12)
                        )::uuid AS id,
                        w.user_id,
                        t.category
                    FROM transactions t
                    INNER JOIN wallets w ON w.id = t.wallet_id
                    WHERE t.category IS NOT NULL
                    GROUP BY w.user_id, t.category
                )
                INSERT INTO categories (id, user_id, name, created_at)
                SELECT id, user_id, category, now()
                FROM distinct_categories;");

            migrationBuilder.Sql(@"
                UPDATE transactions t
                SET category_id = c.id
                FROM wallets w, categories c
                WHERE w.id = t.wallet_id
                  AND c.user_id = w.user_id
                  AND c.name = t.category
                  AND t.category IS NOT NULL;");

            migrationBuilder.DropColumn(
                name: "category",
                table: "transactions");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_category_id",
                table: "transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_user_id",
                table: "categories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_user_id_name",
                table: "categories",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_categories_category_id",
                table: "transactions",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "transactions",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE transactions t
                SET category = c.name
                FROM categories c
                WHERE c.id = t.category_id;");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_categories_category_id",
                table: "transactions");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropIndex(
                name: "IX_transactions_category_id",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "transactions");
        }
    }
}
