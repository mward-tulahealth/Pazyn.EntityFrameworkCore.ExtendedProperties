namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Entities {
    public class ExtendedPropertyResult {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ExtendedPropertyName { get; set; }
        public string ExtendedPropertyValue { get; set; }
        public bool HasBeenCompared { get; set; } = false;
    }
}
