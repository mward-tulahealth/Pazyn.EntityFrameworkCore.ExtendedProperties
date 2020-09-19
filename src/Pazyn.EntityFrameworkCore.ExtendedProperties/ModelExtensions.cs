using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public static class ModelExtensions
    {
        public static IMutableAnnotatable HasExtendedProperty(IMutableAnnotatable annotatable, String key, String value)
        {
            var annotation = annotatable.FindAnnotation(nameof(ExtendedProperty)) ?? new Annotation(nameof(ExtendedProperty), "{}");
            var annotationValues = JsonSerializer.Deserialize<Dictionary<String, String>>(annotation.Value as String);

            annotationValues.Add(key, value);

            annotatable.SetAnnotation(nameof(ExtendedProperty), JsonSerializer.Serialize(annotationValues));
            return annotatable;
        }

        public static EntityTypeBuilder HasExtendedProperty(this EntityTypeBuilder entityTypeBuilder, String key, String value)
        {
            var mutableEntityType = entityTypeBuilder.Metadata;
            HasExtendedProperty(mutableEntityType, key, value);
            return entityTypeBuilder;
        }

        public static PropertyBuilder HasExtendedProperty(this PropertyBuilder propertyBuilder, String key, String value)
        {
            var mutableEntityType = propertyBuilder.Metadata;
            HasExtendedProperty(mutableEntityType, key, value);
            return propertyBuilder;
        }

        internal static ExtendedProperty[] GetExtendedProperties(this IMutableAnnotatable operation)
        {
            var annotation = operation.FindAnnotation(nameof(ExtendedProperty));
            if (annotation == null)
            {
                return new ExtendedProperty[0];
            }
            var annotationValues = JsonSerializer.Deserialize<Dictionary<String, String>>(annotation.Value as String);
            return annotationValues.Select(x => new ExtendedProperty(x.Key, x.Value)).ToArray();
        }
    }
}