using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UCMS.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationFieldToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EducationLevel",
                table: "Students",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationLevel",
                table: "Students");
        }
    }
}
