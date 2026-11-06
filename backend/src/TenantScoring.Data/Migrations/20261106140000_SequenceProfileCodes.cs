using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TenantScoring.Data;

#nullable disable

namespace TenantScoring.Data.Migrations;

[DbContext(typeof(TenantDbContext))]
[Migration("20261106140000_SequenceProfileCodes")]
public partial class SequenceProfileCodes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSequence<long>(
            name: "ProfileCodeSequence",
            schema: "dbo",
            startValue: 1_000_000_000L);

        migrationBuilder.Sql(";WITH [NumberedProfiles] AS (SELECT [ProfileId], ROW_NUMBER() OVER (ORDER BY [ProfileId]) AS [SequenceNumber] FROM [UserProfiles]) UPDATE [UserProfiles] SET [ProfileCode] = N'KID' + RIGHT(N'0000000000' + CONVERT(nvarchar(10), [NumberedProfiles].[SequenceNumber]), 10) FROM [UserProfiles] INNER JOIN [NumberedProfiles] ON [UserProfiles].[ProfileId] = [NumberedProfiles].[ProfileId]");

        migrationBuilder.DropIndex(
            name: "IX_UserProfiles_ProfileCode",
            table: "UserProfiles");

        migrationBuilder.AlterColumn<string>(
            name: "ProfileCode",
            table: "UserProfiles",
            type: "nvarchar(13)",
            maxLength: 13,
            nullable: false,
            defaultValueSql: "N'KID' + RIGHT('0000000000' + CONVERT(varchar(10), NEXT VALUE FOR dbo.ProfileCodeSequence), 10)",
            oldClrType: typeof(string),
            oldType: "nvarchar(35)",
            oldMaxLength: 35);

        migrationBuilder.CreateIndex(
            name: "IX_UserProfiles_ProfileCode",
            table: "UserProfiles",
            column: "ProfileCode",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_UserProfiles_ProfileCode",
            table: "UserProfiles");

        migrationBuilder.Sql("UPDATE [UserProfiles] SET [ProfileCode] = N'KID' + UPPER(REPLACE(CONVERT(nvarchar(36), [ProfileId]), N'-', N''))");

        migrationBuilder.AlterColumn<string>(
            name: "ProfileCode",
            table: "UserProfiles",
            type: "nvarchar(35)",
            maxLength: 35,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(13)",
            oldMaxLength: 13,
            oldDefaultValueSql: "N'KID' + RIGHT('0000000000' + CONVERT(varchar(10), NEXT VALUE FOR dbo.ProfileCodeSequence), 10)");

        migrationBuilder.CreateIndex(
            name: "IX_UserProfiles_ProfileCode",
            table: "UserProfiles",
            column: "ProfileCode",
            unique: true);

        migrationBuilder.DropSequence(name: "ProfileCodeSequence", schema: "dbo");
    }
}