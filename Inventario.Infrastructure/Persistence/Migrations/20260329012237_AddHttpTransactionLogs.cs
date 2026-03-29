using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventario.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHttpTransactionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HttpTransactionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestTraceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TraceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EndpointName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RoutePattern = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    QueryStringMasked = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    RequestSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    RequestHeadersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestBodyMasked = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestFingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RequestCaptureStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRequestTruncated = table.Column<bool>(type: "bit", nullable: false),
                    ResponseContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ResponseSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ResponseHeadersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseBodyMasked = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseFingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ResponseCaptureStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsResponseTruncated = table.Column<bool>(type: "bit", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    IsIdempotencyReplay = table.Column<bool>(type: "bit", nullable: false),
                    IdempotencyKeyHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    HasSecurityEvent = table.Column<bool>(type: "bit", nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpTransactionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HttpTransactionLogs_CorrelationId",
                table: "HttpTransactionLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_HttpTransactionLogs_CreatedAt",
                table: "HttpTransactionLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HttpTransactionLogs_Path_Method_CreatedAt",
                table: "HttpTransactionLogs",
                columns: new[] { "Path", "Method", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HttpTransactionLogs_RequestTraceId",
                table: "HttpTransactionLogs",
                column: "RequestTraceId",
                unique: true,
                filter: "[RequestTraceId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HttpTransactionLogs");
        }
    }
}
