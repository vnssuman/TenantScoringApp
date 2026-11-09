using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TenantScoring.Data;

#nullable disable

namespace TenantScoring.Data.Migrations;

[DbContext(typeof(TenantDbContext))]
[Migration("20261109130000_AddTenantRequestComments")]
public partial class AddTenantRequestComments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TenantComments",
            table: "Leases",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TenantComments",
            table: "Leases");
    }
}
