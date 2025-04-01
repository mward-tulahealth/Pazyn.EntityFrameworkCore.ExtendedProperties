using System;
using System.Collections.Generic;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Entities;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties {
    [AttributeUsage(AttributeTargets.Property)]
    // This attribute is used to add extended properties to the column in the database.
    public class ExtendedPropertyAttribute : Attribute {
        public string Name { get; }
        public string Value { get; }
        private static Dictionary<string, ExtendedProperty> AllExtendedPropertiesDictionary { get; } = new Dictionary<string, ExtendedProperty>();

        public ExtendedPropertyAttribute(string name, string value) {
            Name = name;
            Value = value;
            var type = this.GetType().ToString();
            AllExtendedPropertiesDictionary.TryAdd(type, new ExtendedProperty(name, value));
        }

        public static Dictionary<string, ExtendedProperty> GetAllExtendedPropertiesDictionary() {
            return AllExtendedPropertiesDictionary;
        }
    }
}
