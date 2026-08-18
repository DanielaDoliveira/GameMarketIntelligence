using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketIntel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContextualGameReleases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_releases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_game_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    release_date_kind = table.Column<string>(type: "text", nullable: false),
                    release_year = table.Column<int>(type: "integer", nullable: true),
                    release_month = table.Column<int>(type: "integer", nullable: true),
                    release_day = table.Column<int>(type: "integer", nullable: true),
                    release_quarter = table.Column<int>(type: "integer", nullable: true),
                    region_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    ecosystem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    external_release_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_releases", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_releases_Games_game_id",
                        column: x => x.game_id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_releases_Platforms_platform_id",
                        column: x => x.platform_id,
                        principalTable: "Platforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_releases_external_game_records_external_game_record_id",
                        column: x => x.external_game_record_id,
                        principalTable: "external_game_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_game_releases_game_id",
                table: "game_releases",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_releases_platform_id",
                table: "game_releases",
                column: "platform_id");

            migrationBuilder.CreateIndex(
                name: "ux_game_releases_external_record_external_release_id",
                table: "game_releases",
                columns: new[] { "external_game_record_id", "external_release_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_releases");
        }
    }
}
