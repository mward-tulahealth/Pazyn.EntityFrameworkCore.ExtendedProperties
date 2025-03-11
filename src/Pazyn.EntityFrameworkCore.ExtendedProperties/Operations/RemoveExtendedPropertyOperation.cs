using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Operations
{
    public class RemoveExtendedPropertyOperation : MigrationOperation
    {
        public SchemaTableColumn SchemaTableColumn { get; }
        public ExtendedProperty ExtendedProperty { get; }

        public RemoveExtendedPropertyOperation(SchemaTableColumn schemaTableColumn, ExtendedProperty extendedProperty)
        {
            SchemaTableColumn = schemaTableColumn;
            ExtendedProperty = extendedProperty;
        }
    }
}