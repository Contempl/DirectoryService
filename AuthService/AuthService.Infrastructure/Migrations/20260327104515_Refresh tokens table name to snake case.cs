using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Core.Migrations
{
    /// <inheritdoc />
    public partial class Refreshtokenstablenametosnakecase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_application_users_user_id",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "refresh_tokens");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_user_id",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_application_users_user_id",
                table: "refresh_tokens",
                column: "user_id",
                principalTable: "application_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_application_users_user_id",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                newName: "RefreshTokens");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_user_id",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_application_users_user_id",
                table: "RefreshTokens",
                column: "user_id",
                principalTable: "application_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
