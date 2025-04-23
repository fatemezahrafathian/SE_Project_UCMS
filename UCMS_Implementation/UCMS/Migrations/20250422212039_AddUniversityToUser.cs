using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UCMS.Migrations
{
    /// <inheritdoc />
    public partial class AddUniversityToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "University",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "Instructors",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "University",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "Instructors");
        }
    }
}
