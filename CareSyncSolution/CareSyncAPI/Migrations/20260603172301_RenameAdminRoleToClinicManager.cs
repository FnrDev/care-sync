using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSyncAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameAdminRoleToClinicManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the existing "Admin" role to "ClinicManager" in place so the
            // role's Id is preserved and the manager user's AspNetUserRoles link
            // stays intact. The MVC app authorizes the clinic manager area by the
            // "ClinicManager" role name.
            migrationBuilder.Sql(@"
                UPDATE [AspNetRoles]
                SET [Name] = 'ClinicManager', [NormalizedName] = 'CLINICMANAGER'
                WHERE [NormalizedName] = 'ADMIN';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [AspNetRoles]
                SET [Name] = 'Admin', [NormalizedName] = 'ADMIN'
                WHERE [NormalizedName] = 'CLINICMANAGER';
            ");
        }
    }
}
