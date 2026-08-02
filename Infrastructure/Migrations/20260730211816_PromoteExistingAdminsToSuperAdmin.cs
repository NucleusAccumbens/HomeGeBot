using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PromoteExistingAdminsToSuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Admins\" SET \"Role\" = 1 WHERE \"IsActive\" = true AND \"Role\" = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Admins\" SET \"Role\" = 0 WHERE \"IsActive\" = true AND \"Role\" = 1;");
        }
    }
}
