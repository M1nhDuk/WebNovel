using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class rhFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingHistory_User_Series_Chapter",
                table: "ReadingHistories");

            migrationBuilder.CreateIndex(
                name: "IX_UserReadChapter_User_Series_Chapter_Unique",
                table: "UserReadChapter",
                columns: new[] { "UserId", "SeriesId", "ChapterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReadingHistory_User_Series_Unique",
                table: "ReadingHistories",
                columns: new[] { "UserId", "SeriesId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserReadChapter_User_Series_Chapter_Unique",
                table: "UserReadChapter");

            migrationBuilder.DropIndex(
                name: "IX_ReadingHistory_User_Series_Unique",
                table: "ReadingHistories");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingHistory_User_Series_Chapter",
                table: "ReadingHistories",
                columns: new[] { "UserId", "SeriesId", "ChapterId" },
                unique: true);
        }
    }
}
