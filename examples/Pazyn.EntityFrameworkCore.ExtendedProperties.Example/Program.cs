using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Example
{
    public class Entity
    {
        public Int32 EntityId { get; set; }
        public Int32 Name { get; set; }
    }

    public class ExampleDbContext : DbContext
    {
        public DbSet<Entity> Entities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=Pazyn.EntityFrameworkCore.ExtendedProperties;Integrated Security=True");
            optionsBuilder.AddExtendedProperties();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entityTypeBuilder = modelBuilder.Entity<Entity>();
            var propertyBuilder = entityTypeBuilder.Property(x => x.Name);

            entityTypeBuilder.HasExtendedProperty("Key1", nameof(EntityTypeBuilder));
            propertyBuilder.HasExtendedProperty("Key2", nameof(PropertyBuilder));
        }
    }

    class Program
    {
        static void Main(String[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}