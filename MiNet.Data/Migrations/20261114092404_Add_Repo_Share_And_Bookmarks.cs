using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNet.Migrations
{
    /// <inheritdoc />
    public partial class Add_Repo_Share_And_Bookmarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostRepoShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    RepoFullName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RepoHtmlUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerAvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Forks = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostRepoShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostRepoShares_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepoBookmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RepoFullName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RepoHtmlUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerAvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepoBookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepoBookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostRepoShares_PostId",
                table: "PostRepoShares",
                column: "PostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostRepoShares_RepoFullName",
                table: "PostRepoShares",
                column: "RepoFullName");

            migrationBuilder.CreateIndex(
                name: "IX_RepoBookmarks_UserId_RepoFullName",
                table: "RepoBookmarks",
                columns: new[] { "UserId", "RepoFullName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostRepoShares");

            migrationBuilder.DropTable(
                name: "RepoBookmarks");
        }
    }
}
