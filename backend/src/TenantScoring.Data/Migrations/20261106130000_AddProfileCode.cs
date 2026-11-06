using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TenantScoring.Data;

#nullable disable

namespace TenantScoring.Data.Migrations;

[DbContext(typeof(TenantDbContext))]
[Migration("20261106130000_AddProfileCode")]
public partial class AddProfileCode : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ProfileCode",
            table: "UserProfiles",
            type: "nvarchar(35)",
            maxLength: 35,
            nullable: true);

        migrationBuilder.Sql("UPDATE [UserProfiles] SET [ProfileCode] = N'KID' + UPPER(REPLACE(CONVERT(nvarchar(36), [ProfileId]), N'-', N''))");

        migrationBuilder.AlterColumn<string>(
            name: "ProfileCode",
            table: "UserProfiles",
            type: "nvarchar(35)",
            maxLength: 35,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(35)",
            oldMaxLength: 35,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserProfiles_ProfileCode",
            table: "UserProfiles",
            column: "ProfileCode",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_UserProfiles_ProfileCode", "UserProfiles");
        migrationBuilder.DropColumn("ProfileCode", "UserProfiles");
    }
}