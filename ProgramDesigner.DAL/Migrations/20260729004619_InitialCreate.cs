using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramDesigner.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgramItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    ParentGroupId = table.Column<int>(type: "int", nullable: true),
                    PrerequisiteItemId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ChoiceCount = table.Column<int>(type: "int", nullable: true),
                    StepType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramItems_ProgramItems_ParentGroupId",
                        column: x => x.ParentGroupId,
                        principalTable: "ProgramItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramItems_ProgramItems_PrerequisiteItemId",
                        column: x => x.PrerequisiteItemId,
                        principalTable: "ProgramItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearningPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RootGroupId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningPrograms_ProgramItems_RootGroupId",
                        column: x => x.RootGroupId,
                        principalTable: "ProgramItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningPrograms_RootGroupId",
                table: "LearningPrograms",
                column: "RootGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramItems_ParentGroupId",
                table: "ProgramItems",
                column: "ParentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramItems_PrerequisiteItemId",
                table: "ProgramItems",
                column: "PrerequisiteItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningPrograms");

            migrationBuilder.DropTable(
                name: "ProgramItems");
        }
    }
}
