using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.Logging;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Entities;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Operations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties {
    public class CustomMigrationsModelDiffer : MigrationsModelDiffer {
        private DbContext _dbContext;
        private static readonly ILogger Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<CustomMigrationsModelDiffer>();

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
            // Debugger.Launch();

            if (_dbContext == null) {
                if (DbContextHolder.DbContext == null) {
                    Logger.LogError("DbContextHolder.DbContext is null. Please set the DbContextHolder.DbContext before using CustomMigrationsModelDiffer.");
                    throw new InvalidOperationException("DbContextHolder.DbContext is null. Please set the DbContextHolder.DbContext before using CustomMigrationsModelDiffer.");
                }
                _dbContext = DbContextHolder.DbContext;
            }

            if (source == null && _dbContext.GetType().Name == "DatabaseContext" || target == null) {
                Logger.LogDebug("Skipping extended properties for trigger generation script");
                return base.Diff(source, target, diffContext);
            }

            var operations = base.Diff(source, target, diffContext).ToList();
            var baseOperationsCount = operations.Count;

            if (baseOperationsCount == 1 && operations[0].ToString().Contains("CreateTable")) {
                Logger.LogDebug("Skipping extended properties for CreateTable operation");
                return operations;
            }

            // If we have already generated operations for Up and are now generating operations for Down,
            // we will use a cached copy of the operations reversed for Down instead of trying to generate them
            // since generating them will cause issues with the source/target swapped.
            var operationsCount = UpOperationsHolder.AddExtendedPropertyOperations.Count + UpOperationsHolder.RemoveExtendedPropertyOperations.Count;
            if (operationsCount > 0) {
                Logger.LogInformation($"Using reversed cached operations from `Up` for `Down`: {operationsCount}");
                operations.AddRange(UpOperationsHolder.GetDownOperations());
                UpOperationsHolder.Clear();
                return operations;
            }

            // Run a single query to get all custom extended properties from the database
            var existingExtendedPropertiesInDatabase = Utilities.GetAllExtendedPropertiesFromDatabase(_dbContext);
            Logger.LogInformation($"Existing extended properties in database: {existingExtendedPropertiesInDatabase.Count}");

            foreach (var table in target.Model?.GetEntityTypes()) {
                var assembly = LoadAssembly(table);

                foreach (var targetProperty in table.GetProperties()) {
                    var sourceProperty = source?.Model?.FindEntityType(table.Name)?.FindProperty(targetProperty.Name);
                    sourceProperty ??= diffContext.FindSource(targetProperty); // This doesn't seem to ever find anything...

                    var sourceShortTableName = sourceProperty?.DeclaringType.Name.Split('.').Last();
                    var sourceExtendedProperties = existingExtendedPropertiesInDatabase.Where(ep => ep.TableName == sourceShortTableName && ep.ColumnName == sourceProperty?.Name);
                    var targetCustomAttributes = Utilities.GetPropertyCustomAttributesFromAssembly(table.Name, targetProperty.Name, assembly);

                    // If source already had EPs, compare them to the target's custom attributes
                    if (sourceExtendedProperties.Any()) {
                        ProcessExistingExtendedProperties(operations, table, targetProperty, sourceProperty, sourceExtendedProperties, targetCustomAttributes);
                    }
                    else { // If source didn't have EPs, check if target has any custom attributes to add
                        ProcessNewExtendedProperties(operations, table, targetProperty, targetCustomAttributes);
                    }
                }
            }

            // Remove any extended properties that were never compared, like removed columns
            foreach (var ep in existingExtendedPropertiesInDatabase.Where(ep => !ep.HasBeenCompared)) {
                Logger.LogInformation($"Creating RemoveExtendedPropertyOperation for {ep.TableName}.{ep.ColumnName} for EP key '{ep.ExtendedPropertyName}'");
                var removeExtendedPropertyOperation = new RemoveExtendedPropertyOperation(new SchemaTableColumn("dbo", ep.TableName, ep.ColumnName), ep.ExtendedPropertyName);
                operations.Add(removeExtendedPropertyOperation);
            }

            // Cache Up operations to reverse for Down
            UpOperationsHolder.AddExtendedPropertyOperations.AddRange(operations.OfType<AddExtendedPropertyOperation>());
            UpOperationsHolder.RemoveExtendedPropertyOperations.AddRange(operations.OfType<RemoveExtendedPropertyOperation>());

            Logger.LogDebug($"New operations: {operations.Count - baseOperationsCount}/{operations.Count}");
            return operations;
        }

        private Assembly LoadAssembly(IEntityType table) {
            var tableNameParts = table.Name.Split('.');
            
            // Try to load the assembly using the full name first, 
            // removing another split from the end each time until we find a matching assembly
            for (int i = tableNameParts.Length; i > 0; i--) {
                var assemblyName = string.Join('.', tableNameParts.Take(i));
                try {
                    return Assembly.Load(assemblyName);
                }
                catch {
                    // Ignore and try the next combination
                }
            }
            throw new InvalidOperationException($"Unable to load assembly for table {table.Name}");
        }

        private void ProcessNewExtendedProperties(List<MigrationOperation> operations, IEntityType entityType, 
        IProperty targetProperty, IEnumerable<ExtendedPropertyAttribute> targetCustomAttributes) {
            foreach (var targetCustomAttribute in targetCustomAttributes) {
                Logger.LogInformation($"Creating AddExtendedPropertyOperation for {entityType.GetTableName()}.{targetProperty.Name} for EP key '{targetCustomAttribute.Name}'");
                var addEpOperation = new AddExtendedPropertyOperation(new SchemaTableColumn(entityType.GetSchema(), entityType.GetTableName(), targetProperty.Name), new ExtendedProperty(targetCustomAttribute.Name, targetCustomAttribute.Value));
                operations.Add(addEpOperation);
            }
        }

        private static void ProcessExistingExtendedProperties(List<MigrationOperation> operations, IEntityType entityType, IProperty targetProperty, IProperty sourceProperty, IEnumerable<ExtendedPropertyResult> sourceExtendedProperties, IEnumerable<ExtendedPropertyAttribute> targetCustomAttributes) {
            foreach (var targetCustomAttribute in targetCustomAttributes) {
                var matchingEP = sourceExtendedProperties.FirstOrDefault(ep => ep.ExtendedPropertyName == targetCustomAttribute.Name && ep.ExtendedPropertyValue == targetCustomAttribute.Value);
                if (matchingEP != null && !matchingEP.HasBeenCompared) {
                    // Mark as compared to avoid duplicate operations and so we can see any that were never compared at the end
                    matchingEP.HasBeenCompared = true;
                    // If the source and target have the same EP, but property was renamed, 
                    // we'll have to remove the old EP and add a new one with the new name
                    if (sourceProperty.Name != targetProperty.Name) {
                        // Remove old EP
                        Logger.LogInformation($"Creating RemoveExtendedPropertyOperation for {entityType.GetTableName()}.{targetProperty.Name} for EP key '{targetCustomAttribute.Name}'");
                        var removeEpOperation = new RemoveExtendedPropertyOperation(new SchemaTableColumn(entityType.GetSchema(), entityType.GetTableName(), targetProperty.Name), targetCustomAttribute.Name);
                        operations.Add(removeEpOperation);

                        // Add new EP
                        Logger.LogInformation($"Creating AddExtendedPropertyOperation for {entityType.GetTableName()}.{sourceProperty.Name} for EP key '{targetCustomAttribute.Name}'");
                        var addEpOperation = new AddExtendedPropertyOperation(new SchemaTableColumn(entityType.GetSchema(), entityType.GetTableName(), sourceProperty.Name), new
                        ExtendedProperty(targetCustomAttribute.Name, targetCustomAttribute.Value));
                        operations.Add(addEpOperation);
                    }
                }
                else { // If target has attribute, but source doesn't have matching EP, we'll add the EP
                    Logger.LogInformation($"Creating AddExtendedPropertyOperation for {entityType.GetTableName()}.{targetProperty.Name} for EP key '{targetCustomAttribute.Name}'");
                    var addEpOperation = new AddExtendedPropertyOperation(new SchemaTableColumn(entityType.GetSchema(), entityType.GetTableName(), targetProperty.Name), new ExtendedProperty(targetCustomAttribute.Name, targetCustomAttribute.Value));
                    operations.Add(addEpOperation);
                }
            }
        }
    }
}
