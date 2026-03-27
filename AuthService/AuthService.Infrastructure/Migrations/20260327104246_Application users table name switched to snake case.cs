using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Core.Migrations
{
    /// <inheritdoc />
    public partial class Applicationuserstablenameswitchedtosnakecase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_ApplicationUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_ApplicationUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_ApplicationUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_ApplicationUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_ApplicationUsers_user_id",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationUsers",
                table: "ApplicationUsers");

            migrationBuilder.RenameTable(
                name: "ApplicationUsers",
                newName: "application_users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_application_users",
                table: "application_users",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_application_users_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "application_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_application_users_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "application_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_application_users_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "application_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_application_users_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "application_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_application_users_user_id",
                table: "RefreshTokens",
                column: "user_id",
                principalTable: "application_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_application_users_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_application_users_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_application_users_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_application_users_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_application_users_user_id",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_application_users",
                table: "application_users");

            migrationBuilder.RenameTable(
                name: "application_users",
                newName: "ApplicationUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationUsers",
                table: "ApplicationUsers",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_ApplicationUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_ApplicationUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_ApplicationUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_ApplicationUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_ApplicationUsers_user_id",
                table: "RefreshTokens",
                column: "user_id",
                principalTable: "ApplicationUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
