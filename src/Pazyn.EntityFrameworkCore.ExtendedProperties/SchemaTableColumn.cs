using System;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    internal class SchemaTableColumn
    {
        public String Schema { get; }
        public String Table { get; }
        public String Column { get; }

        public SchemaTableColumn(String schema, String table, String column)
        {
            Schema = schema;
            Table = table;
            Column = column;
        }
    }
}