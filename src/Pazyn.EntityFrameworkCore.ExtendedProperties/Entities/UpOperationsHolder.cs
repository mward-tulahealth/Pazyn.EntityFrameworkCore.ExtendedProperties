using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Operations;
using System.Collections.Generic;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Entities {
    public static class UpOperationsHolder {
        private static readonly ILogger Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<CustomMigrationsModelDiffer>();
        public static List<AddExtendedPropertyOperation> AddExtendedPropertyOperations { get; set; } = new List<AddExtendedPropertyOperation>();
        public static List<RemoveExtendedPropertyOperation> RemoveExtendedPropertyOperations { get; set; } = new List<RemoveExtendedPropertyOperation>();

        // This method reverses the operations in the UpOperationsHolder to create a down migration.
        public static List<MigrationOperation> GetDownOperations() {
            var downOperations = new List<MigrationOperation>();

            foreach (var addOperation in AddExtendedPropertyOperations)
            {
                downOperations.Add(new RemoveExtendedPropertyOperation(
                    addOperation.SchemaTableColumn,
                    addOperation.ExtendedProperty.Key));
            }

            foreach (var removeOperation in RemoveExtendedPropertyOperations)
            {
                var ep = GetExtendedPropertyValue(removeOperation);
                if (ep != null)
                {
                    downOperations.Add(new AddExtendedPropertyOperation(
                        removeOperation.SchemaTableColumn,
                        ep));
                }
                else
                {
                    downOperations.Add(new AddExtendedPropertyOperation(
                        removeOperation.SchemaTableColumn,
                        new ExtendedProperty(removeOperation.Key, "true")));
                }
            }

            return downOperations;
        }

        private static ExtendedProperty GetExtendedPropertyValue(RemoveExtendedPropertyOperation removeOperation) {
            // Search cached attributes for matching extended property.
            Logger.LogInformation($"Searching for extended property with key: {removeOperation.Key} in {ExtendedPropertyAttribute.GetAllExtendedPropertiesDictionary().Values}");
            foreach (ExtendedProperty ep in ExtendedPropertyAttribute.GetAllExtendedPropertiesDictionary().Values)
            {
                if (ep.Key == removeOperation.Key)
                {
                    return ep;
                }
            }

            return null;
        }

        // After this holder has been used for Down operations, it should be cleared to avoid reusing the same operations.
        internal static void Clear() {
            AddExtendedPropertyOperations.Clear();
            RemoveExtendedPropertyOperations.Clear();
        }
    }
}
