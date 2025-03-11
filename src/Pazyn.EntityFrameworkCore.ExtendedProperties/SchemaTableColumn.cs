namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public class SchemaTableColumn
    {
        public string Schema { get; }
        public string Table { get; }
        public string Column { get; }

        public SchemaTableColumn(string schema, string table, string column)
        {
            Schema = schema;
            Table = table;
            Column = column;
        }
    }
}
