using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pazyn.EntityFrameworkCore.ExtendedProperties;

#nullable disable

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Tests.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExistingTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, comment: "Auto incrementing id that is for internal use only")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Public unique identifier of this point transaction"),
                    AddAttributeToExistingField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemoveAttributeFromExistingField = table.Column<bool>(type: "bit", nullable: false),
                    RemoveFieldAndAttributeFromExistingField = table.Column<int>(type: "int", nullable: false),
                    ExistingField_RenameExistingField = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExistingTable", x => x.Id);
                },
                comment: "Existing table for integration tests");

            migrationBuilder.CreateTable(
                name: "TableToBeRemoved",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, comment: "Auto incrementing id that is for internal use only")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Public unique identifier of this point transaction"),
                    FieldToBeRemoved = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableToBeRemoved", x => x.Id);
                },
                comment: "Table to be removed for integration tests");

            migrationBuilder.GenerateAddExtendedPropertySql(
                table: "ExistingTable", 
                column: "ExistingField_RenameExistingField"
            );

            migrationBuilder.GenerateAddExtendedPropertySql(
                table: "ExistingTable", 
                column: "RemoveAttributeFromExistingField"
            );

            migrationBuilder.GenerateAddExtendedPropertySql(
                table: "ExistingTable", 
                column: "RemoveFieldAndAttributeFromExistingField"
            );

            migrationBuilder.GenerateAddExtendedPropertySql(
                table: "TableToBeRemoved", 
                column: "FieldToBeRemoved"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExistingTable");

            migrationBuilder.DropTable(
                name: "TableToBeRemoved");
        }
    }
}
