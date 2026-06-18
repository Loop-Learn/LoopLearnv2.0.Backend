using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoopLearn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFeatureToApplicationUser_IsInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInstructorRequested",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInstructorRequested",
                table: "AspNetUsers");
        }
    }
}
