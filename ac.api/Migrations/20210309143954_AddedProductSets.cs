using Microsoft.EntityFrameworkCore.Migrations;

namespace ac.api.Migrations
{
    public partial class AddedProductSets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductSetId",
                table: "Products",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DivisionId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSets_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductSetId",
                table: "Products",
                column: "ProductSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSets_DivisionId",
                table: "ProductSets",
                column: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductSets_ProductSetId",
                table: "Products",
                column: "ProductSetId",
                principalTable: "ProductSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductSets_ProductSetId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductSets");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductSetId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductSetId",
                table: "Products");
        }
    }
}
