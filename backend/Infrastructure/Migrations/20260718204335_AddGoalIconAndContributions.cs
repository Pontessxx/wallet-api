using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalIconAndContributions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "icon_key",
                table: "goals",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "target");

            migrationBuilder.CreateTable(
                name: "goal_contributions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_recurring = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goal_contributions", x => x.id);
                    table.CheckConstraint("ck_goal_contributions_amount_positive", "amount > 0");
                    table.ForeignKey(
                        name: "FK_goal_contributions_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_goal_contributions_goal_id",
                table: "goal_contributions",
                column: "goal_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goal_contributions");

            migrationBuilder.DropColumn(
                name: "icon_key",
                table: "goals");
        }
    }
}
