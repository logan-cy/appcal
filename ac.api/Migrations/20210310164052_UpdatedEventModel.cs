using Microsoft.EntityFrameworkCore.Migrations;

namespace ac.api.Migrations
{
    public partial class UpdatedEventModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ClientId",
                table: "Events",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CompanyId",
                table: "Events",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Clients_ClientId",
                table: "Events",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Companies_CompanyId",
                table: "Events",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Clients_ClientId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Companies_CompanyId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ClientId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_CompanyId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Events");
        }
    }
}
