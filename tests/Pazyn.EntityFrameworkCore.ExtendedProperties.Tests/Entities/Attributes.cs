using System;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Tests.Entities {
    [AttributeUsage(AttributeTargets.Property)]
    public class PHIAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PIIAttribute : Attribute {
    }
}
