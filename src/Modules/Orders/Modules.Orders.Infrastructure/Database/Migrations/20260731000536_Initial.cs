using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Orders.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "orders");

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "VARCHAR(200)", nullable: false),
                    Content = table.Column<string>(type: "JSONB", nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "VARCHAR(256)", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsPermanentFailure = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Launches",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "VARCHAR(200)", nullable: false),
                    DiscountedPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalQuantity = table.Column<int>(type: "integer", nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Launches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderCreationSagas",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Step = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "VARCHAR(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCreationSagas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LaunchId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderCode = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    Status = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "VARCHAR(500)", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "VARCHAR(200)", nullable: false),
                    Content = table.Column<string>(type: "JSONB", nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "VARCHAR(256)", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsPermanentFailure = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessageConsumers",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InboxMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR(256)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessageConsumers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboxMessageConsumers_InboxMessages_InboxMessageId",
                        column: x => x.InboxMessageId,
                        principalSchema: "orders",
                        principalTable: "InboxMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessageConsumers",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OutboxMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR(256)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessageConsumers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboxMessageConsumers_OutboxMessages_OutboxMessageId",
                        column: x => x.OutboxMessageId,
                        principalSchema: "orders",
                        principalTable: "OutboxMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessageConsumers_InboxMessageId_Name",
                schema: "orders",
                table: "InboxMessageConsumers",
                columns: new[] { "InboxMessageId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_CorrelationId",
                schema: "orders",
                table: "InboxMessages",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderCreationSagas_Step_CreatedOn",
                schema: "orders",
                table: "OrderCreationSagas",
                columns: new[] { "Step", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_LaunchId",
                schema: "orders",
                table: "Orders",
                columns: new[] { "CustomerId", "LaunchId" },
                unique: true,
                filter: "\"Status\" IN ('AwaitingPayment', 'PaymentProcessing')");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_LaunchId_Status",
                schema: "orders",
                table: "Orders",
                columns: new[] { "CustomerId", "LaunchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                schema: "orders",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_ExpiresAt",
                schema: "orders",
                table: "Orders",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessageConsumers_OutboxMessageId_Name",
                schema: "orders",
                table: "OutboxMessageConsumers",
                columns: new[] { "OutboxMessageId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CorrelationId",
                schema: "orders",
                table: "OutboxMessages",
                column: "CorrelationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessageConsumers",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "Launches",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "OrderCreationSagas",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "OutboxMessageConsumers",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "orders");
        }
    }
}
