using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenantScoring.Data.Migrations
{
    public partial class AddRentalFeedback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feedback_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    question_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_feedback_questions", question => question.id));

            migrationBuilder.CreateTable(
                name: "feedback_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    lease_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    property_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    evaluator_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    subject_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    feedback_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    overall_score = table.Column<double>(type: "float", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_sessions", session => session.id);
                    table.ForeignKey("FK_feedback_sessions_Leases_lease_id", session => session.lease_id, "Leases", "LeaseId", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_feedback_sessions_Properties_property_id", session => session.property_id, "Properties", "PropertyId", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_feedback_sessions_Users_evaluator_id", session => session.evaluator_id, "Users", "UserId", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_feedback_sessions_Users_subject_id", session => session.subject_id, "Users", "UserId", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feedback_responses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_responses", response => response.id);
                    table.CheckConstraint("CK_feedback_responses_Rating", "[rating] >= 1 AND [rating] <= 5");
                    table.ForeignKey("FK_feedback_responses_feedback_questions_question_id", response => response.question_id, "feedback_questions", "id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_feedback_responses_feedback_sessions_session_id", response => response.session_id, "feedback_sessions", "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "feedback_questions",
                columns: new[] { "id", "category", "created_at", "display_order", "question_text" },
                values: new object[,]
                {
                    { new Guid("a1000001-0000-0000-0000-000000000001"), "TENANT_TO_OWNER", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1, "Responsiveness to Maintenance: How quickly and effectively did the owner address maintenance requests?" },
                    { new Guid("a1000001-0000-0000-0000-000000000002"), "TENANT_TO_OWNER", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 2, "Clarity of Communication: How clear, transparent, and professional was the owner in their communication?" },
                    { new Guid("a1000001-0000-0000-0000-000000000003"), "TENANT_TO_OWNER", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 3, "Property Condition at Move-In: How well did the initial property condition match what was advertised?" },
                    { new Guid("a1000001-0000-0000-0000-000000000004"), "TENANT_TO_OWNER", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 4, "Respect for Privacy: How well did the owner respect your privacy and provide adequate notice before entering?" },
                    { new Guid("a1000001-0000-0000-0000-000000000005"), "TENANT_TO_OWNER", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 5, "Fairness in Financial Dealings: How fair and transparent was the owner regarding deposit returns and fees?" },
                    { new Guid("a1000001-0000-0000-0000-000000000006"), "TENANT_TO_OWNER", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 6, "General Habitability & Upkeep: How well-maintained are the building's common areas and shared amenities?" },
                    { new Guid("a1000001-0000-0000-0000-000000000007"), "TENANT_TO_OWNER", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 7, "Problem-Solving & Conflict Resolution: How cooperative was the owner in resolving issues or disagreements?" },
                    { new Guid("a1000001-0000-0000-0000-000000000008"), "TENANT_TO_OWNER", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 8, "Lease Renewal & Flexibility: How reasonable was the owner regarding lease terms and moving out?" },
                    { new Guid("a1000001-0000-0000-0000-000000000009"), "TENANT_TO_OWNER", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 9, "Overall Value for Money: How satisfied were you with the overall quality of living relative to rent paid?" },
                    { new Guid("a1000001-0000-0000-0000-000000000010"), "TENANT_TO_OWNER", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10, "Likelihood of Re-renting: How likely are you to rent from this owner again or recommend them?" },
                    { new Guid("a2000001-0000-0000-0000-000000000001"), "OWNER_TO_TENANT", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1, "Timeliness of Rent Payments: How consistently did the tenant pay rent on or before the due date?" },
                    { new Guid("a2000001-0000-0000-0000-000000000002"), "OWNER_TO_TENANT", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 2, "Care of Property & Cleanliness: How well did the tenant maintain the interior condition of the property?" },
                    { new Guid("a2000001-0000-0000-0000-000000000003"), "OWNER_TO_TENANT", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 3, "Communication & Responsiveness: How responsive was the tenant regarding notices and administrative matters?" },
                    { new Guid("a2000001-0000-0000-0000-000000000004"), "OWNER_TO_TENANT", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 4, "Adherence to Lease Rules: How well did the tenant follow building rules, pet policies, and lease terms?" },
                    { new Guid("a2000001-0000-0000-0000-000000000005"), "OWNER_TO_TENANT", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 5, "Prompt Reporting of Issues: Did the tenant report maintenance issues early before they escalated?" },
                    { new Guid("a2000001-0000-0000-0000-000000000006"), "OWNER_TO_TENANT", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 6, "Neighbor Relations & Conduct: How respectful was the tenant toward neighbors and noise levels?" },
                    { new Guid("a2000001-0000-0000-0000-000000000007"), "OWNER_TO_TENANT", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 7, "Handling of Move-Out / Handover: How clean and undamaged was the property returned at move-out?" },
                    { new Guid("a2000001-0000-0000-0000-000000000008"), "OWNER_TO_TENANT", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 8, "Self-Sufficiency / Minor Upkeep: How capable was the tenant of handling basic, day-to-day minor tasks?" },
                    { new Guid("a2000001-0000-0000-0000-000000000009"), "OWNER_TO_TENANT", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 9, "Trustworthiness & Honesty: How reliable and honest was the tenant in all interactions?" },
                    { new Guid("a2000001-0000-0000-0000-000000000010"), "OWNER_TO_TENANT", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10, "Likelihood of Renting Again: How likely would you be to rent to this tenant again in the future?" }
                });

            migrationBuilder.CreateIndex("IX_feedback_questions_category_display_order", "feedback_questions", new[] { "category", "display_order" }, unique: true);
            migrationBuilder.CreateIndex("IX_feedback_responses_question_id", "feedback_responses", "question_id");
            migrationBuilder.CreateIndex("IX_feedback_responses_session_id_question_id", "feedback_responses", new[] { "session_id", "question_id" }, unique: true);
            migrationBuilder.CreateIndex("IX_feedback_sessions_evaluator_id", "feedback_sessions", "evaluator_id");
            migrationBuilder.CreateIndex("IX_feedback_sessions_lease_id_feedback_type", "feedback_sessions", new[] { "lease_id", "feedback_type" }, unique: true);
            migrationBuilder.CreateIndex("IX_feedback_sessions_property_id", "feedback_sessions", "property_id");
            migrationBuilder.CreateIndex("IX_feedback_sessions_subject_id", "feedback_sessions", "subject_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "feedback_responses");
            migrationBuilder.DropTable(name: "feedback_questions");
            migrationBuilder.DropTable(name: "feedback_sessions");
        }
    }
}
