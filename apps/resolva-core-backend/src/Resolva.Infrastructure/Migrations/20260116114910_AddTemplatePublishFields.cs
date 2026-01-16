using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resolva.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplatePublishFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "survey_templates",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "survey_templates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "survey_templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublishedAt",
                table: "survey_templates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "survey_templates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<JsonDocument>(
                name: "ValidationErrors",
                table: "survey_templates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppFlowId",
                table: "survey_templates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppStatus",
                table: "survey_templates",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Channel",
                table: "survey_templates");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "survey_templates");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "survey_templates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "survey_templates");

            migrationBuilder.DropColumn(
                name: "ValidationErrors",
                table: "survey_templates");

            migrationBuilder.DropColumn(
                name: "WhatsAppFlowId",
                table: "survey_templates");

            migrationBuilder.DropColumn(
                name: "WhatsAppStatus",
                table: "survey_templates");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "survey_templates",
                newName: "Status");
        }
    }
}
