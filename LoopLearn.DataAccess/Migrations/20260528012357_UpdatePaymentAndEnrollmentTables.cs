using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoopLearn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentAndEnrollmentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentId",
                table: "Enrollments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_PaymentId",
                table: "Enrollments",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Payments_PaymentId",
                table: "Enrollments",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Payments_PaymentId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_PaymentId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Enrollments");
        }
    }
}
