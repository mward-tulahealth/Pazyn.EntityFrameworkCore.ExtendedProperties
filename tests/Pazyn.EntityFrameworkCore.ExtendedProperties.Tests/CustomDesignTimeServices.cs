using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Tests {
    public class CustomDesignTimeServices : IDesignTimeServices {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection) {
            serviceCollection.AddSingleton<IMigrationsCodeGenerator, CustomCSharpMigrationsGenerator>();
            serviceCollection.AddSingleton<ICSharpMigrationOperationGenerator, CustomCSharpMigrationOperationGenerator>();
        }
    }
}
