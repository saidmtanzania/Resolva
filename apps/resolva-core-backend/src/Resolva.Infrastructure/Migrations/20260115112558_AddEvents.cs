using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resolva.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactPhone = table.Column<string>(type: "text", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_events_TenantId_EventType_OccurredAt",
                table: "events",
                columns: new[] { "TenantId", "EventType", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_events_TenantId_OccurredAt",
                table: "events",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_events_TenantId_ProductId_OccurredAt",
                table: "events",
                columns: new[] { "TenantId", "ProductId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_events_TenantId_ServiceId_OccurredAt",
                table: "events",
                columns: new[] { "TenantId", "ServiceId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_events_TenantId_Status_OccurredAt",
                table: "events",
                columns: new[] { "TenantId", "Status", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events");
        }
    }
}
