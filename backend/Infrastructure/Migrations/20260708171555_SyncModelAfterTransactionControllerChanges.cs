using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAfterTransactionControllerChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    charges = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_effective = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    posted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    effective_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                    table.CheckConstraint("ck_transactions_amount_positive", "amount > 0");
                    table.CheckConstraint("ck_transactions_charges_non_negative", "charges >= 0");
                    table.CheckConstraint("ck_transactions_effective_date", "(is_effective = false AND effective_at IS NULL) OR (is_effective = true AND effective_at IS NOT NULL AND effective_at >= posted_at)");
                    table.CheckConstraint("ck_transactions_total_amount_consistent", "total_amount = amount + charges");
                    table.ForeignKey(
                        name: "FK_transactions_wallets_wallet_id",
                        column: x => x.wallet_id,
                        principalTable: "wallets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_due_date",
                table: "transactions",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_is_effective",
                table: "transactions",
                column: "is_effective");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_posted_at",
                table: "transactions",
                column: "posted_at");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_type",
                table: "transactions",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_wallet_id",
                table: "transactions",
                column: "wallet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactions");
        }
    }
}
