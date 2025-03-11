using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public static class Utilities {
        public static void Log(string message)
        {
            var logFilePath = "migration_log.txt";
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }

        public static IEnumerable<string> GetPropertyCustomAttributesFromAssembly(string fullTableName, string propertyName)
        {
            var shortTableName = fullTableName.Split('.').Last();
            // Log($"U.{nameof(GetPropertyCustomAttributesFromAssembly)} - Table.Name: {fullTableName}.{propertyName}");

            // We have to load the assembly to get attributes (the target model doesn't have attribute info)
            var assembly = Assembly.Load("LiveTula.Gateway.Domain");

            var entityType = assembly.GetTypes().FirstOrDefault(t => t.Name == shortTableName);
            if (entityType != null)
            {
                var property = entityType.GetProperty(propertyName);
                if (property != null)
                {
                    var customAttributes = property.GetCustomAttributes();
                    
                    // Get attribute names for now, could get actual attributes later if needed
                    var results = customAttributes
                        .Where(a => Config.AttributeToExtendedPropertyMap.ContainsKey(a.GetType().Name))
                        .Select(a => a.GetType().Name);

                    foreach (var attr in results) {
                        Log($"U.{nameof(GetPropertyCustomAttributesFromAssembly)} - {shortTableName}.{propertyName} has attribute: {attr}");
                    }

                    return results;
                }
                else {
                    var skipNames = new List<string> { "CreatedSubjectId", "LastModifiedSubjectId" };
                    if (!skipNames.Contains(propertyName)) {
                        Log($"U.{nameof(GetPropertyCustomAttributesFromAssembly)} - Property is null: {propertyName}");
                    }
                }
            }
            else {
                var skipNames = new List<string> { "Subject", "Outbox" };
                if (!skipNames.Contains(shortTableName)) {
                    Log($"U.{nameof(GetPropertyCustomAttributesFromAssembly)} - EntityType is null: {shortTableName}");
                }
            }
            
            return [];
        }

        public static List<ExtendedPropertyResult> GetAllExtendedPropertiesFromDatabase(DbContext dbContext, string extendedPropertyName = "PHI") {
            // column = "Email"; //for testing, TODO: remove this line
            var results = new List<ExtendedPropertyResult>();
            try {
                if (dbContext == null) {
                    dbContext = DbContextHolder.DbContext;
                    // Log($"U.{nameof(GetAllExtendedPropertiesFromDatabase)} - DbContext from holder: {dbContext}");
                }
                var connection = dbContext.Database.GetDbConnection() as SqlConnection;
                // Log($"U.{nameof(GetAllExtendedPropertiesFromDatabase)} - {connection.ConnectionString}");

                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                var command = connection.CreateCommand();

                command.CommandText = $@"
                    SELECT tbl.TABLE_NAME, col.COLUMN_NAME, hprop.name AS ExtendedPropertyName, hprop.value AS ExtendedPropertyValue
                    FROM INFORMATION_SCHEMA.TABLES AS tbl
                    INNER JOIN INFORMATION_SCHEMA.COLUMNS AS col ON col.TABLE_NAME = tbl.TABLE_NAME AND col.TABLE_SCHEMA = tbl.TABLE_SCHEMA
                    INNER JOIN sys.columns AS sc ON sc.object_id = OBJECT_ID(tbl.TABLE_SCHEMA + '.' + tbl.TABLE_NAME) AND sc.name = col.COLUMN_NAME
                    LEFT JOIN sys.extended_properties hprop ON hprop.major_id = sc.object_id AND hprop.minor_id = sc.column_id AND hprop.name = '{extendedPropertyName}'
                    WHERE tbl.TABLE_SCHEMA = 'dbo' AND hprop.name IS NOT NULL AND hprop.value IS NOT NULL";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new ExtendedPropertyResult
                        {
                            TableName = reader.GetString(0),
                            ColumnName = reader.GetString(1),
                            ExtendedPropertyName = reader.GetString(2),
                            ExtendedPropertyValue = reader.GetString(3)
                        });
                    }
                }
            }
            catch (Exception e) {
                Log($"U.{nameof(GetAllExtendedPropertiesFromDatabase)} - Error getting extended properties: {e.Message}");
            }

            return results;
        }
    }

    public class ExtendedPropertyResult {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ExtendedPropertyName { get; set; }
        public string ExtendedPropertyValue { get; set; }
    }

    public static class DbContextHolder {
        public static DbContext DbContext { get; set; }
    }
}
