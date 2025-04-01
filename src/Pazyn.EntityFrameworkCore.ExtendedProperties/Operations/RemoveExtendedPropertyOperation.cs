using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Entities;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Operations
{
    public class RemoveExtendedPropertyOperation : MigrationOperation
    {
        public SchemaTableColumn SchemaTableColumn { get; }
        public string Key { get; }

        public RemoveExtendedPropertyOperation(SchemaTableColumn schemaTableColumn, string key)
        {
            SchemaTableColumn = schemaTableColumn;
            Key = key;
        }
    }
}