using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties {
    public class CustomMigrationsModelDiffer : MigrationsModelDiffer {
        private DbContext _dbContext;

        public CustomMigrationsModelDiffer(
            IRelationalTypeMappingSource typeMappingSource,
            IMigrationsAnnotationProvider migrationsAnnotationProvider,
            IRelationalAnnotationProvider changeDetector,
            IRowIdentityMapFactory rowUpdateAdapterFactory,
            CommandBatchPreparerDependencies dependencies,
            DbContext dbContext = null)
            : base(typeMappingSource, migrationsAnnotationProvider, changeDetector, rowUpdateAdapterFactory, dependencies) {
            _dbContext = dbContext;
        }

        protected override IEnumerable<MigrationOperation> Diff(
            IRelationalModel? source,
            IRelationalModel? target,
            DiffContext diffContext) {
            //Debugger.Launch();

            if (_dbContext == null)
            {
                _dbContext = DbContextHolder.DbContext;
            }

            // Don't run for trigger generation script
            if (source == null && _dbContext.GetType().Name == "DatabaseContext" || target == null)
            {
                return base.Diff(source, target, diffContext);
            }

            var operations = base.Diff(source, target, diffContext).ToList();
            var baseOperationsCount = operations.Count;

            // Run a single query to get all PHI extended properties from the database
            var existingExtendedPropertiesInDatabase = Utilities.GetAllExtendedPropertiesFromDatabase(_dbContext, "PHI");
            Utilities.Log($"D.{nameof(Diff)} - Extended properties in database: {existingExtendedPropertiesInDatabase.Count}");

            foreach (var table in target.Model?.GetEntityTypes())
            {
                var assembly = LoadAssembly(table);

                foreach (var targetProperty in table.GetProperties())
                {
                    var sourceProperty = source?.Model?.FindEntityType(table.Name)?.FindProperty(targetProperty.Name);
                    sourceProperty ??= diffContext.FindSource(targetProperty); // This doesn't seem to ever find anything...

                    var sourceShortTableName = sourceProperty?.DeclaringType.Name.Split('.').Last();
                    var sourceExtendedProperties = existingExtendedPropertiesInDatabase.Where(ep => ep.TableName == sourceShortTableName && ep.ColumnName == sourceProperty?.Name);
                    var targetCustomAttributes = Utilities.GetPropertyCustomAttributesFromAssembly(table.Name, targetProperty.Name, assembly);

                    // If source already had EPs, compare them to the target's custom attributes
                    if (sourceExtendedProperties.Any())
                    {
                        ProcessExistingExtendedProperties(operations, table, targetProperty, sourceProperty, sourceExtendedProperties, targetCustomAttributes);
                    }
                    else
                    { // If source didn't have EPs, check if target has any custom attributes to add
                        ProcessNewExtendedProperties(operations, table, targetProperty, targetCustomAttributes);
                    }
                }
            }

            // Remove any extended properties that were never compared, like removed columns
            foreach (var ep in existingExtendedPropertiesInDatabase.Where(ep => !ep.HasBeenCompared))
            {
                Utilities.Log($"D.{nameof(Diff)} - Creating RemoveExtendedPropertyOperation for {ep.TableName}.{ep.ColumnName} with annotation PHI Remove");
                var removeExtendedPropertyOperation = new RemoveExtendedPropertyOperation(new SchemaTableColumn("dbo", ep.TableName, ep.ColumnName), new ExtendedProperty(ep.ExtendedPropertyName, ep.ExtendedPropertyValue));
                removeExtendedPropertyOperation.AddAnnotation(ep.ExtendedPropertyName, "Remove");
                operations.Add(removeExtendedPropertyOperation);
            }

            Utilities.Log($"D.{nameof(Diff)} - New operations: {operations.Count - baseOperationsCount}/{operations.Count}");
            return operations;
        }

        private Assembly LoadAssembly(IEntityType table) {
            var tableNameParts = table.Name.Split('.');
            for (int i = tableNameParts.Length; i > 0; i--)
            {
                var assemblyName = string.Join('.', tableNameParts.Take(i));
                try
                {
                    return Assembly.Load(assemblyName);
                }
                catch
                {
                    // Ignore and try the next combination
                }
            }
            throw new InvalidOperationException($"Unable to load assembly for table {table.Name}");
        }

        private void ProcessNewExtendedProperties(List<MigrationOperation> operations, IEntityType entityType, IProperty targetProperty, IEnumerable<string> targetCustomAttributes) {
            foreach (var dictionaryKey in Config.AttributeToExtendedPropertyMap.Keys)
            {
                var dictionaryExtendedProperty = Config.AttributeToExtendedPropertyMap[dictionaryKey];
                if (targetCustomAttributes.Contains(dictionaryKey))
                {
                    Utilities.Log($"D.{nameof(ProcessNewExtendedProperties)} - Creating AddExtendedPropertyOperation for {entityType.GetTableName()}.{targetProperty.Name} with annotation PHI Add");
                    var addExtendedPropertyOperation = new AddExtendedPropertyOperation(new SchemaTableColumn(entityType.GetSchema(), entityType.GetTableName(), targetProperty.Name), Config.AttributeToExtendedPropertyMap[dictionaryKey]);
                    addExtendedPropertyOperation.AddAnnotation(dictionaryExtendedProperty.Key, "Add");
                    operations.Add(addExtendedPropertyOperation);
                    return;
                }
            }
        }

        private static void ProcessExistingExtendedProperties(List<MigrationOperation> operations, IEntityType entityType, IProperty targetProperty, IProperty sourceProperty, IEnumerable<ExtendedPropertyResult> sourceExtendedProperties, IEnumerable<string> targetCustomAttributes) {
            foreach (var dictionaryKey in Config.AttributeToExtendedPropertyMap.Keys)
            {
                var dictionaryExtendedProperty = Config.AttributeToExtendedPropertyMap[dictionaryKey];
                var found = sourceExtendedProperties.FirstOrDefault(ep => ep.ExtendedPropertyName == dictionaryExtendedProperty.Key && ep.ExtendedPropertyValue == dictionaryExtendedProperty.Value);
                if (found != null && !found.HasBeenCompared)
                {
                    // Mark as compared to avoid duplicate operations and so we can see any that were never compared at the end
                    found.HasBeenCompared = true;

                    if (targetCustomAttributes.Contains(dictionaryKey))
                    {
                        // If the source and target have the same EP, but property was renamed, 
                        // we'll have to remove the old EP and add a new one with the new name
                        if (sourceProperty.Name != targetProperty.Name)
                        {
                            // Remove old EP
                            Utilities.Log($"D.{nameof(ProcessExistingExtendedProperties)} - Creating RemoveExtendedPropertyOperation for {entityType.GetTableName()}.{targetProperty.Name} with annotation PHI Rename");
                            var removeExtendedPropertyOperation = new RemoveExtendedPropertyOperation(new SchemaTableColumn(entityType.GetSchema(), entityType.GetTableName(), targetProperty.Name), dictionaryExtendedProperty);
                            removeExtendedPropertyOperation.AddAnnotation(dictionaryExtendedProperty.Key, "Remove");
                            operations.Add(removeExtendedPropertyOperation);

                            // Add new EP
                            Utilities.Log($"D.{nameof(ProcessExistingExtendedProperties)} - Creating AddExtendedPropertyOperation for {entityType.GetTableName()}.{sourceProperty.Name} with annotation PHI Rename");
                            var addExtendedPropertyOperation = new AddExtendedPropertyOperation(new SchemaTableColumn(entityType.GetSchema(), entityType.GetTableName(), sourceProperty.Name), dictionaryExtendedProperty);
                            addExtendedPropertyOperation.AddAnnotation(dictionaryExtendedProperty.Key, "Add");
                            operations.Add(addExtendedPropertyOperation);
                            return;
                        }
                    }
                    else
                    { // If source has EP, but target doesn't have attribute, we'll remove the EP
                        Utilities.Log($"D.{nameof(ProcessExistingExtendedProperties)} - Creating RemoveExtendedPropertyOperation for {entityType.GetTableName()}.{targetProperty.Name} with annotation PHI Remove");
                        var removeExtendedPropertyOperation = new RemoveExtendedPropertyOperation(new SchemaTableColumn(entityType.GetSchema(), entityType.GetTableName(), targetProperty.Name), dictionaryExtendedProperty);
                        removeExtendedPropertyOperation.AddAnnotation(dictionaryExtendedProperty.Key, "Remove");
                        operations.Add(removeExtendedPropertyOperation);
                        return;
                    }
                }
            }
        }
    }
}
