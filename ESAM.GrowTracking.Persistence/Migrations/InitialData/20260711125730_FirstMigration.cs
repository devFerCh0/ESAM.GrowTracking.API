using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESAM.GrowTracking.Persistence.Migrations.InitialData
{
    /// <inheritdoc />
    public partial class FirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WebSite = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FoundingDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SecondLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdentityDocument = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdentityDocumentType = table.Column<byte>(type: "tinyint", nullable: false),
                    Gender = table.Column<byte>(type: "tinyint", nullable: false),
                    MaritalStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    WorkProfileType = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Campuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessUnitId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    WebSite = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FoundingDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campuses_BusinessUnits_BusinessUnitId",
                        column: x => x.BusinessUnitId,
                        principalTable: "BusinessUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "CONVERT(varchar(36), NEWID())"),
                    TokenVersion = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LockoutEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_People_Id",
                        column: x => x.Id,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    HasAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkProfilePermissions",
                columns: table => new
                {
                    WorkProfileId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    HasAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkProfilePermissions", x => new { x.WorkProfileId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_WorkProfilePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkProfilePermissions_WorkProfiles_WorkProfileId",
                        column: x => x.WorkProfileId,
                        principalTable: "WorkProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BlacklistedAccessTokensTemporary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Jti = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BlacklistedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedAccessTokensTemporary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlacklistedAccessTokensTemporary_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApiClientType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsTrusted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastUserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastFailedLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockoutEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPhotos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleCampuses",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleCampuses", x => new { x.UserId, x.RoleId, x.CampusId });
                    table.ForeignKey(
                        name: "FK_UserRoleCampuses_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoleCampuses_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoleCampuses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWorkProfiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WorkProfileId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWorkProfiles", x => new { x.UserId, x.WorkProfileId });
                    table.ForeignKey(
                        name: "FK_UserWorkProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWorkProfiles_WorkProfiles_WorkProfileId",
                        column: x => x.WorkProfileId,
                        principalTable: "WorkProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserDeviceId = table.Column<int>(type: "int", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AbsoluteExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsPersistent = table.Column<bool>(type: "bit", nullable: false),
                    ClosedByUserId = table.Column<int>(type: "int", nullable: true),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_UserDevices_UserDeviceId",
                        column: x => x.UserDeviceId,
                        principalTable: "UserDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BlacklistedAccessTokensSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserSessionId = table.Column<int>(type: "int", nullable: false),
                    Jti = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BlacklistedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedAccessTokensSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlacklistedAccessTokensSession_UserSessions_UserSessionId",
                        column: x => x.UserSessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessionRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserSessionId = table.Column<int>(type: "int", nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RotationCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReplacedByUserSessionRefreshTokenId = table.Column<int>(type: "int", nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessionRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessionRefreshTokens_UserSessionRefreshTokens_ReplacedByUserSessionRefreshTokenId",
                        column: x => x.ReplacedByUserSessionRefreshTokenId,
                        principalTable: "UserSessionRefreshTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSessionRefreshTokens_UserSessions_UserSessionId",
                        column: x => x.UserSessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessionWorkProfilesSelected",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserSessionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WorkProfileId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessionWorkProfilesSelected", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessionWorkProfilesSelected_UserSessions_UserSessionId",
                        column: x => x.UserSessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSessionWorkProfilesSelected_UserWorkProfiles_UserId_WorkProfileId",
                        columns: x => new { x.UserId, x.WorkProfileId },
                        principalTable: "UserWorkProfiles",
                        principalColumns: ["UserId", "WorkProfileId"],
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BlacklistedRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserSessionRefreshTokenId = table.Column<int>(type: "int", nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BlacklistedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    RecordVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlacklistedRefreshTokens_UserSessionRefreshTokens_UserSessionRefreshTokenId",
                        column: x => x.UserSessionRefreshTokenId,
                        principalTable: "UserSessionRefreshTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessionRoleCampusesSelected",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserSessionWorkProfileSelectedId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessionRoleCampusesSelected", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessionRoleCampusesSelected_UserRoleCampuses_UserId_RoleId_CampusId",
                        columns: x => new { x.UserId, x.RoleId, x.CampusId },
                        principalTable: "UserRoleCampuses",
                        principalColumns: ["UserId", "RoleId", "CampusId"],
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSessionRoleCampusesSelected_UserSessionWorkProfilesSelected_UserSessionWorkProfileSelectedId",
                        column: x => x.UserSessionWorkProfileSelectedId,
                        principalTable: "UserSessionWorkProfilesSelected",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "BusinessUnits",
                columns: ["Id", "Abbreviation", "CreatedAt", "CreatedBy", "FoundingDate", "Name", "UpdatedAt", "UpdatedBy", "WebSite"],
                values: [1, "ESAM", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ESAM", null, null, "https://esam.edu.bo/"]);

            migrationBuilder.InsertData(
                table: "Modules",
                columns: ["Id", "Description", "Name"],
                values: [1, null, "Académico"]);

            migrationBuilder.InsertData(
                table: "People",
                columns: ["Id", "CreatedAt", "CreatedBy", "FirstName", "Gender", "IdentityDocument", "IdentityDocumentType", "LastName", "MaritalStatus", "SecondLastName", "UpdatedAt", "UpdatedBy"],
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Luis Fernando", (byte)1, "5681003", (byte)1, "Flores", (byte)2, "Padilla", null, null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Efrain", (byte)1, "13071262", (byte)1, "Chiri", (byte)1, "Nina", null, null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: ["Id", "CreatedAt", "CreatedBy", "Description", "Name", "UpdatedAt", "UpdatedBy"],
                values: [1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, "Coordinador de T. I.", null, null]);

            migrationBuilder.InsertData(
                table: "WorkProfiles",
                columns: ["Id", "Description", "Name", "WorkProfileType"],
                values: new object[,]
                {
                    { 1, null, "Gestor", (byte)1 },
                    { 2, null, "Docente", (byte)2 },
                    { 3, null, "Estudiante", (byte)2 }
                });

            migrationBuilder.InsertData(
                table: "Campuses",
                columns: ["Id", "BusinessUnitId", "CreatedAt", "CreatedBy", "FoundingDate", "Name", "UpdatedAt", "UpdatedBy", "WebSite"],
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ESAM Sucre 2", null, null, "https://esam.edu.bo/Sucre2" },
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ESAM Monteagudo", null, null, "https://esam.edu.bo/Monteagudo" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: ["Id", "Code", "Description", "ModuleId", "Name"],
                values: new object[,]
                {
                    { 1, null, null, 1, "Agregar Proyectos" },
                    { 2, null, null, 1, "Agregar Calificación" },
                    { 3, null, null, 1, "Ver Calificaciones" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: ["Id", "CreatedAt", "CreatedBy", "Email", "LockoutEndAt", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "Salt", "SecurityStamp", "UpdatedAt", "UpdatedBy", "Username"],
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "luis.flores@esam.edu.bo", null, "LUIS.FLORES@ESAM.EDU.BO", "LFLORESPADILLA", "cNqBlSDNez491Q3/7bC8mmNnFisihQ28n1MlWy6fXyU=", "1DAIl850O7FsKxxjnPtRuw==", "2bb48cdd-afbd-48f7-ab11-0cd74eea240e", null, null, "lflorespadilla" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "efrain.chiri@esam.edu.bo", null, "EFRAIN.CHIRI@ESAM.EDU.BO", "ECHIRININA", "PKi+hECJUsg7aujM85GlYNGEAu2J1ZrNS6QqJ603WpU=", "pxAU4s4HEGtDsUFFA3y1vw==", "2f01a267-92db-4703-99f5-5b995167d3bd", null, null, "echirinina" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: ["PermissionId", "RoleId", "CreatedAt", "CreatedBy", "HasAccess", "UpdatedAt", "UpdatedBy"],
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, null, null },
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, null, null },
                    { 3, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, null, null }
                });

            migrationBuilder.InsertData(
                table: "UserRoleCampuses",
                columns: ["CampusId", "RoleId", "UserId", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"],
                values: new object[,]
                {
                    { 1, 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null },
                    { 2, 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null },
                    { 1, 1, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null },
                    { 2, 1, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "UserWorkProfiles",
                columns: ["UserId", "WorkProfileId", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"],
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null },
                    { 1, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null },
                    { 1, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null },
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null },
                    { 2, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null },
                    { 2, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "WorkProfilePermissions",
                columns: ["PermissionId", "WorkProfileId", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"],
                values: [1, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null]);

            migrationBuilder.InsertData(
                table: "WorkProfilePermissions",
                columns: ["PermissionId", "WorkProfileId", "CreatedAt", "CreatedBy", "HasAccess", "UpdatedAt", "UpdatedBy"],
                values: new object[,]
                {
                    { 2, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, null, null },
                    { 3, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, null, null }
                });

            migrationBuilder.InsertData(
                table: "WorkProfilePermissions",
                columns: ["PermissionId", "WorkProfileId", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"],
                values: new object[,]
                {
                    { 1, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null },
                    { 2, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "WorkProfilePermissions",
                columns: ["PermissionId", "WorkProfileId", "CreatedAt", "CreatedBy", "HasAccess", "UpdatedAt", "UpdatedBy"],
                values: [3, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, null, null]);

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedAccessTokensSession_ExpiresAt",
                table: "BlacklistedAccessTokensSession",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedAccessTokensSession_Jti",
                table: "BlacklistedAccessTokensSession",
                column: "Jti");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedAccessTokensSession_UserSessionId",
                table: "BlacklistedAccessTokensSession",
                column: "UserSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedAccessTokensSession_UserSessionId_ExpiresAt",
                table: "BlacklistedAccessTokensSession",
                columns: ["UserSessionId", "ExpiresAt"]);

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedAccessTokensTemporary_ExpiresAt",
                table: "BlacklistedAccessTokensTemporary",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedAccessTokensTemporary_Jti",
                table: "BlacklistedAccessTokensTemporary",
                column: "Jti");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedAccessTokensTemporary_UserId",
                table: "BlacklistedAccessTokensTemporary",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedAccessTokensTemporary_UserId_ExpiresAt",
                table: "BlacklistedAccessTokensTemporary",
                columns: ["UserId", "ExpiresAt"]);

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedRefreshTokens_ExpiresAt",
                table: "BlacklistedRefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedRefreshTokens_Identifier",
                table: "BlacklistedRefreshTokens",
                column: "Identifier");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedRefreshTokens_UserSessionRefreshTokenId",
                table: "BlacklistedRefreshTokens",
                column: "UserSessionRefreshTokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessUnits_Abbreviation",
                table: "BusinessUnits",
                column: "Abbreviation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessUnits_Name",
                table: "BusinessUnits",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessUnits_WebSite",
                table: "BusinessUnits",
                column: "WebSite",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_BusinessUnitId",
                table: "Campuses",
                column: "BusinessUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_Name",
                table: "Campuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_WebSite",
                table: "Campuses",
                column: "WebSite",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Name",
                table: "Modules",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_FirstName",
                table: "People",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_People_Gender",
                table: "People",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_People_IdentityDocument",
                table: "People",
                column: "IdentityDocument",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_IdentityDocumentType",
                table: "People",
                column: "IdentityDocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_People_LastName",
                table: "People",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_People_MaritalStatus",
                table: "People",
                column: "MaritalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_People_SecondLastName",
                table: "People",
                column: "SecondLastName");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ModuleId",
                table: "Permissions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_ApiClientType",
                table: "UserDevices",
                column: "ApiClientType");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_DeviceIdentifier",
                table: "UserDevices",
                column: "DeviceIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId",
                table: "UserDevices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId_IsDeleted",
                table: "UserDevices",
                columns: ["UserId", "IsDeleted"]);

            migrationBuilder.CreateIndex(
                name: "IX_UserPhotos_UserId",
                table: "UserPhotos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPhotos_UserId_IsDefault",
                table: "UserPhotos",
                columns: ["UserId", "IsDefault"],
                unique: true,
                filter: "[IsDefault] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleCampuses_CampusId",
                table: "UserRoleCampuses",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleCampuses_RoleId",
                table: "UserRoleCampuses",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedUserName",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionRefreshTokens_ExpiresAt",
                table: "UserSessionRefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionRefreshTokens_Identifier",
                table: "UserSessionRefreshTokens",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionRefreshTokens_ReplacedByUserSessionRefreshTokenId",
                table: "UserSessionRefreshTokens",
                column: "ReplacedByUserSessionRefreshTokenId",
                unique: true,
                filter: "[ReplacedByUserSessionRefreshTokenId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionRefreshTokens_UserSessionId",
                table: "UserSessionRefreshTokens",
                column: "UserSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionRefreshTokens_UserSessionId_IsRevoked",
                table: "UserSessionRefreshTokens",
                columns: ["UserSessionId", "IsRevoked"]);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionRoleCampusesSelected_UserId_RoleId_CampusId",
                table: "UserSessionRoleCampusesSelected",
                columns: ["UserId", "RoleId", "CampusId"]);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionRoleCampusesSelected_UserSessionWorkProfileSelectedId_IsActive",
                table: "UserSessionRoleCampusesSelected",
                columns: ["UserSessionWorkProfileSelectedId", "IsActive"]);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_AbsoluteExpiresAt",
                table: "UserSessions",
                column: "AbsoluteExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ClosedByUserId",
                table: "UserSessions",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ExpiresAt",
                table: "UserSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserDeviceId",
                table: "UserSessions",
                column: "UserDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserDeviceId_IsRevoked",
                table: "UserSessions",
                columns: ["UserDeviceId", "IsRevoked"]);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId_IsRevoked",
                table: "UserSessions",
                columns: ["UserId", "IsRevoked"]);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionWorkProfilesSelected_UserId_WorkProfileId",
                table: "UserSessionWorkProfilesSelected",
                columns: ["UserId", "WorkProfileId"]);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionWorkProfilesSelected_UserSessionId_IsActive",
                table: "UserSessionWorkProfilesSelected",
                columns: ["UserSessionId", "IsActive"]);

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkProfiles_WorkProfileId",
                table: "UserWorkProfiles",
                column: "WorkProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkProfilePermissions_PermissionId",
                table: "WorkProfilePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkProfiles_Name",
                table: "WorkProfiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkProfiles_WorkProfileType",
                table: "WorkProfiles",
                column: "WorkProfileType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlacklistedAccessTokensSession");

            migrationBuilder.DropTable(
                name: "BlacklistedAccessTokensTemporary");

            migrationBuilder.DropTable(
                name: "BlacklistedRefreshTokens");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserPhotos");

            migrationBuilder.DropTable(
                name: "UserSessionRoleCampusesSelected");

            migrationBuilder.DropTable(
                name: "WorkProfilePermissions");

            migrationBuilder.DropTable(
                name: "UserSessionRefreshTokens");

            migrationBuilder.DropTable(
                name: "UserRoleCampuses");

            migrationBuilder.DropTable(
                name: "UserSessionWorkProfilesSelected");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Campuses");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "UserWorkProfiles");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "BusinessUnits");

            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.DropTable(
                name: "WorkProfiles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}