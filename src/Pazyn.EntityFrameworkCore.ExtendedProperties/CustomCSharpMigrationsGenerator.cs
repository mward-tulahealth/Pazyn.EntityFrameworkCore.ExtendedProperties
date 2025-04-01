using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties {
    public class CustomCSharpMigrationsGenerator : CSharpMigrationsGenerator {
        public CustomCSharpMigrationsGenerator(MigrationsCodeGeneratorDependencies dependencies, CSharpMigrationsGeneratorDependencies csharpDependencies) : base(dependencies, csharpDependencies) {
        }

        protected override IEnumerable<string> GetNamespaces(IEnumerable<MigrationOperation> operations) => base.GetNamespaces(operations).Concat(new List<string> { typeof(ExtendedPropertiesExtension).Namespace });
    }
}
