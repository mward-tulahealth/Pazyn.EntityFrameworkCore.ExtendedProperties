using System.Collections.Generic;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties {
    public class Config {
        // Dictionary mapping attribute name to the corresponding extended property key/value tuple for custom
        // attributes that should be converted to extended properties
        public static Dictionary<string, ExtendedProperty> AttributeToExtendedPropertyMap = new()
        {
            { "PHIAttribute", new ExtendedProperty("PHI", "true") },
            { "ToRemoveAttribute", new ExtendedProperty("ToRemove", "1") } // This one shouldn't be used, but it's here for testing purposes
        };
    }
}
