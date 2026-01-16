using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LtreeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name_Value",
                table: "positions",
                newName: "name");

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS ltree;");
            
            migrationBuilder.Sql(
                @"ALTER TABLE departments 
          ALTER COLUMN path TYPE ltree 
          USING path::text::ltree;"
            );

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "positions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "name",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "path",
                table: "departments",
                type: "ltree",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_positions_name",
                table: "positions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_department_path",
                table: "departments",
                column: "path")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_positions_name",
                table: "positions");

            migrationBuilder.DropIndex(
                name: "idx_department_path",
                table: "departments");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "positions",
                newName: "Name_Value");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "Name_Value",
                table: "positions",
                type: "name",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "path",
                table: "departments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "ltree",
                oldMaxLength: 500);
        }
    }
}
