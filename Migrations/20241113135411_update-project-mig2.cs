using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class updateprojectmig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_QUALIFICATION_NAME",
                table: "Projects");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QUALIFICATION_NAME",
                table: "Projects",
                sql: "[QualificationName] IN ('backend', 'frontend', 'mobile', 'uiux', 'fullstack')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_QUALIFICATION_NAME",
                table: "Projects");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QUALIFICATION_NAME",
                table: "Projects",
                sql: "[QualificationName] IN ('Back-end', 'Front-end', 'Mobile', 'UIUX')");
        }
    }
}
