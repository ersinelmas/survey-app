using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyResponseTextValueAndMultipleChoiceSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponses_AnswerOptions_SelectedOptionId",
                table: "SurveyResponses");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponses_SurveyId_RespondentToken_QuestionId",
                table: "SurveyResponses");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponses_SurveyId_UserId_QuestionId",
                table: "SurveyResponses");

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectedOptionId",
                table: "SurveyResponses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "TextValue",
                table: "SurveyResponses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_SurveyId_RespondentToken_QuestionId_Selecte~",
                table: "SurveyResponses",
                columns: new[] { "SurveyId", "RespondentToken", "QuestionId", "SelectedOptionId" },
                unique: true,
                filter: "\"RespondentToken\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_SurveyId_UserId_QuestionId_SelectedOptionId",
                table: "SurveyResponses",
                columns: new[] { "SurveyId", "UserId", "QuestionId", "SelectedOptionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponses_AnswerOptions_SelectedOptionId",
                table: "SurveyResponses",
                column: "SelectedOptionId",
                principalTable: "AnswerOptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponses_AnswerOptions_SelectedOptionId",
                table: "SurveyResponses");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponses_SurveyId_RespondentToken_QuestionId_Selecte~",
                table: "SurveyResponses");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponses_SurveyId_UserId_QuestionId_SelectedOptionId",
                table: "SurveyResponses");

            migrationBuilder.DropColumn(
                name: "TextValue",
                table: "SurveyResponses");

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectedOptionId",
                table: "SurveyResponses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_SurveyId_RespondentToken_QuestionId",
                table: "SurveyResponses",
                columns: new[] { "SurveyId", "RespondentToken", "QuestionId" },
                unique: true,
                filter: "\"RespondentToken\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_SurveyId_UserId_QuestionId",
                table: "SurveyResponses",
                columns: new[] { "SurveyId", "UserId", "QuestionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponses_AnswerOptions_SelectedOptionId",
                table: "SurveyResponses",
                column: "SelectedOptionId",
                principalTable: "AnswerOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
