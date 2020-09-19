using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Operations;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public class ExtendedPropertiesMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
    {
        public ExtendedPropertiesMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IMigrationsAnnotationProvider migrationsAnnotations) : base(dependencies, migrationsAnnotations)
        {
        }

        protected override void Generate(AddColumnOperation operation, IModel model, MigrationCommandListBuilder builder, Boolean terminate)
        {
            base.Generate(operation, model, builder);
            foreach (var extendedProperty in operation.GetExtendedProperties())
            {
                Generate(new AddExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Table, operation.Name), extendedProperty), builder);
            }
        }

        protected override void Generate(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);
            foreach (var extendedProperty in operation.OldColumn.GetExtendedProperties())
            {
                Generate(new RemoveExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Table, operation.Name), extendedProperty), builder);
            }

            foreach (var extendedProperty in operation.GetExtendedProperties())
            {
                Generate(new AddExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Table, operation.Name), extendedProperty), builder);
            }
        }

        protected override void Generate(CreateTableOperation operation, IModel model, MigrationCommandListBuilder builder, Boolean terminate = true)
        {
            base.Generate(operation, model, builder);
            foreach (var extendedProperty in operation.GetExtendedProperties())
            {
                Generate(new AddExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Name, null), extendedProperty), builder);
            }

            foreach (var addColumnOperation in operation.Columns)
            {
                foreach (var extendedProperty in addColumnOperation.GetExtendedProperties())
                {
                    Generate(new AddExtendedPropertyOperation(new SchemaTableColumn(addColumnOperation.Schema, addColumnOperation.Table, addColumnOperation.Name), extendedProperty), builder);
                }
            }
        }

        protected override void Generate(AlterTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);
            foreach (var extendedProperty in operation.OldTable.GetExtendedProperties())
            {
                Generate(new RemoveExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Name, null), extendedProperty), builder);
            }

            foreach (var extendedProperty in operation.GetExtendedProperties())
            {
                Generate(new AddExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Name, null), extendedProperty), builder);
            }
        }

        private void Generate(AddExtendedPropertyOperation operation, MigrationCommandListBuilder builder)
        {
            builder.Append("EXEC sys.sp_addextendedproperty");
            builder.Append($" @name = N'{operation.ExtendedProperty.Key}'");
            builder.Append($", @value = N'{operation.ExtendedProperty.Value}'");
            builder.Append(", @level0type = N'SCHEMA'");
            builder.Append($", @level0name = N'{operation.SchemaTableColumn.Schema ?? "dbo"}'");
            builder.Append(", @level1type = N'TABLE'");
            builder.Append($", @level1name = N'{operation.SchemaTableColumn.Table}'");

            if (!String.IsNullOrEmpty(operation.SchemaTableColumn.Column))
            {
                builder.Append($@",@level2type = 'COLUMN', @level2name = '{operation.SchemaTableColumn.Column}'");
            }

            builder.EndCommand();
        }

        private void Generate(RemoveExtendedPropertyOperation operation, MigrationCommandListBuilder builder)
        {
            builder.Append("EXEC sys.sp_dropextendedproperty");
            builder.Append($" @name = N'{operation.ExtendedProperty.Key}'");
            builder.Append(", @level0type = N'SCHEMA'");
            builder.Append($", @level0name = N'{operation.SchemaTableColumn.Schema ?? "dbo"}'");
            builder.Append(", @level1type = N'TABLE'");
            builder.Append($", @level1name = N'{operation.SchemaTableColumn.Table}'");

            if (!String.IsNullOrEmpty(operation.SchemaTableColumn.Column))
            {
                builder.Append($@",@level2type = 'COLUMN', @level2name = '{operation.SchemaTableColumn.Column}'");
            }

            builder.EndCommand();
        }
    }
}