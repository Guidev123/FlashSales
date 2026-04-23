using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Users.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "users",
                columns: table => new
                {
                    Code = table.Column<string>(type: "VARCHAR(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "users",
                columns: table => new
                {
                    Name = table.Column<string>(type: "VARCHAR(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "VARCHAR(160)", nullable: false),
                    FirstName = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    LastName = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    BirthDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IdentiyProviderId = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationTypeRoles",
                schema: "users",
                columns: table => new
                {
                    Type = table.Column<string>(type: "VARCHAR(30)", nullable: false),
                    RoleName = table.Column<string>(type: "VARCHAR(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationTypeRoles", x => new { x.Type, x.RoleName });
                    table.ForeignKey(
                        name: "FK_RegistrationTypeRoles_Roles_RoleName",
                        column: x => x.RoleName,
                        principalSchema: "users",
                        principalTable: "Roles",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "users",
                columns: table => new
                {
                    PermissionCode = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    RoleName = table.Column<string>(type: "VARCHAR(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.PermissionCode, x.RoleName });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionCode",
                        column: x => x.PermissionCode,
                        principalSchema: "users",
                        principalTable: "Permissions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleName",
                        column: x => x.RoleName,
                        principalSchema: "users",
                        principalTable: "Roles",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SellerProfiles",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Document = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    DocumentType = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    BankCode = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    Agency = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    AccountNumber = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    AccountType = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    Status = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    ActivatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "VARCHAR(160)", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "users",
                columns: table => new
                {
                    RoleName = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.RoleName, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleName",
                        column: x => x.RoleName,
                        principalSchema: "users",
                        principalTable: "Roles",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationTypeRoles_RoleName",
                schema: "users",
                table: "RegistrationTypeRoles",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleName",
                schema: "users",
                table: "RolePermissions",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_UserId",
                schema: "users",
                table: "SellerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                schema: "users",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedOn_Id",
                schema: "users",
                table: "Users",
                columns: new[] { "CreatedOn", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "users",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationTypeRoles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "users");

            migrationBuilder.DropTable(
                name: "SellerProfiles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "users");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "users");
        }
    }
}
