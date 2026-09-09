using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IntroduceOwnershipModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "IsSuperAdmin",
                table: "Users",
                newName: "IsAdmin");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Surveys",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "AnswerTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_OwnerId",
                table: "Surveys",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_OwnerId",
                table: "Questions",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerTemplates_OwnerId",
                table: "AnswerTemplates",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerTemplates_Users_OwnerId",
                table: "AnswerTemplates",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_OwnerId",
                table: "Questions",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_Users_OwnerId",
                table: "Surveys",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerTemplates_Users_OwnerId",
                table: "AnswerTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_OwnerId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Users_OwnerId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_OwnerId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Questions_OwnerId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_AnswerTemplates_OwnerId",
                table: "AnswerTemplates");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "AnswerTemplates");

            migrationBuilder.RenameColumn(
                name: "IsAdmin",
                table: "Users",
                newName: "IsSuperAdmin");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
