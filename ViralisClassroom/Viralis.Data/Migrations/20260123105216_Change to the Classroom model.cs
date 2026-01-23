using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viralis.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangetotheClassroommodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_AspNetUsers_OwnerTeacherId",
                table: "Classrooms");

            migrationBuilder.DropIndex(
                name: "IX_Classrooms_OwnerTeacherId",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "OwnerTeacherId",
                table: "Classrooms");

            migrationBuilder.AddColumn<bool>(
                name: "IsOwner",
                table: "ClassroomTeachers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOwner",
                table: "ClassroomTeachers");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerTeacherId",
                table: "Classrooms",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_OwnerTeacherId",
                table: "Classrooms",
                column: "OwnerTeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_AspNetUsers_OwnerTeacherId",
                table: "Classrooms",
                column: "OwnerTeacherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
