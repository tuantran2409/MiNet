using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNet.Migrations
{
    /// <inheritdoc />
    public partial class User_changed_ImageUrl_ProfilePictureUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Users",
                newName: "ProfilePictureUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfilePictureUrl",
                table: "Users",
                newName: "ImageUrl");
        }
    }
}
