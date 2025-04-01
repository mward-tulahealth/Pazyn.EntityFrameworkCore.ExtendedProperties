using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Tests.Entities;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Tests {
    // InitialTestDbContext

    // Do not modify this file in `TestDbContext.cs` file, only in `InitialTestDbContext.txt` template file.
    public class TestDbContext : DbContext {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<ExistingTable>(entity =>
            {
                entity.ToTable("ExistingTable");
            });
            
            modelBuilder.Entity<TableToBeRemoved>(entity =>
            {
                entity.ToTable("TableToBeRemoved");
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder
                .UseSqlServer(b => b.MigrationsAssembly(typeof(TestDbContext).Assembly.FullName))
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            optionsBuilder.ReplaceService<IMigrationsModelDiffer, CustomMigrationsModelDiffer>();
            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, ExtendedPropertiesMigrationsSqlGenerator>();
            optionsBuilder.ReplaceService<ICSharpMigrationOperationGenerator, CustomCSharpMigrationOperationGenerator>();
            optionsBuilder.ReplaceService<SqlServerMigrationsSqlGenerator, ExtendedPropertiesMigrationsSqlGenerator>();
        }

        public DbSet<ExistingTable> ExistingTable { get; set; }
        public DbSet<TableToBeRemoved> TableToBeRemoved { get; set; }
    }
}
