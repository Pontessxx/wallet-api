using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModelAfterWalletRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stock_transactions_wallet_accounts_wallet_account_id",
                table: "stock_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transfers_wallet_accounts_destination_wallet_account~",
                table: "wallet_transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transfers_wallet_accounts_source_wallet_account_id",
                table: "wallet_transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_wallets_wallet_accounts_wallet_account_id",
                table: "wallets");

            migrationBuilder.DropIndex(
                name: "IX_wallets_wallet_account_id",
                table: "wallets");

            migrationBuilder.DropCheckConstraint(
                name: "ck_wallet_transfers_different_accounts",
                table: "wallet_transfers");

            migrationBuilder.DropColumn(
                name: "description",
                table: "wallets");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "wallets",
                type: "uuid",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "source_wallet_account_id",
                table: "wallet_transfers",
                newName: "source_wallet_id");

            migrationBuilder.RenameColumn(
                name: "destination_wallet_account_id",
                table: "wallet_transfers",
                newName: "destination_wallet_id");

            migrationBuilder.RenameIndex(
                name: "IX_wallet_transfers_source_wallet_account_id",
                table: "wallet_transfers",
                newName: "IX_wallet_transfers_source_wallet_id");

            migrationBuilder.RenameIndex(
                name: "IX_wallet_transfers_destination_wallet_account_id",
                table: "wallet_transfers",
                newName: "IX_wallet_transfers_destination_wallet_id");

            migrationBuilder.RenameColumn(
                name: "wallet_account_id",
                table: "stock_transactions",
                newName: "wallet_id");

            migrationBuilder.RenameIndex(
                name: "IX_stock_transactions_wallet_account_id",
                table: "stock_transactions",
                newName: "IX_stock_transactions_wallet_id");

            migrationBuilder.Sql("""
                UPDATE wallets w
                SET user_id = wa.user_id
                FROM wallet_accounts wa
                WHERE w.wallet_account_id = wa.id;
                """);

            migrationBuilder.Sql("""
                UPDATE stock_transactions st
                SET wallet_id = w.id
                FROM wallets w
                WHERE st.wallet_id = w.wallet_account_id;
                """);

            migrationBuilder.Sql("""
                UPDATE wallet_transfers wt
                SET source_wallet_id = ws.id
                FROM wallets ws
                WHERE wt.source_wallet_id = ws.wallet_account_id;
                """);

            migrationBuilder.Sql("""
                UPDATE wallet_transfers wt
                SET destination_wallet_id = wd.id
                FROM wallets wd
                WHERE wt.destination_wallet_id = wd.wallet_account_id;
                """);

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "wallets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "wallets",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE wallets w
                SET
                    category = wa.category,
                    name = wa.name
                FROM wallet_accounts wa
                WHERE w.wallet_account_id = wa.id;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "wallets",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "wallet_account_id",
                table: "wallets");

            migrationBuilder.DropTable(
                name: "wallet_accounts");

            migrationBuilder.CreateIndex(
                name: "IX_wallets_user_id",
                table: "wallets",
                column: "user_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_wallet_transfers_different_wallets",
                table: "wallet_transfers",
                sql: "source_wallet_id <> destination_wallet_id");

            migrationBuilder.AddForeignKey(
                name: "FK_stock_transactions_wallets_wallet_id",
                table: "stock_transactions",
                column: "wallet_id",
                principalTable: "wallets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transfers_wallets_destination_wallet_id",
                table: "wallet_transfers",
                column: "destination_wallet_id",
                principalTable: "wallets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transfers_wallets_source_wallet_id",
                table: "wallet_transfers",
                column: "source_wallet_id",
                principalTable: "wallets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_wallets_users_user_id",
                table: "wallets",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stock_transactions_wallets_wallet_id",
                table: "stock_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transfers_wallets_destination_wallet_id",
                table: "wallet_transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transfers_wallets_source_wallet_id",
                table: "wallet_transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_wallets_users_user_id",
                table: "wallets");

            migrationBuilder.DropIndex(
                name: "IX_wallets_user_id",
                table: "wallets");

            migrationBuilder.DropCheckConstraint(
                name: "ck_wallet_transfers_different_wallets",
                table: "wallet_transfers");

            migrationBuilder.DropColumn(
                name: "category",
                table: "wallets");

            migrationBuilder.DropColumn(
                name: "name",
                table: "wallets");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "wallets",
                newName: "wallet_account_id");

            migrationBuilder.RenameColumn(
                name: "source_wallet_id",
                table: "wallet_transfers",
                newName: "source_wallet_account_id");

            migrationBuilder.RenameColumn(
                name: "destination_wallet_id",
                table: "wallet_transfers",
                newName: "destination_wallet_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_wallet_transfers_source_wallet_id",
                table: "wallet_transfers",
                newName: "IX_wallet_transfers_source_wallet_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_wallet_transfers_destination_wallet_id",
                table: "wallet_transfers",
                newName: "IX_wallet_transfers_destination_wallet_account_id");

            migrationBuilder.RenameColumn(
                name: "wallet_id",
                table: "stock_transactions",
                newName: "wallet_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_stock_transactions_wallet_id",
                table: "stock_transactions",
                newName: "IX_stock_transactions_wallet_account_id");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "wallets",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "wallet_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_wallet_accounts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wallets_wallet_account_id",
                table: "wallets",
                column: "wallet_account_id",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_wallet_transfers_different_accounts",
                table: "wallet_transfers",
                sql: "source_wallet_account_id <> destination_wallet_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_accounts_user_id",
                table: "wallet_accounts",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_stock_transactions_wallet_accounts_wallet_account_id",
                table: "stock_transactions",
                column: "wallet_account_id",
                principalTable: "wallet_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transfers_wallet_accounts_destination_wallet_account~",
                table: "wallet_transfers",
                column: "destination_wallet_account_id",
                principalTable: "wallet_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transfers_wallet_accounts_source_wallet_account_id",
                table: "wallet_transfers",
                column: "source_wallet_account_id",
                principalTable: "wallet_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_wallets_wallet_accounts_wallet_account_id",
                table: "wallets",
                column: "wallet_account_id",
                principalTable: "wallet_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
