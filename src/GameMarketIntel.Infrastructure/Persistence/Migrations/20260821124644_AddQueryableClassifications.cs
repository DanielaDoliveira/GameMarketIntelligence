using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketIntel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryableClassifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_modes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_modes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "keywords",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_keywords", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "player_perspectives",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_perspectives", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "themes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_themes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_game_mode_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    game_mode_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    source_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_game_mode_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_game_mode_records_data_sources_data_source_id",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_external_game_mode_records_game_modes_game_mode_id",
                        column: x => x.game_mode_id,
                        principalTable: "game_modes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_keyword_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    keyword_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    source_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_keyword_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_keyword_records_data_sources_data_source_id",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_external_keyword_records_keywords_keyword_id",
                        column: x => x.keyword_id,
                        principalTable: "keywords",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_player_perspective_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    player_perspective_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    source_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_player_perspective_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_player_perspective_records_data_sources_data_sourc~",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_external_player_perspective_records_player_perspectives_pla~",
                        column: x => x.player_perspective_id,
                        principalTable: "player_perspectives",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_theme_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    theme_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    source_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_theme_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_theme_records_data_sources_data_source_id",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_external_theme_records_themes_theme_id",
                        column: x => x.theme_id,
                        principalTable: "themes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "game_game_modes",
                columns: table => new
                {
                    external_game_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_game_mode_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_mode_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_game_modes", x => new { x.external_game_record_id, x.external_game_mode_record_id });
                    table.ForeignKey(
                        name: "FK_game_game_modes_Games_game_id",
                        column: x => x.game_id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_game_modes_external_game_mode_records_external_game_mo~",
                        column: x => x.external_game_mode_record_id,
                        principalTable: "external_game_mode_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_game_modes_external_game_records_external_game_record_~",
                        column: x => x.external_game_record_id,
                        principalTable: "external_game_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_game_modes_game_modes_game_mode_id",
                        column: x => x.game_mode_id,
                        principalTable: "game_modes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "game_keywords",
                columns: table => new
                {
                    external_game_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_keyword_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    keyword_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_keywords", x => new { x.external_game_record_id, x.external_keyword_record_id });
                    table.ForeignKey(
                        name: "FK_game_keywords_Games_game_id",
                        column: x => x.game_id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_keywords_external_game_records_external_game_record_id",
                        column: x => x.external_game_record_id,
                        principalTable: "external_game_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_keywords_external_keyword_records_external_keyword_rec~",
                        column: x => x.external_keyword_record_id,
                        principalTable: "external_keyword_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_keywords_keywords_keyword_id",
                        column: x => x.keyword_id,
                        principalTable: "keywords",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "game_player_perspectives",
                columns: table => new
                {
                    external_game_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_player_perspective_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_perspective_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_player_perspectives", x => new { x.external_game_record_id, x.external_player_perspective_record_id });
                    table.ForeignKey(
                        name: "FK_game_player_perspectives_Games_game_id",
                        column: x => x.game_id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_player_perspectives_external_game_records_external_gam~",
                        column: x => x.external_game_record_id,
                        principalTable: "external_game_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_player_perspectives_external_player_perspective_record~",
                        column: x => x.external_player_perspective_record_id,
                        principalTable: "external_player_perspective_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_player_perspectives_player_perspectives_player_perspec~",
                        column: x => x.player_perspective_id,
                        principalTable: "player_perspectives",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "game_themes",
                columns: table => new
                {
                    external_game_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_theme_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    theme_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_themes", x => new { x.external_game_record_id, x.external_theme_record_id });
                    table.ForeignKey(
                        name: "FK_game_themes_Games_game_id",
                        column: x => x.game_id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_themes_external_game_records_external_game_record_id",
                        column: x => x.external_game_record_id,
                        principalTable: "external_game_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_themes_external_theme_records_external_theme_record_id",
                        column: x => x.external_theme_record_id,
                        principalTable: "external_theme_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_themes_themes_theme_id",
                        column: x => x.theme_id,
                        principalTable: "themes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_external_game_mode_records_game_mode_id",
                table: "external_game_mode_records",
                column: "game_mode_id");

            migrationBuilder.CreateIndex(
                name: "ux_external_game_mode_records_source_external_id",
                table: "external_game_mode_records",
                columns: new[] { "data_source_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_external_keyword_records_keyword_id",
                table: "external_keyword_records",
                column: "keyword_id");

            migrationBuilder.CreateIndex(
                name: "ux_external_keyword_records_source_external_id",
                table: "external_keyword_records",
                columns: new[] { "data_source_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_external_player_perspective_records_player_perspective_id",
                table: "external_player_perspective_records",
                column: "player_perspective_id");

            migrationBuilder.CreateIndex(
                name: "ux_external_player_perspective_records_source_external_id",
                table: "external_player_perspective_records",
                columns: new[] { "data_source_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_external_theme_records_theme_id",
                table: "external_theme_records",
                column: "theme_id");

            migrationBuilder.CreateIndex(
                name: "ux_external_theme_records_source_external_id",
                table: "external_theme_records",
                columns: new[] { "data_source_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_game_modes_external_game_mode_record_id",
                table: "game_game_modes",
                column: "external_game_mode_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_game_modes_game_id",
                table: "game_game_modes",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_game_modes_game_mode_id",
                table: "game_game_modes",
                column: "game_mode_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_keywords_external_keyword_record_id",
                table: "game_keywords",
                column: "external_keyword_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_keywords_game_id",
                table: "game_keywords",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_keywords_keyword_id",
                table: "game_keywords",
                column: "keyword_id");

            migrationBuilder.CreateIndex(
                name: "ux_game_modes_normalized_name",
                table: "game_modes",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_player_perspectives_external_player_perspective_record~",
                table: "game_player_perspectives",
                column: "external_player_perspective_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_player_perspectives_game_id",
                table: "game_player_perspectives",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_player_perspectives_player_perspective_id",
                table: "game_player_perspectives",
                column: "player_perspective_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_themes_external_theme_record_id",
                table: "game_themes",
                column: "external_theme_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_themes_game_id",
                table: "game_themes",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_themes_theme_id",
                table: "game_themes",
                column: "theme_id");

            migrationBuilder.CreateIndex(
                name: "ux_keywords_normalized_name",
                table: "keywords",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_player_perspectives_normalized_name",
                table: "player_perspectives",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_themes_normalized_name",
                table: "themes",
                column: "normalized_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_game_modes");

            migrationBuilder.DropTable(
                name: "game_keywords");

            migrationBuilder.DropTable(
                name: "game_player_perspectives");

            migrationBuilder.DropTable(
                name: "game_themes");

            migrationBuilder.DropTable(
                name: "external_game_mode_records");

            migrationBuilder.DropTable(
                name: "external_keyword_records");

            migrationBuilder.DropTable(
                name: "external_player_perspective_records");

            migrationBuilder.DropTable(
                name: "external_theme_records");

            migrationBuilder.DropTable(
                name: "game_modes");

            migrationBuilder.DropTable(
                name: "keywords");

            migrationBuilder.DropTable(
                name: "player_perspectives");

            migrationBuilder.DropTable(
                name: "themes");
        }
    }
}
