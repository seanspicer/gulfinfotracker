using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GulfInfoTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PluginId = table.Column<string>(type: "text", nullable: false),
                    HeadlineEn = table.Column<string>(type: "text", nullable: false),
                    HeadlineAr = table.Column<string>(type: "text", nullable: true),
                    SummaryEn = table.Column<string>(type: "text", nullable: true),
                    SummaryAr = table.Column<string>(type: "text", nullable: true),
                    SourceUrl = table.Column<string>(type: "text", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IngestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    CredibilityScore = table.Column<int>(type: "integer", nullable: true),
                    CredibilityReasoning = table.Column<string>(type: "text", nullable: true),
                    FullText = table.Column<bool>(type: "boolean", nullable: false),
                    Translated = table.Column<bool>(type: "boolean", nullable: false),
                    ScoringAttempts = table.Column<int>(type: "integer", nullable: false),
                    NamedEntitiesJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourcePollLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PluginId = table.Column<string>(type: "text", nullable: false),
                    PolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ArticlesIngested = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourcePollLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LabelEn = table.Column<string>(type: "text", nullable: false),
                    LabelAr = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTopics",
                columns: table => new
                {
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTopics", x => new { x.ArticleId, x.TopicId });
                    table.ForeignKey(
                        name: "FK_ArticleTopics_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "LabelAr", "LabelEn" },
                values: new object[,]
                {
                    { "T1", "السياسة والحكومة", "Politics & Government" },
                    { "T2", "الاقتصاد والمالية", "Economy & Finance" },
                    { "T3", "الطاقة والنفط", "Energy & Oil" },
                    { "T4", "الأعمال والاستثمار", "Business & Investment" },
                    { "T5", "الصراع الإيراني / الإسرائيلي / الأمريكي", "Iran / Israel / US Conflict" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Country",
                table: "Articles",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CredibilityScore",
                table: "Articles",
                column: "CredibilityScore");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IngestedAt",
                table: "Articles",
                column: "IngestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_PluginId",
                table: "Articles",
                column: "PluginId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_SourceUrl",
                table: "Articles",
                column: "SourceUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTopics_TopicId",
                table: "ArticleTopics",
                column: "TopicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleTopics");

            migrationBuilder.DropTable(
                name: "SourcePollLogs");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Topics");
        }
    }
}
