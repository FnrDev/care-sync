using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSyncAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameClinicManagerRoleToManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the clinic manager role from "ClinicManager" to "Manager" in
            // place so the role Id (and the manager user's AspNetUserRoles link)
            // is preserved. The app authorizes the manager by the "Manager" role.
            migrationBuilder.Sql(@"
                UPDATE [AspNetRoles]
                SET [Name] = 'Manager', [NormalizedName] = 'MANAGER'
                WHERE [NormalizedName] = 'CLINICMANAGER';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [AspNetRoles]
                SET [Name] = 'ClinicManager', [NormalizedName] = 'CLINICMANAGER'
                WHERE [NormalizedName] = 'MANAGER';
            ");
        }
    }
}
