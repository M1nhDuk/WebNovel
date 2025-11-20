using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelService.Migrations
{
    /// <inheritdoc />
    public partial class ISBNconstrains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "iSBN_13",
                table: "classic_series",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "iSBN_10",
                table: "classic_series",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_classic_series_iSBN_10",
                table: "classic_series",
                column: "iSBN_10",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_classic_series_iSBN_13",
                table: "classic_series",
                column: "iSBN_13",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_classic_series_iSBN_10",
                table: "classic_series");

            migrationBuilder.DropIndex(
                name: "IX_classic_series_iSBN_13",
                table: "classic_series");

            migrationBuilder.AlterColumn<string>(
                name: "iSBN_13",
                table: "classic_series",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "iSBN_10",
                table: "classic_series",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
