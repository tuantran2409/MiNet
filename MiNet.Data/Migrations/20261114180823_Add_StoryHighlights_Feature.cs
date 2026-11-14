using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNet.Migrations
{
    /// <inheritdoc />
    public partial class Add_StoryHighlights_Feature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HighlightTitle",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlight",
                table: "Stories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighlightTitle",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "IsHighlight",
                table: "Stories");
        }
    }
}
