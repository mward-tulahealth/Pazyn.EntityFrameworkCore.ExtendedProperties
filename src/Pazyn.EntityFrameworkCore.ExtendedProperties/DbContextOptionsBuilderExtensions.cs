using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder AddExtendedProperties(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, ExtendedPropertiesMigrationsSqlGenerator>();

            var extension = optionsBuilder.Options.FindExtension<ExtendedPropertiesExtension>() ?? new ExtendedPropertiesExtension();
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            return optionsBuilder;
        }
    }
}