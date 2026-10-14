using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIRecruitmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateInterviewResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CandidateRespondedAt",
                schema: "dbo",
                table: "AIRecruitmentSystemInterviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CandidateResponse",
                schema: "dbo",
                table: "AIRecruitmentSystemInterviews",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CandidateRespondedAt",
                schema: "dbo",
                table: "AIRecruitmentSystemInterviews");

            migrationBuilder.DropColumn(
                name: "CandidateResponse",
                schema: "dbo",
                table: "AIRecruitmentSystemInterviews");
        }
    }
}
