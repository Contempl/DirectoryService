using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "photo_asset_id",
                table: "locations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_content_type",
                table: "locations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_file_name",
                table: "locations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "photo_size",
                table: "locations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "photo_verified_at",
                table: "locations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_locations_photo_asset_id",
                table: "locations",
                column: "photo_asset_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_locations_photo_asset_id",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "photo_asset_id",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "photo_content_type",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "photo_file_name",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "photo_size",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "photo_verified_at",
                table: "locations");
        }
    }
}
