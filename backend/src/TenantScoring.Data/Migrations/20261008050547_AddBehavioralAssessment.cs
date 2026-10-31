using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenantScoring.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBehavioralAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>("BehavioralAssessmentCompleted", "UserProfiles", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<decimal>("BehavioralScore", "UserProfiles", type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m);
            migrationBuilder.CreateTable(
                name: "BehavioralQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PreferredAnswer = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    WeightPoints = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_BehavioralQuestions", x => x.QuestionId));
            migrationBuilder.CreateTable(
                name: "TenantBehavioralResponses",
                columns: table => new
                {
                    ResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserAnswer = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PointsEarned = table.Column<int>(type: "int", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantBehavioralResponses", x => x.ResponseId);
                    table.ForeignKey("FK_TenantBehavioralResponses_BehavioralQuestions_QuestionId", x => x.QuestionId, "BehavioralQuestions", "QuestionId", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_TenantBehavioralResponses_Users_UserId", x => x.UserId, "Users", "UserId", onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex("IX_TenantBehavioralResponses_QuestionId", "TenantBehavioralResponses", "QuestionId");
            migrationBuilder.CreateIndex("IX_TenantBehavioralResponses_UserId_QuestionId", "TenantBehavioralResponses", new[] { "UserId", "QuestionId" }, unique: true);
            migrationBuilder.InsertData("BehavioralQuestions", new[] { "QuestionId", "Category", "QuestionText", "PreferredAnswer", "WeightPoints", "IsActive" }, new object[,]
            {
                { new Guid("11111111-1111-1111-1111-111111111111"), "Eviction & Disputes", "Have you ever been served an eviction notice or had a legal dispute with a landlord?", "No", 33, true },
                { new Guid("22222222-2222-2222-2222-222222222222"), "Subletting Intent", "Do you plan to sublet any part of the rented property to third parties?", "No", 33, true },
                { new Guid("33333333-3333-3333-3333-333333333333"), "Smoking Policy", "Do you smoke or use tobacco products inside residential premises?", "No", 34, true }
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("TenantBehavioralResponses");
            migrationBuilder.DropTable("BehavioralQuestions");
            migrationBuilder.DropColumn("BehavioralAssessmentCompleted", "UserProfiles");
            migrationBuilder.DropColumn("BehavioralScore", "UserProfiles");
        }
    }
}
