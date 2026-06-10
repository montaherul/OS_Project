using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERDRR.Migrations
{
    /// <inheritdoc />
    public partial class AddAlgorithmComparisons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlgorithmComparisons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchedulingSessionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EDFWaitingTime = table.Column<double>(type: "float", nullable: false),
                    EDFTurnaroundTime = table.Column<double>(type: "float", nullable: false),
                    EDFResponseTime = table.Column<double>(type: "float", nullable: false),
                    EDFCPUUtilization = table.Column<double>(type: "float", nullable: false),
                    EDFThroughput = table.Column<double>(type: "float", nullable: false),
                    EDFContextSwitches = table.Column<int>(type: "int", nullable: false),
                    EDFDeadlineMissRatio = table.Column<double>(type: "float", nullable: false),
                    EDFExecutionTime = table.Column<int>(type: "int", nullable: false),
                    RRWaitingTime = table.Column<double>(type: "float", nullable: false),
                    RRTurnaroundTime = table.Column<double>(type: "float", nullable: false),
                    RRResponseTime = table.Column<double>(type: "float", nullable: false),
                    RRCPUUtilization = table.Column<double>(type: "float", nullable: false),
                    RRThroughput = table.Column<double>(type: "float", nullable: false),
                    RRContextSwitches = table.Column<int>(type: "int", nullable: false),
                    RRDeadlineMissRatio = table.Column<double>(type: "float", nullable: false),
                    RRExecutionTime = table.Column<int>(type: "int", nullable: false),
                    HybridWaitingTime = table.Column<double>(type: "float", nullable: false),
                    HybridTurnaroundTime = table.Column<double>(type: "float", nullable: false),
                    HybridResponseTime = table.Column<double>(type: "float", nullable: false),
                    HybridCPUUtilization = table.Column<double>(type: "float", nullable: false),
                    HybridThroughput = table.Column<double>(type: "float", nullable: false),
                    HybridContextSwitches = table.Column<int>(type: "int", nullable: false),
                    HybridDeadlineMissRatio = table.Column<double>(type: "float", nullable: false),
                    HybridExecutionTime = table.Column<int>(type: "int", nullable: false),
                    RecommendedAlgorithm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecommendationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BestScore = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlgorithmComparisons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlgorithmComparisons_SchedulingSessions_SchedulingSessionId",
                        column: x => x.SchedulingSessionId,
                        principalTable: "SchedulingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "8e445865-e2ff-4350-84d0-4c83e07bf1f3",
                columns: new[] { "CreatedAt", "Email", "UserName" },
                values: new object[] { new DateTime(2026, 6, 10, 11, 16, 44, 436, DateTimeKind.Utc).AddTicks(92), "admin@erdrr.com", "admin@erdrr.com" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "CreatedAt", "Email", "UserName" },
                values: new object[] { new DateTime(2026, 6, 10, 11, 16, 44, 436, DateTimeKind.Utc).AddTicks(99), "user@erdrr.com", "user@erdrr.com" });

            migrationBuilder.CreateIndex(
                name: "IX_AlgorithmComparisons_SchedulingSessionId",
                table: "AlgorithmComparisons",
                column: "SchedulingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AlgorithmComparisons_UserId",
                table: "AlgorithmComparisons",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlgorithmComparisons");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "8e445865-e2ff-4350-84d0-4c83e07bf1f3",
                columns: new[] { "CreatedAt", "Email", "UserName" },
                values: new object[] { new DateTime(2026, 6, 10, 8, 31, 39, 262, DateTimeKind.Utc).AddTicks(1428), "admin@erdr.com", "admin@erdr.com" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "CreatedAt", "Email", "UserName" },
                values: new object[] { new DateTime(2026, 6, 10, 8, 31, 39, 262, DateTimeKind.Utc).AddTicks(1435), "user@erdr.com", "user@erdr.com" });
        }
    }
}
