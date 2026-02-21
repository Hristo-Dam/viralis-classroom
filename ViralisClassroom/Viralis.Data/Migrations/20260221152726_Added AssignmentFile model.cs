using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viralis.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedAssignmentFilemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionFile_Submissions_SubmissionId",
                table: "SubmissionFile");

            migrationBuilder.CreateTable(
                name: "AssignmentFile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentFile_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentFile_AssignmentId",
                table: "AssignmentFile",
                column: "AssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionFile_Submissions_SubmissionId",
                table: "SubmissionFile",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionFile_Submissions_SubmissionId",
                table: "SubmissionFile");

            migrationBuilder.DropTable(
                name: "AssignmentFile");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionFile_Submissions_SubmissionId",
                table: "SubmissionFile",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
