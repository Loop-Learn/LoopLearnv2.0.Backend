using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoopLearn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCorrectFeatureToStudentAnswersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "StudentAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "StudentAnswers");
        }
    }
}
