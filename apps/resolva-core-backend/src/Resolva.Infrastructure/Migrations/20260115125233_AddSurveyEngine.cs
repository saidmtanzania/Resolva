using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resolva.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "survey_outcomes",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfirmationStatus = table.Column<string>(type: "text", nullable: false),
                    SatisfactionScore = table.Column<decimal>(type: "numeric", nullable: true),
                    Sentiment = table.Column<string>(type: "text", nullable: true),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_outcomes", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "survey_responses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<string>(type: "text", nullable: false),
                    AnswerJson = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_responses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "survey_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientPhone = table.Column<string>(type: "text", nullable: false),
                    Channel = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastInteractionAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReminderCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "survey_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SchemaJson = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_templates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_survey_outcomes_TenantId_ComputedAt",
                table: "survey_outcomes",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_outcomes_TenantId_ConfirmationStatus",
                table: "survey_outcomes",
                columns: new[] { "TenantId", "ConfirmationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_responses_TenantId_CreatedAt",
                table: "survey_responses",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_responses_TenantId_QuestionId",
                table: "survey_responses",
                columns: new[] { "TenantId", "QuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_responses_TenantId_SessionId",
                table: "survey_responses",
                columns: new[] { "TenantId", "SessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_sessions_RecipientPhone",
                table: "survey_sessions",
                column: "RecipientPhone");

            migrationBuilder.CreateIndex(
                name: "IX_survey_sessions_TenantId_EventId",
                table: "survey_sessions",
                columns: new[] { "TenantId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_survey_sessions_TenantId_Status_CreatedAt",
                table: "survey_sessions",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_sessions_TenantId_TemplateId",
                table: "survey_sessions",
                columns: new[] { "TenantId", "TemplateId" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_templates_TenantId_EventType_Language_CreatedAt",
                table: "survey_templates",
                columns: new[] { "TenantId", "EventType", "Language", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_templates_TenantId_EventType_Version",
                table: "survey_templates",
                columns: new[] { "TenantId", "EventType", "Version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "survey_outcomes");

            migrationBuilder.DropTable(
                name: "survey_responses");

            migrationBuilder.DropTable(
                name: "survey_sessions");

            migrationBuilder.DropTable(
                name: "survey_templates");
        }
    }
}
