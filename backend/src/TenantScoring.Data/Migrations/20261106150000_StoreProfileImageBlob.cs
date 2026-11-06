using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TenantScoring.Data;

#nullable disable

namespace TenantScoring.Data.Migrations;

[DbContext(typeof(TenantDbContext))]
[Migration("20261106150000_StoreProfileImageBlob")]
public partial class StoreProfileImageBlob : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ProfileImageFileName",
            table: "UserProfiles");

        migrationBuilder.AddColumn<byte[]>(
            name: "ProfileImageData",
            table: "UserProfiles",
            type: "varbinary(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ProfileImageContentType",
            table: "UserProfiles",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("ProfileImageData", "UserProfiles");
        migrationBuilder.DropColumn("ProfileImageContentType", "UserProfiles");

        migrationBuilder.AddColumn<string>(
            name: "ProfileImageFileName",
            table: "UserProfiles",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }
}