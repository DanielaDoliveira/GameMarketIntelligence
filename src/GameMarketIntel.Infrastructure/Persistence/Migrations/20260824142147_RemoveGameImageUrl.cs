using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketIntel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGameImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Games");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Games",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}
