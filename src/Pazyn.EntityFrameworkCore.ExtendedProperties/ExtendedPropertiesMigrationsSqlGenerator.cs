using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Operations;
using Microsoft.EntityFrameworkCore.Update;
using System.Diagnostics;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public class ExtendedPropertiesMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator {
        public ExtendedPropertiesMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            ICommandBatchPreparer migrationsCommandBatchPreparer)
            : base(dependencies, migrationsCommandBatchPreparer) {
            // Utilities.Log($"G.Generate(ColumnOperation) - constructor");
            DbContextHolder.DbContext = dependencies.CurrentContext.Context;
        }

        protected override void Generate(
            MigrationOperation operation,
            IModel model,
            MigrationCommandListBuilder builder) {
                // Debugger.Launch();
                switch (operation) {
                    case AddExtendedPropertyOperation addExtendedPropertyOperation:
                        Utilities.Log($"G.Generate(AddExtendedPropertyOperation) - {addExtendedPropertyOperation.SchemaTableColumn.Table}.{addExtendedPropertyOperation.SchemaTableColumn.Column}");
                        GenerateAddExtendedPropertySql(addExtendedPropertyOperation, builder, true);
                        break;
                    case RemoveExtendedPropertyOperation removeExtendedPropertyOperation:
                        Utilities.Log($"G.Generate(RemoveExtendedPropertyOperation) - {removeExtendedPropertyOperation.SchemaTableColumn.Table}.{removeExtendedPropertyOperation.SchemaTableColumn.Column}");
                        GenerateRemoveExtendedPropertySql(removeExtendedPropertyOperation, builder, true);
                        break;
                    default:
                        base.Generate(operation, model, builder);
                        break;
                }
        }

        // protected override void Generate(
        //         MigrationOperation operation,
        //         IModel model,
        //         MigrationCommandListBuilder builder) {
        //     Utilities.Log($"In ExtendedPropertiesMigrationsSqlGenerator GenerateMigrationOperation: {operation.GetType().Name}");

        //     switch (operation) {
        //         case AddColumnOperation addColumnOperation:
        //             var eps = Utilities.GetPropertyCustomAttributesFromAssembly(addColumnOperation.Table, addColumnOperation.Name) ? [new ExtendedProperty("PHI", "true")] : Array.Empty<ExtendedProperty>();
        //             // var eps = operation.GetExtendedProperties();
        //             Utilities.Log($"In ExtendedPropertiesMigrationsSqlGenerator AddColumnOperation: {addColumnOperation.Name} {addColumnOperation.Schema} {addColumnOperation.Table} with {eps.Length} extended properties");
        //             foreach(var ep in eps) {
        //                 GenerateAddExtendedPropertySql(new AddExtendedPropertyOperation(new SchemaTableColumn(addColumnOperation.Schema, addColumnOperation.Table, addColumnOperation.Name), ep), builder);
        //             }

        //             base.Generate(operation, model, builder);
        //             break;
        //         case AlterColumnOperation alterColumnOperation:
        //             var newExtendedProperties = Utilities.GetPropertyCustomAttributesFromAssembly(alterColumnOperation.Table, alterColumnOperation.Name) ? [new ExtendedProperty("PHI", "true")] : Array.Empty<ExtendedProperty>();
        //             if (alterColumnOperation.OldColumn == null) {
        //                 break;
        //             }

        //             var oldExtendedProperties = GetExistingSqlExtendedProperties(alterColumnOperation.OldColumn.Schema, alterColumnOperation.OldColumn.Table, alterColumnOperation.OldColumn.Name);
        //             if (oldExtendedProperties.Any(ep => ep.Key == "PHI" && ep.Value == "true")) {
        //                 GenerateRemoveExtendedPropertySql(new RemoveExtendedPropertyOperation(new SchemaTableColumn(alterColumnOperation.OldColumn.Schema, alterColumnOperation.OldColumn.Table, alterColumnOperation.OldColumn.Name), new ExtendedProperty("PHI", "true")), builder);
        //             }
                    
        //             Utilities.Log($"In ExtendedPropertiesMigrationsSqlGenerator AlterColumnOperation: {alterColumnOperation.Name} {alterColumnOperation.Schema} {alterColumnOperation.Table} {alterColumnOperation.OldColumn?.Name} {alterColumnOperation.OldColumn?.Schema} {alterColumnOperation.OldColumn?.Table} with {newExtendedProperties.Length} extended properties");
        //             foreach(var ep in newExtendedProperties) {
        //                 GenerateAddExtendedPropertySql(new AddExtendedPropertyOperation(new SchemaTableColumn(alterColumnOperation.Schema, alterColumnOperation.Table, alterColumnOperation.Name), ep), builder);
        //             }

        //             base.Generate(operation, model, builder);
        //             break;
        //         case RenameColumnOperation renameColumnOperation:
        //             foreach(var ep in operation.GetExtendedProperties()) {
        //                 GenerateAddExtendedPropertySql(new AddExtendedPropertyOperation(new SchemaTableColumn(renameColumnOperation.Schema, renameColumnOperation.Table, renameColumnOperation.Name), ep), builder);
        //             }

        //             base.Generate(operation, model, builder);
        //             break;
        //         case CreateTableOperation createTableOperation:
        //             Generate(createTableOperation, model, builder);
        //             break;
        //         case AlterTableOperation alterTableOperation:
        //             Generate(alterTableOperation, model, builder);
        //             break;
        //         default:
        //             base.Generate(operation, model, builder);
        //             break;
        //     }
        // }

        // private IEnumerable<ExtendedProperty> GetExistingSqlExtendedProperties(string schema, string table, string column)
        // {
        //     var shortTableName = table.Split('.').Last();


        //     // column = "Email"; //TODO remove this line


        //     var extendedProperties = new List<ExtendedProperty>();
        //     try {
        //         connection ??= dbContext.Database.GetDbConnection() as SqlConnection;
        //         Utilities.Log($"{nameof(GetExistingSqlExtendedProperties)} - connectionString: {connection.ConnectionString}");

        //         if (connection.State != System.Data.ConnectionState.Open)
        //         {
        //             connection.Open();
        //         }

        //         var command = connection.CreateCommand();

        //         command.CommandText = $@"
        //             SELECT hprop.name, hprop.value
        //             FROM INFORMATION_SCHEMA.TABLES AS tbl
        //             INNER JOIN INFORMATION_SCHEMA.COLUMNS AS col ON col.TABLE_NAME = tbl.TABLE_NAME AND col.TABLE_SCHEMA = tbl.TABLE_SCHEMA
        //             INNER JOIN sys.columns AS sc ON sc.object_id = OBJECT_ID(tbl.TABLE_SCHEMA + '.' + tbl.TABLE_NAME) AND sc.name = col.COLUMN_NAME
        //             LEFT JOIN sys.extended_properties hprop ON hprop.major_id = sc.object_id AND hprop.minor_id = sc.column_id AND hprop.name = 'PHI'
        //             WHERE tbl.TABLE_SCHEMA = '{schema}' AND tbl.TABLE_NAME = '{shortTableName}' AND col.COLUMN_NAME = '{column}'";

        //         // Utilities.Log($"{nameof(GetExistingSqlExtendedProperties)} - command: {command.CommandText}");

        //         using (var reader = command.ExecuteReader())
        //         {
        //             while (reader.Read())
        //             {
        //                 var name = reader.IsDBNull(0) ? null : reader.GetString(0);
        //                 var value = reader.IsDBNull(1) ? null : reader.GetString(1);
        //                 Utilities.Log($"\t\t\t{nameof(GetExistingSqlExtendedProperties)} - name, value: {name}, {value}");
        //                 if (name != null && value != null)
        //                 {
        //                     Utilities.Log($"\t\t\t\t{nameof(GetExistingSqlExtendedProperties)} - ADDING EXTENDED PROPERTY: {name}, {value}");
        //                     extendedProperties.Add(new ExtendedProperty(name, value));
        //                 }
        //             }
        //         }
        //     }
        //     catch (Exception e) {
        //         Utilities.Log($"\t\t{nameof(GetExistingSqlExtendedProperties)} - Error getting extended properties: {e.Message}");
        //     }

        //     return extendedProperties;
        // }

        // private bool IsPHIColumn(MigrationOperation operation, IModel model) {
        //     if (operation is ColumnOperation columnOperation) {
        //         Utilities.Log($"\tIn IsPHIColumn Table.Name: {columnOperation.Table}.{columnOperation.Name}");
        //         var property = model?.GetEntityTypes().FirstOrDefault(t => t.Name.Contains("Domain.Entities."+columnOperation.Table))?.FindProperty(columnOperation.Name);
        //         property ??= model?.GetEntityTypes().FirstOrDefault(t => t.Name.Contains(columnOperation.Table))?.FindProperty(columnOperation.Name);
        //         Utilities.Log($"\tIn IsPHIColumn model: {model?.FindEntityType("LiveTula.Gateway.Domain.Entities."+columnOperation.Table)}");
        //         Utilities.Log($"\tIn IsPHIColumn property: {property}");
        //         var propertyInfo = property?.PropertyInfo;
        //         Utilities.Log($"\tIn IsPHIColumn propertyInfo: {Newtonsoft.Json.JsonConvert.SerializeObject(propertyInfo)}");
        //         if (propertyInfo != null) {
        //             var customAttributes = propertyInfo.GetCustomAttributes(true).ToList();
        //             Utilities.Log($"\tIn IsPHIColumn customAttributes count: {customAttributes.Count}");
        //             foreach (var attribute in customAttributes) {
        //                 Utilities.Log($"\tIn IsPHIColumn customAttribute: {attribute.GetType().Name}");
        //             }
        //             if (customAttributes.Any(attr => attr is PHIAttribute)) {
        //                 Utilities.Log($"\tPHI attribute found on operation {columnOperation.Name}");
        //                 operation.AddAnnotation("PHI", true);
        //                 return true;
        //             }
        //         }
        //     }

        //     Utilities.Log($"\tPHI attribute not found on operation {operation.GetType().Name}");
        //     return false;
        // }

        private void HandleAddColumnOperation(
        AddColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate) {
            base.Generate(operation, model, builder, terminate);

            foreach (var key in Config.AttributeToExtendedPropertyMap.Keys) {
                var annotation = operation.FindAnnotation(key);
                if (annotation != null) {
                    if (annotation.Value as string == "true") {
                        var extendedProperty = Config.AttributeToExtendedPropertyMap[key];
                        Utilities.Log($"G.Generate(AddColumnOperation) - Found annotation {key}");
                        GenerateAddExtendedPropertySql(new AddExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Table, operation.Name), extendedProperty), builder);
                    }
                }
            }
        }

        // protected override void Generate(
        // AlterColumnOperation operation,
        // IModel? model,
        // MigrationCommandListBuilder builder) {
        //     Utilities.Log($"G.Generate(ColumnOperation) - AlterColumnOperation");
        //     base.Generate(operation, model, builder);
        //     foreach (var extendedProperty in operation.OldColumn.GetExtendedProperties()) {
        //         GenerateRemoveExtendedPropertySql(new RemoveExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Table, operation.Name), extendedProperty), builder);
        //     }

        //     foreach (var extendedProperty in operation.GetExtendedProperties()) {
        //         GenerateAddExtendedPropertySql(new AddExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Table, operation.Name), extendedProperty), builder);
        //     }
        // }

        // protected override void Generate(
        // RenameColumnOperation operation,
        // IModel? model,
        // MigrationCommandListBuilder builder) {
        //     Utilities.Log($"G.Generate(ColumnOperation) - RenameColumnOperation");
        //     Utilities.Log($"OldName: {operation.Name}");
        //     Utilities.Log($"NewName: {operation.NewName}");
        //     Utilities.Log($"A: {operation.GetExtendedProperties().Length} extended properties");
        //     Utilities.Log($"B: {operation.GetExtendedProperties()}");
        //     Utilities.Log($"C: {operation.FindAnnotation("PHI")}");
        //     Utilities.Log($"D: {operation.GetAnnotations()}");
        //     base.Generate(operation, model, builder);

        //     foreach (var extendedProperty in operation.GetExtendedProperties()) {
        //         GenerateAddExtendedPropertySql(new AddExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Table, operation.Name), extendedProperty), builder);
        //     }
        // }

        // protected override void Generate(
        // CreateTableOperation operation,
        // IModel? model,
        // MigrationCommandListBuilder builder,
        // bool terminate = true) {
        //     Utilities.Log($"G.Generate(ColumnOperation) - CreateTableOperation");
        //     base.Generate(operation, model, builder);
        //     foreach (var extendedProperty in operation.GetExtendedProperties()) {
        //         GenerateAddExtendedPropertySql(new AddExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Name, null), extendedProperty), builder);
        //     }

        //     foreach (var addColumnOperation in operation.Columns) {
        //         foreach (var extendedProperty in addColumnOperation.GetExtendedProperties()) {
        //             GenerateAddExtendedPropertySql(new AddExtendedPropertyOperation(new SchemaTableColumn(addColumnOperation.Schema, addColumnOperation.Table, addColumnOperation.Name), extendedProperty), builder);
        //         }
        //     }
        // }

        // protected override void Generate(AlterTableOperation operation, IModel? model, MigrationCommandListBuilder builder) {
        //     Utilities.Log($"G.Generate(ColumnOperation) - AlterTableOperation");
        //     base.Generate(operation, model, builder);
        //     foreach (var extendedProperty in operation.OldTable.GetExtendedProperties()) {
        //         GenerateRemoveExtendedPropertySql(new RemoveExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Name, null), extendedProperty), builder);
        //     }

        //     foreach (var extendedProperty in operation.GetExtendedProperties()) {
        //         GenerateAddExtendedPropertySql(new AddExtendedPropertyOperation(new SchemaTableColumn(operation.Schema, operation.Name, null), extendedProperty), builder);
        //     }
        // }

        private void GenerateAddExtendedPropertySql(AddExtendedPropertyOperation operation, MigrationCommandListBuilder builder, Boolean terminate = true) {
            Utilities.Log($"G.{nameof(GenerateAddExtendedPropertySql)} - AddExtendedPropertyOperation");

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

            builder.EndCommand();
        }

        private void GenerateRemoveExtendedPropertySql(RemoveExtendedPropertyOperation operation, MigrationCommandListBuilder builder, Boolean terminate = true) {
            Utilities.Log($"G.{nameof(GenerateRemoveExtendedPropertySql)} - RemoveExtendedPropertyOperation");

            builder.Append("EXEC sys.sp_dropextendedproperty");
            builder.AppendLine($" @name = N'{operation.ExtendedProperty.Key}',");
            builder.Append("@level0type = N'SCHEMA',");
            builder.AppendLine($" @level0name = N'{operation.SchemaTableColumn.Schema ?? "dbo"}',");
            builder.Append("@level1type = N'TABLE',");
            builder.Append($" @level1name = N'{operation.SchemaTableColumn.Table}'");

            if (!string.IsNullOrEmpty(operation.SchemaTableColumn.Column)) {
                builder.AppendLine(",");
                builder.Append($@"@level2type = N'COLUMN', @level2name = N'{operation.SchemaTableColumn.Column}'");
            }

            builder.AppendLine(";");

            builder.EndCommand();
        }
    }

    public class HPropResult
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
