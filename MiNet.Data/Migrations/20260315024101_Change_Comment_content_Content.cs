using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNet.Migrations
{
    /// <inheritdoc />
    public partial class Change_Comment_content_Content : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "content",
                table: "Comments",
                newName: "Content");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Comments",
                newName: "content");
        }
    }
}
