using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemovednagiationentityfromRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "refresh_tokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_ApplicationUserId",
                table: "refresh_tokens",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_application_users_ApplicationUserId",
                table: "refresh_tokens",
                column: "ApplicationUserId",
                principalTable: "application_users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_application_users_ApplicationUserId",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_ApplicationUserId",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "refresh_tokens");
        }
    }
}
