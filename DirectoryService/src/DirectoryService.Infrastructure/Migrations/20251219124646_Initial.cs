using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    depth = table.Column<short>(type: "smallint", nullable: false),
                    ChildrenCount = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    identifier = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    street = table.Column<string>(type: "text", nullable: false),
                    house = table.Column<string>(type: "text", nullable: false),
                    apartment = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    timezone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name_Value = table.Column<string>(type: "name", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "department_locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_department_locations_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_department_locations_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "department_positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department_positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_department_positions_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_department_positions_positions_position_id",
                        column: x => x.position_id,
                        principalTable: "positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_department_id",
                table: "department_locations",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_location_id",
                table: "department_locations",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_department_id",
                table: "department_positions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_position_id",
                table: "department_positions",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "ix_locations_address",
                table: "locations",
                columns: new[] { "city", "street", "house", "apartment" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_locations_name",
                table: "locations",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "department_locations");

            migrationBuilder.DropTable(
                name: "department_positions");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "positions");
        }
    }
}
