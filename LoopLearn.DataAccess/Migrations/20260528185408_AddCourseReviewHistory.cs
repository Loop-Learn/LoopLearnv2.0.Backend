using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoopLearn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseReviewHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedForReviewAt",
                table: "Courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseReviewHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseReviewHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseReviewHistories_AspNetUsers_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseReviewHistories_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseReviewHistories_CourseId",
                table: "CourseReviewHistories",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseReviewHistories_PerformedById",
                table: "CourseReviewHistories",
                column: "PerformedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseReviewHistories");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SubmittedForReviewAt",
                table: "Courses");
        }
    }
}
