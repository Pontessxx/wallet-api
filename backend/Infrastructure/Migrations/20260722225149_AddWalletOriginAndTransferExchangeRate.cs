using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletOriginAndTransferExchangeRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "origin",
                table: "wallets",
                type: "text",
                nullable: false,
                defaultValue: "Nacional");

            migrationBuilder.AddColumn<decimal>(
                name: "converted_amount",
                table: "wallet_transfers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "exchange_rate",
                table: "wallet_transfers",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "origin",
                table: "wallets");

            migrationBuilder.DropColumn(
                name: "converted_amount",
                table: "wallet_transfers");

            migrationBuilder.DropColumn(
                name: "exchange_rate",
                table: "wallet_transfers");
        }
    }
}
