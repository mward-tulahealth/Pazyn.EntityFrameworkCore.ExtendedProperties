using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public static class ModelExtensions
    {
        public static IMutableAnnotatable HasExtendedProperty(IMutableAnnotatable annotatable, string key, string value)
        {
            var annotation = annotatable.FindAnnotation(nameof(ExtendedProperty)) ?? new Annotation(nameof(ExtendedProperty), "{}");
            var annotationValues = JsonSerializer.Deserialize<Dictionary<string, string>>(annotation.Value as string);

            annotationValues.Add(key, value);

            annotatable.SetAnnotation(nameof(ExtendedProperty), JsonSerializer.Serialize(annotationValues));
            return annotatable;
        }

        public static EntityTypeBuilder HasExtendedProperty(this EntityTypeBuilder entityTypeBuilder, string key, string value)
        {
            var mutableEntityType = entityTypeBuilder.Metadata;
            HasExtendedProperty(mutableEntityType, key, value);
            return entityTypeBuilder;
        }

        public static PropertyBuilder HasExtendedProperty(this PropertyBuilder propertyBuilder, string key, string value)
        {
            var mutableEntityType = propertyBuilder.Metadata;
            HasExtendedProperty(mutableEntityType, key, value);
            return propertyBuilder;
        }

        internal static ExtendedProperty[] GetExtendedProperties(this IMutableAnnotatable operation)
        {
            var annotation = operation.FindAnnotation("PHI");
            if (annotation == null)
            {
                return new ExtendedProperty[0];
            }
            
            Log($"In GetExtendedProperties: {annotation}");
            Log($"In GetExtendedProperties: {annotation?.Value}");
            var eps = new ExtendedProperty[1];
            eps[0] = new ExtendedProperty(annotation.Name, annotation.Value as string);
            return eps;
        }

        private static void Log(string message)
        {
            var logFilePath = "migration_log.txt";
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
    }
}
