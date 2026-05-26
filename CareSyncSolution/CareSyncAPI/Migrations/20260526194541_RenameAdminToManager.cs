using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSyncAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameAdminToManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove the old admin@caresync.local user so DbSeeder recreates it as
            // manager@caresync.local with the new Manager@123 password on next startup.
            // Cascade removes the matching AspNetUserRoles row.
            migrationBuilder.Sql(@"
                DELETE FROM [AspNetUsers]
                WHERE [NormalizedEmail] = 'ADMIN@CARESYNC.LOCAL';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: the deleted user is restored by DbSeeder on next API startup
            // (under the new manager@caresync.local identity).
        }
    }
}
