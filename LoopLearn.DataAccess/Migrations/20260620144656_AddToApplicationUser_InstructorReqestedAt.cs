using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoopLearn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddToApplicationUser_InstructorReqestedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InstructorRequestedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstructorRequestedAt",
                table: "AspNetUsers");
        }
    }
}
