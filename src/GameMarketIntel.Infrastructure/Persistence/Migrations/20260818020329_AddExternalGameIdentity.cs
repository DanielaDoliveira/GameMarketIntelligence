using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketIntel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalGameIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "data_sources",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE data_sources
                SET code = CASE
                    WHEN lower(trim(name)) = 'igdb'
                        THEN 'igdb'
                    WHEN lower(trim(name)) IN ('steam', 'steam web api')
                        THEN 'steam'
                    WHEN lower(trim(name)) IN ('wikidata', 'wikidata api')
                        THEN 'wikidata'
                    ELSE 'legacy-' || replace(id::text, '-', '')
                END
                WHERE code IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "data_sources",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
            
            migrationBuilder.CreateTable(
                name: "external_game_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    source_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_game_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_game_records_Games_game_id",
                        column: x => x.game_id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_external_game_records_data_sources_data_source_id",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_data_sources_code",
                table: "data_sources",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_game_records_game_id",
                table: "external_game_records",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ux_external_game_records_source_external_id",
                table: "external_game_records",
                columns: new[] { "data_source_id", "external_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "external_game_records");

            migrationBuilder.DropIndex(
                name: "ux_data_sources_code",
                table: "data_sources");

            migrationBuilder.DropColumn(
                name: "code",
                table: "data_sources");
        }
    }
}
