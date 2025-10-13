using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelService.Migrations
{
    /// <inheritdoc />
    public partial class novelmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    category_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.category_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Novel_Statuses",
                columns: table => new
                {
                    statusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    statusName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Novel_Statuses", x => x.statusId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    tagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    tagName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.tagId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    chapter_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    word_count = table.Column<int>(type: "int", nullable: false),
                    chapter_number = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    series_Id = table.Column<int>(type: "int", nullable: true),
                    novelID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.chapter_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "classic_series",
                columns: table => new
                {
                    series_Id = table.Column<int>(type: "int", nullable: false),
                    ISBN_10 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ISBN_13 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    publisher = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    publish_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    edition = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classic_series", x => x.series_Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "novel_series",
                columns: table => new
                {
                    series_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    series_title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    artist = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    author = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cover_images = table.Column<string>(type: "longtext", nullable: true, defaultValue: "/images/cover/default_cover.jpg")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    word_count = table.Column<int>(type: "int", nullable: false),
                    views = table.Column<int>(type: "int", nullable: false),
                    note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    uploader_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    status_id = table.Column<int>(type: "int", nullable: false),
                    ClassicSeriesseries_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_novel_series", x => x.series_Id);
                    table.ForeignKey(
                        name: "FK_novel_series_Categories_category_id",
                        column: x => x.category_id,
                        principalTable: "Categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_novel_series_Novel_Statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "Novel_Statuses",
                        principalColumn: "statusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_novel_series_classic_series_ClassicSeriesseries_Id",
                        column: x => x.ClassicSeriesseries_Id,
                        principalTable: "classic_series",
                        principalColumn: "series_Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Novel_Tags",
                columns: table => new
                {
                    novelTagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    series_Id = table.Column<int>(type: "int", nullable: false),
                    tagID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Novel_Tags", x => x.novelTagId);
                    table.ForeignKey(
                        name: "FK_Novel_Tags_Tags_tagID",
                        column: x => x.tagID,
                        principalTable: "Tags",
                        principalColumn: "tagId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Novel_Tags_novel_series_series_Id",
                        column: x => x.series_Id,
                        principalTable: "novel_series",
                        principalColumn: "series_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Novels",
                columns: table => new
                {
                    novel_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cover_images = table.Column<string>(type: "longtext", nullable: true, defaultValue: "/images/cover/default_cover.jpg")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    novel_number = table.Column<int>(type: "int", nullable: false),
                    series_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Novels", x => x.novel_Id);
                    table.ForeignKey(
                        name: "FK_Novels_novel_series_series_Id",
                        column: x => x.series_Id,
                        principalTable: "novel_series",
                        principalColumn: "series_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Chapter_Novel_ChapterNumber",
                table: "Chapters",
                columns: new[] { "novelID", "chapter_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chapter_Series_ChapterNumber",
                table: "Chapters",
                columns: new[] { "series_Id", "chapter_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_novel_series_category_id",
                table: "novel_series",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_novel_series_ClassicSeriesseries_Id",
                table: "novel_series",
                column: "ClassicSeriesseries_Id");

            migrationBuilder.CreateIndex(
                name: "IX_novel_series_status_id",
                table: "novel_series",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_Novel_Tags_series_Id_tagID",
                table: "Novel_Tags",
                columns: new[] { "series_Id", "tagID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Novel_Tags_tagID",
                table: "Novel_Tags",
                column: "tagID");

            migrationBuilder.CreateIndex(
                name: "IX_NovelSeries_Novel_ChapterNumber",
                table: "Novels",
                columns: new[] { "series_Id", "novel_number" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Novels_novelID",
                table: "Chapters",
                column: "novelID",
                principalTable: "Novels",
                principalColumn: "novel_Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_novel_series_series_Id",
                table: "Chapters",
                column: "series_Id",
                principalTable: "novel_series",
                principalColumn: "series_Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_classic_series_novel_series_series_Id",
                table: "classic_series",
                column: "series_Id",
                principalTable: "novel_series",
                principalColumn: "series_Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_classic_series_novel_series_series_Id",
                table: "classic_series");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Novel_Tags");

            migrationBuilder.DropTable(
                name: "Novels");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "novel_series");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Novel_Statuses");

            migrationBuilder.DropTable(
                name: "classic_series");
        }
    }
}
