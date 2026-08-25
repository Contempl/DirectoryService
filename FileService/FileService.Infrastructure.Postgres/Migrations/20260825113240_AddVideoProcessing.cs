using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UploadId",
                table: "media_assets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hls_root_key",
                table: "media_assets",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_metadata",
                table: "media_assets",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "video_processing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    video_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    overall_progress_percentage = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    is_critical_error = table.Column<bool>(type: "boolean", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    max_retries = table.Column<int>(type: "integer", nullable: false),
                    next_retry_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_processing", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processing_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    weight = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    result_data = table.Column<string>(type: "jsonb", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    video_processing_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processing_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_processing_steps_video_processing_video_processing_id",
                        column: x => x.video_processing_id,
                        principalTable: "video_processing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_processing_steps_status",
                table: "processing_steps",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_processing_steps_step_type",
                table: "processing_steps",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_processing_steps_video_processing_id",
                table: "processing_steps",
                column: "video_processing_id");

            migrationBuilder.CreateIndex(
                name: "ix_video_processing_status",
                table: "video_processing",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_video_processing_status_started_at",
                table: "video_processing",
                columns: new[] { "status", "started_at" });

            migrationBuilder.CreateIndex(
                name: "ux_video_processing_video_asset_id",
                table: "video_processing",
                column: "video_asset_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processing_steps");

            migrationBuilder.DropTable(
                name: "video_processing");

            migrationBuilder.DropColumn(
                name: "UploadId",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "hls_root_key",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "video_metadata",
                table: "media_assets");
        }
    }
}
