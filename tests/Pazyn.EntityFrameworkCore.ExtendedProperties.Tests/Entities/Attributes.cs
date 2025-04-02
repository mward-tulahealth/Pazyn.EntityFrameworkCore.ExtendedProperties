using System;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Tests.Entities {
    [AttributeUsage(AttributeTargets.Property)]
    public class PHIAttribute : ExtendedPropertyAttribute {
        public PHIAttribute() : base("PHI", "true") {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PIIAttribute : ExtendedPropertyAttribute {
        public PIIAttribute() : base("PII", "true") {
        }
    }
}
