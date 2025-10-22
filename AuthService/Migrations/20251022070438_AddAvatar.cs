using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AvataThumbnail",
                table: "Users",
                newName: "AvatarThumbnail");

            migrationBuilder.RenameColumn(
                name: "Avata",
                table: "Users",
                newName: "Avatar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AvatarThumbnail",
                table: "Users",
                newName: "AvataThumbnail");

            migrationBuilder.RenameColumn(
                name: "Avatar",
                table: "Users",
                newName: "Avata");
        }
    }
}
