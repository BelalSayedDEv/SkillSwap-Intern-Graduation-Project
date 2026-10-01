using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MatchesDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SwapRequests_ReceiverId",
                table: "SwapRequests");

            migrationBuilder.DropIndex(
                name: "IX_SwapRequests_SenderId",
                table: "SwapRequests");

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_ReceiverId_Status",
                table: "SwapRequests",
                columns: new[] { "ReceiverId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_SenderId_Status",
                table: "SwapRequests",
                columns: new[] { "SenderId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SwapRequests_ReceiverId_Status",
                table: "SwapRequests");

            migrationBuilder.DropIndex(
                name: "IX_SwapRequests_SenderId_Status",
                table: "SwapRequests");

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_ReceiverId",
                table: "SwapRequests",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_SenderId",
                table: "SwapRequests",
                column: "SenderId");
        }
    }
}
