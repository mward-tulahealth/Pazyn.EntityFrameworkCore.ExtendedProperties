using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Operations;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public class CustomCSharpMigrationOperationGenerator : CSharpMigrationOperationGenerator {
        public CustomCSharpMigrationOperationGenerator(CSharpMigrationOperationGeneratorDependencies dependencies)
            : base(dependencies) {
        }

        protected override void Generate(MigrationOperation operation, IndentedStringBuilder builder) {
            if (operation is AddExtendedPropertyOperation addExtendedPropertyOperation) {
                GenerateAddExtendedPropertyOperation(addExtendedPropertyOperation, builder);
            }
            else if (operation is RemoveExtendedPropertyOperation removeExtendedPropertyOperation) {
                GenerateRemoveExtendedPropertyOperation(removeExtendedPropertyOperation, builder);
            }
            else {
                base.Generate(operation, builder);
            }
        }

        private void GenerateAddExtendedPropertyOperation(AddExtendedPropertyOperation addExtendedPropertyOperation, IndentedStringBuilder builder) {
            builder.AppendLine($".AddSqlExtendedProperty(");
            using (builder.Indent()) {
                builder.AppendLine($"table: \"{addExtendedPropertyOperation.SchemaTableColumn.Table}\", ");
                builder.AppendLine($"column: \"{addExtendedPropertyOperation.SchemaTableColumn.Column}\", ");
                builder.AppendLine($"key: \"{addExtendedPropertyOperation.ExtendedProperty.Key}\", ");
                builder.AppendLine($"value: \"{addExtendedPropertyOperation.ExtendedProperty.Value}\"");
            }
            builder.Append($")");
        }

        private void GenerateRemoveExtendedPropertyOperation(RemoveExtendedPropertyOperation removeExtendedPropertyOperation, IndentedStringBuilder builder) {
            builder.AppendLine($".RemoveSqlExtendedProperty(");
            using (builder.Indent()) {
                builder.AppendLine($"table: \"{removeExtendedPropertyOperation.SchemaTableColumn.Table}\", ");
                builder.AppendLine($"column: \"{removeExtendedPropertyOperation.SchemaTableColumn.Column}\", ");
                builder.AppendLine($"key: \"{removeExtendedPropertyOperation.Key}\"");
            }
            builder.Append($")");
        }
    }
}
