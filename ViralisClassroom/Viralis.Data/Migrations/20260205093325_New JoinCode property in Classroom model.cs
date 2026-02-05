using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viralis.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewJoinCodepropertyinClassroommodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JoinCode",
                table: "Classrooms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoinCode",
                table: "Classrooms");
        }
    }
}
