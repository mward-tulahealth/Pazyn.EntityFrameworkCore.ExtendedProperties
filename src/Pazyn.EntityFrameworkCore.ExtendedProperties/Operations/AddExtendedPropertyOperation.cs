using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Operations
{
    public class AddExtendedPropertyOperation : MigrationOperation
    {
        public SchemaTableColumn SchemaTableColumn { get; }
        public ExtendedProperty ExtendedProperty { get; }

        public AddExtendedPropertyOperation(SchemaTableColumn schemaTableColumn, ExtendedProperty extendedProperty)
        {
            SchemaTableColumn = schemaTableColumn;
            ExtendedProperty = extendedProperty;
        }
    }
}