using System;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    internal class ExtendedProperty
    {
        public String Key { get; }
        public String Value { get; }

        public ExtendedProperty(String key, String value)
        {
            Key = key;
            Value = value;
        }
    }
}