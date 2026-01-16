using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resolva.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddYourChangeDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_survey_templates_TenantId_EventType_Language_CreatedAt",
                table: "survey_templates");

            migrationBuilder.DropIndex(
                name: "IX_survey_templates_TenantId_EventType_Version",
                table: "survey_templates");

            migrationBuilder.CreateIndex(
                name: "IX_survey_templates_TenantId_CreatedAt",
                table: "survey_templates",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_templates_TenantId_EventType_Language_IsActive",
                table: "survey_templates",
                columns: new[] { "TenantId", "EventType", "Language", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_templates_TenantId_WhatsAppStatus",
                table: "survey_templates",
                columns: new[] { "TenantId", "WhatsAppStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_survey_templates_TenantId_CreatedAt",
                table: "survey_templates");

            migrationBuilder.DropIndex(
                name: "IX_survey_templates_TenantId_EventType_Language_IsActive",
                table: "survey_templates");

            migrationBuilder.DropIndex(
                name: "IX_survey_templates_TenantId_WhatsAppStatus",
                table: "survey_templates");

            migrationBuilder.CreateIndex(
                name: "IX_survey_templates_TenantId_EventType_Language_CreatedAt",
                table: "survey_templates",
                columns: new[] { "TenantId", "EventType", "Language", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_templates_TenantId_EventType_Version",
                table: "survey_templates",
                columns: new[] { "TenantId", "EventType", "Version" });
        }
    }
}
