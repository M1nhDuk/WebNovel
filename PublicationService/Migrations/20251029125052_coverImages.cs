using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelService.Migrations
{
    /// <inheritdoc />
    public partial class coverImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "cover_images",
                table: "Novels",
                type: "longtext",
                nullable: true,
                defaultValue: "/images/covers/default_cover.jpg",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true,
                oldDefaultValue: "/images/cover/default_cover.jpg")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "cover_images",
                table: "novel_series",
                type: "longtext",
                nullable: true,
                defaultValue: "/images/covers/default_cover.jpg",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true,
                oldDefaultValue: "/images/cover/default_cover.jpg")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "cover_images",
                table: "Novels",
                type: "longtext",
                nullable: true,
                defaultValue: "/images/cover/default_cover.jpg",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true,
                oldDefaultValue: "/images/covers/default_cover.jpg")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "cover_images",
                table: "novel_series",
                type: "longtext",
                nullable: true,
                defaultValue: "/images/cover/default_cover.jpg",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true,
                oldDefaultValue: "/images/covers/default_cover.jpg")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
