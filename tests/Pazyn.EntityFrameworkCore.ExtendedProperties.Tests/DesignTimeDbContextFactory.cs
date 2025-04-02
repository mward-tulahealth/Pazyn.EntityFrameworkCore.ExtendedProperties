using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.Configuration;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Tests {
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TestDbContext> {
        public TestDbContext CreateDbContext(string[] args) {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("testsettings.json")
                .Build();

            string connectionString = configuration
                .GetSection("Database:ConnectionString").Value;

            var builder = new DbContextOptionsBuilder<TestDbContext>();
            builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(typeof(TestDbContext).Assembly.FullName));

            // Replace default services with custom implementations
            builder.ReplaceService<IMigrationsModelDiffer, CustomMigrationsModelDiffer>();
            builder.ReplaceService<IMigrationsSqlGenerator, ExtendedPropertiesMigrationsSqlGenerator>();
            builder.ReplaceService<ICSharpMigrationOperationGenerator, CustomCSharpMigrationOperationGenerator>();
            builder.ReplaceService<SqlServerMigrationsSqlGenerator, ExtendedPropertiesMigrationsSqlGenerator>();

            var context = new TestDbContext(builder.Options);

            // Apply migrations and update the database
            context.Database.Migrate();

            return context;
        }
    }
}
