using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNet.Migrations
{
    /// <inheritdoc />
    public partial class Added_Post_Reports_BugFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Report_Posts_PostId",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_Users_UserId",
                table: "Report");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Report",
                table: "Report");

            migrationBuilder.RenameTable(
                name: "Report",
                newName: "Reports");

            migrationBuilder.RenameIndex(
                name: "IX_Report_UserId",
                table: "Reports",
                newName: "IX_Reports_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports",
                table: "Reports",
                columns: new[] { "PostId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Posts_PostId",
                table: "Reports",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Posts_PostId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports",
                table: "Reports");

            migrationBuilder.RenameTable(
                name: "Reports",
                newName: "Report");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_UserId",
                table: "Report",
                newName: "IX_Report_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Report",
                table: "Report",
                columns: new[] { "PostId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Report_Posts_PostId",
                table: "Report",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_Users_UserId",
                table: "Report",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
