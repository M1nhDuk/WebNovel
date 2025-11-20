using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReadingHistoryConstrain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingHistories_UserId_SeriesId",
                table: "ReadingHistories");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingHistory_User_Series_Chapter",
                table: "ReadingHistories",
                columns: new[] { "UserId", "SeriesId", "ChapterId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingHistory_User_Series_Chapter",
                table: "ReadingHistories");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingHistories_UserId_SeriesId",
                table: "ReadingHistories",
                columns: new[] { "UserId", "SeriesId" },
                unique: true);
        }
    }
}
