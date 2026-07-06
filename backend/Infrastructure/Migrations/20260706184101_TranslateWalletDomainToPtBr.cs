using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TranslateWalletDomainToPtBr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE wallet_accounts SET category = 'Investimento' WHERE category = 'Investment';");
            migrationBuilder.Sql("UPDATE wallet_accounts SET category = 'Corrente' WHERE category = 'Checking';");

            migrationBuilder.CreateTable(
                name: "stock_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    side = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
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
                    table.PrimaryKey("PK_stock_transactions", x => x.id);
                    table.CheckConstraint("ck_stock_transactions_amount_positive", "amount > 0");
                    table.CheckConstraint("ck_stock_transactions_charges_non_negative", "charges >= 0");
                    table.CheckConstraint("ck_stock_transactions_effective_date", "(is_effective = false AND effective_at IS NULL) OR (is_effective = true AND effective_at IS NOT NULL AND effective_at >= posted_at)");
                    table.CheckConstraint("ck_stock_transactions_quantity_positive", "quantity > 0");
                    table.CheckConstraint("ck_stock_transactions_unit_price_positive", "unit_price > 0");
                    table.ForeignKey(
                        name: "FK_stock_transactions_wallet_accounts_wallet_account_id",
                        column: x => x.wallet_account_id,
                        principalTable: "wallet_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transfers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_wallet_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination_wallet_account_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_wallet_transfers", x => x.id);
                    table.CheckConstraint("ck_wallet_transfers_amount_positive", "amount > 0");
                    table.CheckConstraint("ck_wallet_transfers_charges_non_negative", "charges >= 0");
                    table.CheckConstraint("ck_wallet_transfers_different_accounts", "source_wallet_account_id <> destination_wallet_account_id");
                    table.CheckConstraint("ck_wallet_transfers_effective_date", "(is_effective = false AND effective_at IS NULL) OR (is_effective = true AND effective_at IS NOT NULL AND effective_at >= posted_at)");
                    table.CheckConstraint("ck_wallet_transfers_total_amount_consistent", "total_amount = amount + charges");
                    table.ForeignKey(
                        name: "FK_wallet_transfers_wallet_accounts_destination_wallet_account~",
                        column: x => x.destination_wallet_account_id,
                        principalTable: "wallet_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_wallet_transfers_wallet_accounts_source_wallet_account_id",
                        column: x => x.source_wallet_account_id,
                        principalTable: "wallet_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stock_transactions_due_date",
                table: "stock_transactions",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transactions_is_effective",
                table: "stock_transactions",
                column: "is_effective");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transactions_posted_at",
                table: "stock_transactions",
                column: "posted_at");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transactions_ticker",
                table: "stock_transactions",
                column: "ticker");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transactions_wallet_account_id",
                table: "stock_transactions",
                column: "wallet_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transfers_destination_wallet_account_id",
                table: "wallet_transfers",
                column: "destination_wallet_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transfers_due_date",
                table: "wallet_transfers",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transfers_is_effective",
                table: "wallet_transfers",
                column: "is_effective");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transfers_posted_at",
                table: "wallet_transfers",
                column: "posted_at");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transfers_source_wallet_account_id",
                table: "wallet_transfers",
                column: "source_wallet_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_transactions");

            migrationBuilder.DropTable(
                name: "wallet_transfers");

            migrationBuilder.Sql("UPDATE wallet_accounts SET category = 'Investment' WHERE category = 'Investimento';");
            migrationBuilder.Sql("UPDATE wallet_accounts SET category = 'Checking' WHERE category = 'Corrente';");
        }
    }
}
