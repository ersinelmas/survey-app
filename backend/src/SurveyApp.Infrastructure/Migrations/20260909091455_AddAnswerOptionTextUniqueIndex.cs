using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnswerOptionTextUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnswerOptions_AnswerTemplateId",
                table: "AnswerOptions");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_AnswerTemplateId_Text",
                table: "AnswerOptions",
                columns: new[] { "AnswerTemplateId", "Text" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnswerOptions_AnswerTemplateId_Text",
                table: "AnswerOptions");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_AnswerTemplateId",
                table: "AnswerOptions",
                column: "AnswerTemplateId");
        }
    }
}
