using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viralis.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedSchoolAdministratormodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SchoolAdminId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SchoolAdministrators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchoolAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolAdministrators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolAdministrators_AspNetUsers_SchoolAdminId",
                        column: x => x.SchoolAdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SchoolAdminId",
                table: "AspNetUsers",
                column: "SchoolAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAdministrators_SchoolAdminId",
                table: "SchoolAdministrators",
                column: "SchoolAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SchoolAdministrators_SchoolAdminId",
                table: "AspNetUsers",
                column: "SchoolAdminId",
                principalTable: "SchoolAdministrators",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SchoolAdministrators_SchoolAdminId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "SchoolAdministrators");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SchoolAdminId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SchoolAdminId",
                table: "AspNetUsers");
        }
    }
}
