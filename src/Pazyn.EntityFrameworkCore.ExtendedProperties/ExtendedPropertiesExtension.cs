using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    internal class ExtendedPropertiesExtension : IDbContextOptionsExtension
    {
        public void ApplyServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Decorate<IMigrationsAnnotationProvider, ExtendedPropertiesAnnotationProvider>();
        }

        public void Validate(IDbContextOptions options)
        {
        }

        public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            public override bool IsDatabaseProvider => false;

            public override int GetServiceProviderHashCode() => 0;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) =>
                debugInfo[$"Pazyn: {nameof(ExtendedPropertiesExtension)}"] = "1";

            public override string LogFragment => $"using {nameof(ExtendedPropertiesExtension)}";

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;
        }
    }
}