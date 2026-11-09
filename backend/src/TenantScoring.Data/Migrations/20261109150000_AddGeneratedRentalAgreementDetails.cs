using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenantScoring.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedRentalAgreementDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgreementOwnerAddress",
                table: "Leases",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AgreementSigningDate",
                table: "Leases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgreementUnitNumber",
                table: "Leases",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgreementOwnerAddress",
                table: "Leases");

            migrationBuilder.DropColumn(
                name: "AgreementSigningDate",
                table: "Leases");

            migrationBuilder.DropColumn(
                name: "AgreementUnitNumber",
                table: "Leases");
        }
    }
}
