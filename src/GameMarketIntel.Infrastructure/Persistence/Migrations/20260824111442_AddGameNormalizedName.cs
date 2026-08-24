using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketIntel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGameNormalizedName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Games",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Games"
                SET "NormalizedName" = UPPER(TRIM("Name"));
                """);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "Games",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_NormalizedName",
                table: "Games",
                column: "NormalizedName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Games_NormalizedName",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Games");
        }
    }
}