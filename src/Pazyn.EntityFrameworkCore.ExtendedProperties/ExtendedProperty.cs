namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public class ExtendedProperty
    {
        public string Key { get; }
        public string Value { get; }

        public ExtendedProperty(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
