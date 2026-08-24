using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketIntel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGameProductType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductType",
                table: "Games",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "Games");
        }
    }
}
