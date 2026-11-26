using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tis.Persistence.Contexts.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StudentNumber",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_StudentNumber",
                table: "Users",
                columns: new[] { "TenantId", "StudentNumber" },
                unique: true,
                filter: "[StudentNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_StudentNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudentNumber",
                table: "Users");
        }
    }
}
