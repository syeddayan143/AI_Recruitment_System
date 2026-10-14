using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIRecruitmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialAIRecruitmentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemRoles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemUsers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemRoleClaims",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemRoleClaims_AIRecruitmentSystemRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemCandidateProfiles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CurrentJobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HighestEducation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false),
                    PreferredLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Skills = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PortfolioUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemCandidateProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemCandidateProfiles_AIRecruitmentSystemUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemJobs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Requirements = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MinimumSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaximumSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EmploymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecruiterId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemJobs_AIRecruitmentSystemUsers_RecruiterId",
                        column: x => x.RecruiterId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemResumes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ExtractedText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetectedSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResumeScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemResumes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemResumes_AIRecruitmentSystemUsers_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemUserClaims",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemUserClaims_AIRecruitmentSystemUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemUserLogins",
                schema: "dbo",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemUserLogins_AIRecruitmentSystemUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemUserRoles",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemUserRoles_AIRecruitmentSystemRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemUserRoles_AIRecruitmentSystemUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemUserTokens",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemUserTokens_AIRecruitmentSystemUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemJobApplications",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    ResumeId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatchScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    RecruiterNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemJobApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemJobApplications_AIRecruitmentSystemJobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemJobApplications_AIRecruitmentSystemResumes_ResumeId",
                        column: x => x.ResumeId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemResumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemJobApplications_AIRecruitmentSystemUsers_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AIRecruitmentSystemInterviews",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobApplicationId = table.Column<int>(type: "int", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InterviewType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MeetingLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InterviewScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecruitmentSystemInterviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIRecruitmentSystemInterviews_AIRecruitmentSystemJobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalSchema: "dbo",
                        principalTable: "AIRecruitmentSystemJobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemCandidateProfiles_UserId",
                schema: "dbo",
                table: "AIRecruitmentSystemCandidateProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemInterviews_JobApplicationId",
                schema: "dbo",
                table: "AIRecruitmentSystemInterviews",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemJobApplications_CandidateId_JobId",
                schema: "dbo",
                table: "AIRecruitmentSystemJobApplications",
                columns: new[] { "CandidateId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemJobApplications_JobId",
                schema: "dbo",
                table: "AIRecruitmentSystemJobApplications",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemJobApplications_ResumeId",
                schema: "dbo",
                table: "AIRecruitmentSystemJobApplications",
                column: "ResumeId");

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemJobs_RecruiterId",
                schema: "dbo",
                table: "AIRecruitmentSystemJobs",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemResumes_CandidateId",
                schema: "dbo",
                table: "AIRecruitmentSystemResumes",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemRoleClaims_RoleId",
                schema: "dbo",
                table: "AIRecruitmentSystemRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "dbo",
                table: "AIRecruitmentSystemRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemUserClaims_UserId",
                schema: "dbo",
                table: "AIRecruitmentSystemUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemUserLogins_UserId",
                schema: "dbo",
                table: "AIRecruitmentSystemUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIRecruitmentSystemUserRoles_RoleId",
                schema: "dbo",
                table: "AIRecruitmentSystemUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "dbo",
                table: "AIRecruitmentSystemUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "dbo",
                table: "AIRecruitmentSystemUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemCandidateProfiles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemInterviews",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemRoleClaims",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemUserClaims",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemUserLogins",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemUserRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemUserTokens",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemJobApplications",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemJobs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemResumes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AIRecruitmentSystemUsers",
                schema: "dbo");
        }
    }
}
