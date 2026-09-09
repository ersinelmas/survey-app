using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicSurveySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponses_Users_UserId",
                table: "SurveyResponses");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Surveys",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireLoginForPublicResponses",
                table: "Surveys",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "SurveyResponses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "RespondentToken",
                table: "SurveyResponses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_SurveyId_RespondentToken_QuestionId",
                table: "SurveyResponses",
                columns: new[] { "SurveyId", "RespondentToken", "QuestionId" },
                unique: true,
                filter: "\"RespondentToken\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponses_Users_UserId",
                table: "SurveyResponses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponses_Users_UserId",
                table: "SurveyResponses");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponses_SurveyId_RespondentToken_QuestionId",
                table: "SurveyResponses");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "RequireLoginForPublicResponses",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "RespondentToken",
                table: "SurveyResponses");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "SurveyResponses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponses_Users_UserId",
                table: "SurveyResponses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
