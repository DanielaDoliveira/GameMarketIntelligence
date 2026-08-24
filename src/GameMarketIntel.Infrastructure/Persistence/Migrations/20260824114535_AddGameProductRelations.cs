using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketIntel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGameProductRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_product_relations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_source_game_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_target_game_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_product_relations", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_product_relations_Games_source_game_id",
                        column: x => x.source_game_id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_product_relations_Games_target_game_id",
                        column: x => x.target_game_id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_product_relations_external_game_records_external_sourc~",
                        column: x => x.external_source_game_record_id,
                        principalTable: "external_game_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_product_relations_external_game_records_external_targe~",
                        column: x => x.external_target_game_record_id,
                        principalTable: "external_game_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_product_relations_external_target_game_record_id",
                table: "game_product_relations",
                column: "external_target_game_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_product_relations_source_game_id",
                table: "game_product_relations",
                column: "source_game_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_product_relations_target_game_id",
                table: "game_product_relations",
                column: "target_game_id");

            migrationBuilder.CreateIndex(
                name: "ux_game_product_relations_external_records_type",
                table: "game_product_relations",
                columns: new[] { "external_source_game_record_id", "external_target_game_record_id", "relation_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_product_relations");
        }
    }
}
