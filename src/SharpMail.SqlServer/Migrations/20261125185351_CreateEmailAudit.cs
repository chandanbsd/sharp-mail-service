using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharpMail.SqlServer.Migrations;

/// <inheritdoc />
public partial class _20261125185351_CreateEmailAudit : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SharpMailAttempts",
            columns: table => new
            {
                AttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Transport = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                ProviderMessageId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                Payload = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SharpMailAttempts", x => x.AttemptId);
            });

        migrationBuilder.CreateTable(
            name: "SharpMailDeliveries",
            columns: table => new
            {
                AttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Payload = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SharpMailDeliveries", x => new { x.AttemptId, x.EventKey });
                table.ForeignKey(
                    name: "FK_SharpMailDeliveries_SharpMailAttempts_AttemptId",
                    column: x => x.AttemptId,
                    principalTable: "SharpMailAttempts",
                    principalColumn: "AttemptId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SharpMailAttempts_Transport_ProviderMessageId",
            table: "SharpMailAttempts",
            columns: new[] { "Transport", "ProviderMessageId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SharpMailDeliveries");

        migrationBuilder.DropTable(
            name: "SharpMailAttempts");
    }
}
