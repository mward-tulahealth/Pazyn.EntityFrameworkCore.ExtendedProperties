using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Entities;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Operations;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties {
    public static class DbContextOptionsBuilderExtensions {
        public static OperationBuilder<AddExtendedPropertyOperation> AddSqlExtendedProperty(
        this MigrationBuilder migrationBuilder, string table, string column, string key, string value) {
            var newSchemaTableColumn = new SchemaTableColumn("dbo", table, column);
            var extendedProperty = new ExtendedProperty(key, value);
            var operation = new AddExtendedPropertyOperation(newSchemaTableColumn, extendedProperty);
            migrationBuilder.Operations.Add(operation);
            return new OperationBuilder<AddExtendedPropertyOperation>(operation);
        }

        public static OperationBuilder<RemoveExtendedPropertyOperation> RemoveSqlExtendedProperty(
            this MigrationBuilder migrationBuilder, string table, string column, string key) {
            var newSchemaTableColumn = new SchemaTableColumn("dbo", table, column);
            var operation = new RemoveExtendedPropertyOperation(newSchemaTableColumn, key);
            migrationBuilder.Operations.Add(operation);
            return new OperationBuilder<RemoveExtendedPropertyOperation>(operation);
        }
    }
}
