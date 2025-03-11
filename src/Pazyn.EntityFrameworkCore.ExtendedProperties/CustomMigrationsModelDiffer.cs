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
using System.Diagnostics;
using System.Linq;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
	public class CustomMigrationsModelDiffer : MigrationsModelDiffer {
		private readonly DbContext _dbContext;

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
			var operations = base.Diff(source, target, diffContext).ToList();
			var baseOperationsCount = operations.Count;

			// Run a single query to get all PHI extended properties from the database
			var existingExtendedPropertiesInDatabase = Utilities.GetAllExtendedPropertiesFromDatabase(_dbContext, "PHI");
			Utilities.Log($"D.{nameof(Diff)} - Extended properties in database: {existingExtendedPropertiesInDatabase.Count}");

			foreach (var table in target.Model.GetEntityTypes()) {
				foreach (var targetProperty in table.GetProperties()) {
					var sourceProperty = source?.Model?.FindEntityType(table.Name)?.FindProperty(targetProperty.Name);
					sourceProperty ??= diffContext.FindSource(targetProperty);
					sourceProperty ??= targetProperty;

					var sourceShortTableName = sourceProperty?.DeclaringType.Name.Split('.').Last();
					var sourceExtendedProperties = existingExtendedPropertiesInDatabase.Where(ep => ep.TableName == sourceShortTableName && ep.ColumnName == sourceProperty?.Name);
					var targetCustomAttributes = Utilities.GetPropertyCustomAttributesFromAssembly(table.Name, targetProperty.Name);

					// If source already had EPs, compare them to the target's custom attributes
					if (sourceExtendedProperties.Any()) {
						ProcessExistingExtendedProperties(operations, table, targetProperty, sourceProperty, sourceExtendedProperties, targetCustomAttributes);
					}
					else { // If source didn't have EPs, check if target has any custom attributes to add
						ProcessNewExtendedProperties(operations, table, targetProperty, targetCustomAttributes);
					}
				}
			}

			Utilities.Log($"D.{nameof(Diff)} - New operations: {operations.Count - baseOperationsCount}/{operations.Count}");
			return operations;
		}

		private void ProcessNewExtendedProperties(List<MigrationOperation> operations, IEntityType entityType, IProperty targetProperty, IEnumerable<string> targetCustomAttributes) {
			foreach (var dictionaryKey in Config.AttributeToExtendedPropertyMap.Keys) {
				var dictionaryExtendedProperty = Config.AttributeToExtendedPropertyMap[dictionaryKey];
				if (targetCustomAttributes.Contains(dictionaryKey)) {
					Utilities.Log($"D.{nameof(ProcessNewExtendedProperties)} - Creating AddExtendedPropertyOperation for {entityType.GetTableName()}.{targetProperty.Name} with annotation PHI Add");
					var addExtendedPropertyOperation = new AddExtendedPropertyOperation(new SchemaTableColumn(entityType.GetSchema(), entityType.GetTableName(), targetProperty.Name), Config.AttributeToExtendedPropertyMap[dictionaryKey]);
					addExtendedPropertyOperation.AddAnnotation(dictionaryExtendedProperty.Key, "Add");
					operations.Add(addExtendedPropertyOperation);
					return;
				}
			}
		}

		private static void ProcessExistingExtendedProperties(List<MigrationOperation> operations, IEntityType entityType, IProperty targetProperty, IProperty sourceProperty, IEnumerable<ExtendedPropertyResult> sourceExtendedProperties, IEnumerable<string> targetCustomAttributes) {
			foreach (var dictionaryKey in Config.AttributeToExtendedPropertyMap.Keys) {
				var dictionaryExtendedProperty = Config.AttributeToExtendedPropertyMap[dictionaryKey];
				if (sourceExtendedProperties.Any(ep => ep.ExtendedPropertyName == dictionaryExtendedProperty.Key && ep.ExtendedPropertyValue == dictionaryExtendedProperty.Value)) {
					if (targetCustomAttributes.Contains(dictionaryKey)) {
						// If the source and target have the same EP, but property was renamed, 
						// we'll have to remove the old EP and add a new one with the new name
						if (sourceProperty.Name != targetProperty.Name) {
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
					else { // If source has EP, but target doesn't have attribute, we'll remove the EP
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
