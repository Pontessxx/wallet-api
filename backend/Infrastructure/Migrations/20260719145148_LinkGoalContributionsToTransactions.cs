using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkGoalContributionsToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "manual_contributions_amount",
                table: "goals");

            migrationBuilder.AddColumn<Guid>(
                name: "goal_id",
                table: "transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "transaction_id",
                table: "goal_contributions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactions_goal_id",
                table: "transactions",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_goal_contributions_transaction_id",
                table: "goal_contributions",
                column: "transaction_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_goal_contributions_transactions_transaction_id",
                table: "goal_contributions",
                column: "transaction_id",
                principalTable: "transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_goals_goal_id",
                table: "transactions",
                column: "goal_id",
                principalTable: "goals",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goal_contributions_transactions_transaction_id",
                table: "goal_contributions");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_goals_goal_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_transactions_goal_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_goal_contributions_transaction_id",
                table: "goal_contributions");

            migrationBuilder.DropColumn(
                name: "goal_id",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "transaction_id",
                table: "goal_contributions");

            migrationBuilder.AddColumn<decimal>(
                name: "manual_contributions_amount",
                table: "goals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
