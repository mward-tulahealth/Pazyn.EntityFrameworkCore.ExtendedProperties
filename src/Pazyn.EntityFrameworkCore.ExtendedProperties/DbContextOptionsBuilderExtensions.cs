using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Operations;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder AddExtendedProperties(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, ExtendedPropertiesMigrationsSqlGenerator>();

            var extension = optionsBuilder.Options.FindExtension<ExtendedPropertiesExtension>() ?? new ExtendedPropertiesExtension();
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            return optionsBuilder;
        }

        public static OperationBuilder<AddExtendedPropertyOperation> GenerateAddExtendedPropertySql(
        this MigrationBuilder migrationBuilder,
        string table,
        string column)
        {
            var newSchemaTableColumn = new SchemaTableColumn("dbo", table, column);
            var extendedProperty = new ExtendedProperty("PHI", "true");
            var operation = new AddExtendedPropertyOperation(newSchemaTableColumn, extendedProperty);
            migrationBuilder.Operations.Add(operation);
            return new OperationBuilder<AddExtendedPropertyOperation>(operation);
        }

        public static OperationBuilder<RemoveExtendedPropertyOperation> GenerateRemoveExtendedPropertySql(
            this MigrationBuilder migrationBuilder,
            string table,
            string column)
        {
            var newSchemaTableColumn = new SchemaTableColumn("dbo", table, column);
            var extendedProperty = new ExtendedProperty("PHI", "true");
            var operation = new RemoveExtendedPropertyOperation(newSchemaTableColumn, extendedProperty);
            migrationBuilder.Operations.Add(operation);
            return new OperationBuilder<RemoveExtendedPropertyOperation>(operation);
        }
    }
}
