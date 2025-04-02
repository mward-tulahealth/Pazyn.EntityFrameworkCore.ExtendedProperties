using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Operations;
using Microsoft.EntityFrameworkCore.Update;
using System.Diagnostics;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Entities;
using Microsoft.Extensions.Logging;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public class ExtendedPropertiesMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator {
        private static readonly ILogger Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<CustomMigrationsModelDiffer>();

        public ExtendedPropertiesMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            ICommandBatchPreparer migrationsCommandBatchPreparer)
            : base(dependencies, migrationsCommandBatchPreparer) {
            DbContextHolder.DbContext = dependencies.CurrentContext.Context;
        }

        protected override void Generate(
        MigrationOperation operation,
        IModel model,
        MigrationCommandListBuilder builder) {
            // Debugger.Launch();
            switch (operation) {
                case AddExtendedPropertyOperation addExtendedPropertyOperation:
                    AddSqlExtendedProperty(addExtendedPropertyOperation, builder);
                    break;
                case RemoveExtendedPropertyOperation removeExtendedPropertyOperation:
                    RemoveSqlExtendedProperty(removeExtendedPropertyOperation, builder);
                    break;
                default:
                    base.Generate(operation, model, builder);
                    break;
            }
        }
        
        private void AddSqlExtendedProperty(AddExtendedPropertyOperation operation, MigrationCommandListBuilder builder) {
            Logger.LogInformation($"{nameof(AddSqlExtendedProperty)} - {operation.SchemaTableColumn.Table}.{operation.SchemaTableColumn.Column}");

            builder.AppendLine("IF NOT EXISTS (SELECT 1 FROM sys.extended_properties");
            builder.AppendLine($"WHERE name = N'{operation.ExtendedProperty.Key}'");
            builder.AppendLine($"AND major_id = OBJECT_ID(N'{operation.SchemaTableColumn.Schema ?? "dbo"}.{operation.SchemaTableColumn.Table}')");
            if (!string.IsNullOrEmpty(operation.SchemaTableColumn.Column)) {
                builder.AppendLine($"AND minor_id = (SELECT column_id FROM sys.columns WHERE name = N'{operation.SchemaTableColumn.Column}' AND object_id = OBJECT_ID(N'{operation.SchemaTableColumn.Schema ?? "dbo"}.{operation.SchemaTableColumn.Table}'))");
            } else {
                builder.AppendLine("AND minor_id = 0");
            }
            builder.AppendLine(")");

            builder.AppendLine("BEGIN");
            builder.Append("EXEC sys.sp_addextendedproperty");
            builder.Append($" @name = N'{operation.ExtendedProperty.Key}',");
            builder.AppendLine($" @value = N'{operation.ExtendedProperty.Value}',");
            builder.Append("@level0type = N'SCHEMA',");
            builder.AppendLine($" @level0name = N'{operation.SchemaTableColumn.Schema ?? "dbo"}',");
            builder.Append(" @level1type = N'TABLE',");
            builder.Append($"@level1name = N'{operation.SchemaTableColumn.Table}'");

            if (!string.IsNullOrEmpty(operation.SchemaTableColumn.Column)) {
                builder.AppendLine(",");
                builder.Append($@"@level2type = N'COLUMN', @level2name = N'{operation.SchemaTableColumn.Column}'");
            }

            builder.AppendLine(";");
            builder.AppendLine("END");

            builder.EndCommand();
        }

        private void RemoveSqlExtendedProperty(RemoveExtendedPropertyOperation operation, MigrationCommandListBuilder builder) {
            Logger.LogInformation($"{nameof(RemoveSqlExtendedProperty)} - {operation.SchemaTableColumn.Table}.{operation.SchemaTableColumn.Column}");

            builder.AppendLine("IF EXISTS (SELECT 1 FROM sys.extended_properties");
            builder.AppendLine($"WHERE name = N'{operation.Key}'");
            builder.AppendLine($"AND major_id = OBJECT_ID(N'{operation.SchemaTableColumn.Schema ?? "dbo"}.{operation.SchemaTableColumn.Table}')");
            if (!string.IsNullOrEmpty(operation.SchemaTableColumn.Column)) {
                builder.AppendLine($"AND minor_id = (SELECT column_id FROM sys.columns WHERE name = N'{operation.SchemaTableColumn.Column}' AND object_id = OBJECT_ID(N'{operation.SchemaTableColumn.Schema ?? "dbo"}.{operation.SchemaTableColumn.Table}'))");
            } else {
                builder.AppendLine("AND minor_id = 0");
            }
            builder.AppendLine(")");

            builder.AppendLine("BEGIN");
            builder.Append("EXEC sys.sp_dropextendedproperty");
            builder.AppendLine($" @name = N'{operation.Key}',");
            builder.Append("@level0type = N'SCHEMA',");
            builder.AppendLine($" @level0name = N'{operation.SchemaTableColumn.Schema ?? "dbo"}',");
            builder.Append("@level1type = N'TABLE',");
            builder.Append($" @level1name = N'{operation.SchemaTableColumn.Table}'");

            if (!string.IsNullOrEmpty(operation.SchemaTableColumn.Column)) {
                builder.AppendLine(",");
                builder.Append($@"@level2type = N'COLUMN', @level2name = N'{operation.SchemaTableColumn.Column}'");
            }

            builder.AppendLine(";");
            builder.AppendLine("END");

            builder.EndCommand();
        }
    }
}
