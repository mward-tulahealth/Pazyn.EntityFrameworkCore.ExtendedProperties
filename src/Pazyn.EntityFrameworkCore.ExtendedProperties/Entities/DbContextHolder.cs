using Microsoft.EntityFrameworkCore;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Entities {
    /// <summary>
    /// This class holds the current DbContext instance.
    /// It is used to access the DbContext from `CustomMigrationsModelDiffer.cs` which isn't passed a DbContext instance.
    /// </summary>
    public static class DbContextHolder {
        public static DbContext DbContext { get; set; }
    }
}
