using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryVisualMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "color_hex",
                table: "categories",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#64748B");

            migrationBuilder.AddColumn<string>(
                name: "icon_key",
                table: "categories",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "tag");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color_hex",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "icon_key",
                table: "categories");
        }
    }
}
