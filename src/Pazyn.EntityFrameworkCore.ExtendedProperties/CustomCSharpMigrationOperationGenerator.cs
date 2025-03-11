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
                Utilities.Log($"C.{nameof(Generate)} - AddExtendedPropertyOperation: {addExtendedPropertyOperation}");
                GenerateAddExtendedPropertyOperation(addExtendedPropertyOperation, builder);
            }
            else if (operation is RemoveExtendedPropertyOperation removeExtendedPropertyOperation) {
                Utilities.Log($"C.{nameof(Generate)} - RemoveExtendedPropertyOperation: {removeExtendedPropertyOperation}");
                GenerateRemoveExtendedPropertyOperation(removeExtendedPropertyOperation, builder);
            }
            else {
                base.Generate(operation, builder);
            }
        }

        private void GenerateAddExtendedPropertyOperation(AddExtendedPropertyOperation addExtendedPropertyOperation, IndentedStringBuilder builder) {
            builder.AppendLine($".GenerateAddExtendedPropertySql(");
            using (builder.Indent()) {
                builder.AppendLine($"table: \"{addExtendedPropertyOperation.SchemaTableColumn.Table}\", ");
                builder.AppendLine($"column: \"{addExtendedPropertyOperation.SchemaTableColumn.Column}\"");
            }
            builder.Append($")");
        }

        private void GenerateRemoveExtendedPropertyOperation(RemoveExtendedPropertyOperation removeExtendedPropertyOperation, IndentedStringBuilder builder) {
            builder.AppendLine($".GenerateRemoveExtendedPropertySql(");
            using (builder.Indent()) {
                builder.AppendLine($"table: \"{removeExtendedPropertyOperation.SchemaTableColumn.Table}\", ");
                builder.AppendLine($"column: \"{removeExtendedPropertyOperation.SchemaTableColumn.Column}\"");
            }
            builder.Append($")");
        }
    }

    public class CustomCSharpMigrationsGenerator : CSharpMigrationsGenerator
    {
        public CustomCSharpMigrationsGenerator(MigrationsCodeGeneratorDependencies dependencies, CSharpMigrationsGeneratorDependencies csharpDependencies) : base(dependencies, csharpDependencies)
        {
        }

        protected override IEnumerable<string> GetNamespaces(IEnumerable<MigrationOperation> operations) => base.GetNamespaces(operations).Concat(new List<string> { typeof(ExtendedPropertiesExtension).Namespace });
    }
}
