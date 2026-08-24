using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketIntel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionsAndGameCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "collections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_collection_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    collection_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    source_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_collection_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_collection_records_collections_collection_id",
                        column: x => x.collection_id,
                        principalTable: "collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_external_collection_records_data_sources_data_source_id",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "game_collections",
                columns: table => new
                {
                    external_game_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_collection_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    collection_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_collections", x => new { x.external_game_record_id, x.external_collection_record_id });
                    table.ForeignKey(
                        name: "FK_game_collections_Games_game_id",
                        column: x => x.game_id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_collections_collections_collection_id",
                        column: x => x.collection_id,
                        principalTable: "collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_collections_external_collection_records_external_colle~",
                        column: x => x.external_collection_record_id,
                        principalTable: "external_collection_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_collections_external_game_records_external_game_record~",
                        column: x => x.external_game_record_id,
                        principalTable: "external_game_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_collections_normalized_name",
                table: "collections",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_collection_records_collection_id",
                table: "external_collection_records",
                column: "collection_id");

            migrationBuilder.CreateIndex(
                name: "ux_external_collection_records_source_external_id",
                table: "external_collection_records",
                columns: new[] { "data_source_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_collections_collection_id",
                table: "game_collections",
                column: "collection_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_collections_external_collection_record_id",
                table: "game_collections",
                column: "external_collection_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_collections_game_id",
                table: "game_collections",
                column: "game_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_collections");

            migrationBuilder.DropTable(
                name: "external_collection_records");

            migrationBuilder.DropTable(
                name: "collections");
        }
    }
}
