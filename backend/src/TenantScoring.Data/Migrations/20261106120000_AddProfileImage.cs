using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TenantScoring.Data;

#nullable disable

namespace TenantScoring.Data.Migrations;

[DbContext(typeof(TenantDbContext))]
[Migration("20261106120000_AddProfileImage")]
public partial class AddProfileImage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ProfileImageFileName",
            table: "UserProfiles",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ProfileImageFileName",
            table: "UserProfiles");
    }
}