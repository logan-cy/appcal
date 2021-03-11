using Microsoft.EntityFrameworkCore.Migrations;

namespace ac.api.Migrations
{
    public partial class AddedAllDayToEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllDay",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllDay",
                table: "Events");
        }
    }
}
