using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties {
    public static class Utilities {
        private static readonly ILogger Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<CustomMigrationsModelDiffer>();
        public static IEnumerable<ExtendedPropertyAttribute> GetPropertyCustomAttributesFromAssembly(string fullTableName, string propertyName, Assembly assembly) {
            var shortTableName = fullTableName.Split('.').Last();

            var entityType = assembly.GetTypes().FirstOrDefault(t => t.Name == shortTableName);
            if (entityType != null) {
                var property = entityType.GetProperty(propertyName);
                if (property != null) {
                    var customAttributes = property.GetCustomAttributes();

                    var extendedPropertyAttributes = customAttributes
                        .Where(a => typeof(ExtendedPropertyAttribute).IsAssignableFrom(a.GetType()))
                        .Select(a => (ExtendedPropertyAttribute)a);

                    foreach (var attr in extendedPropertyAttributes) {
                        Logger.LogInformation($"{shortTableName}.{propertyName} has attribute: {attr}");
                    }

                    return extendedPropertyAttributes;
                }
                else {
                    var skipNames = new List<string> { "CreatedSubjectId", "LastModifiedSubjectId" };
                    if (!skipNames.Contains(propertyName)) {
                        Logger.LogDebug($"Property is null: {propertyName}");
                    }
                }
            }
            else {
                var skipNames = new List<string> { "Subject", "Outbox" };
                if (!skipNames.Contains(shortTableName)) {
                    Logger.LogDebug($"EntityType is null: {shortTableName}");
                }
            }

            return [];
        }

        public static List<ExtendedPropertyResult> GetAllExtendedPropertiesFromDatabase(DbContext dbContext) {
            var results = new List<ExtendedPropertyResult>();
            try {
                var connection = dbContext.Database.GetDbConnection() as SqlConnection;
                Logger.LogDebug($"{connection.ConnectionString}");

                if (connection.State != System.Data.ConnectionState.Open) {
                    connection.Open();
                }

                var command = connection.CreateCommand();

                command.CommandText = $@"
                    SELECT tbl.TABLE_NAME, col.COLUMN_NAME, hprop.name AS ExtendedPropertyName, hprop.value AS ExtendedPropertyValue
                    FROM INFORMATION_SCHEMA.TABLES AS tbl
                    INNER JOIN INFORMATION_SCHEMA.COLUMNS AS col ON col.TABLE_NAME = tbl.TABLE_NAME AND col.TABLE_SCHEMA = tbl.TABLE_SCHEMA
                    INNER JOIN sys.columns AS sc ON sc.object_id = OBJECT_ID(tbl.TABLE_SCHEMA + '.' + tbl.TABLE_NAME) AND sc.name = col.COLUMN_NAME
                    LEFT JOIN sys.extended_properties hprop ON hprop.major_id = sc.object_id AND hprop.minor_id = sc.column_id
                    WHERE tbl.TABLE_SCHEMA = 'dbo' AND hprop.name IS NOT NULL AND hprop.name <> 'MS_Description' AND hprop.value IS NOT NULL";

                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        results.Add(new ExtendedPropertyResult {
                            TableName = reader.GetString(0),
                            ColumnName = reader.GetString(1),
                            ExtendedPropertyName = reader.GetString(2),
                            ExtendedPropertyValue = reader.GetString(3)
                        });
                    }
                }
            }
            catch (Exception e) {
                Logger.LogError($"Error getting extended properties: {e.Message}");
            }

            return results;
        }
    }
}
