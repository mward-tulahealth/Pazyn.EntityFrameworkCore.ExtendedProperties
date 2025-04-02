using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Tests.Entities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Tests {
    public class IntegrationTests : IAsyncLifetime {
        private readonly string _projectPath = "../../../";
        private readonly string _entitiesPath = "../../../Entities/";
        private readonly string _templatesPath = "../../../Templates/";
        private TestDbContext _context;
        private string snapshotOfSnapshot;

        public async Task InitializeAsync() {
            string connectionString = GetConnectionString();

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new TestDbContext(options);
            await _context.Database.EnsureDeletedAsync();

            await ResetFilesToInitialTemplatesAsync();

            await _context.Database.MigrateAsync(); // Run initial migration
            snapshotOfSnapshot = File.ReadAllText(Path.Join(_projectPath, "Migrations", "TestDbContextModelSnapshot.cs"));
        }

        public async Task DisposeAsync() {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();

            await ResetFilesToInitialTemplatesAsync();
            ResetUpdateMigrationFilesAndRevertSnapshot();
        }

        [Fact]
        public async Task TestCustomMigrationAsync() {
            // Assert initial migration ran correctly
            AssertInitialMigrationSetupEPsCorrectly();
            await AssertCanCreateRecordInExistingTableAsync();

            // Arrange
            await UpdateEntitiesByCopyingFromTemplatesAsync();

            // Act
            await RunMigrationsAddAndApplyPowershellCommand("UpdatedModelsMigration");

            // Assert
            AssertExtendedPropertiesMatchFinalState();
        }

        private string GetConnectionString() {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../")))
                .AddJsonFile("testsettings.json")
                .Build();

            string connectionString = configuration
                .GetSection("Database:ConnectionString").Value;
            return connectionString;
        }

        private void AssertExtendedPropertiesMatchFinalState() {
            var extendedProperties = Utilities.GetAllExtendedPropertiesFromDatabase(_context);

            Assert.True(_context.Database.CanConnect());
            Assert.NotNull(extendedProperties);
            // ExistingTable
            Assert.Contains(extendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "AddAttributeToExistingField" && x.ExtendedPropertyName == "PII" && x.ExtendedPropertyValue == "true");

            Assert.Contains(extendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "AddAllAttributesToExistingField" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
            Assert.Contains(extendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "AddAllAttributesToExistingField" && x.ExtendedPropertyName == "PII" && x.ExtendedPropertyValue == "true");
            Assert.Contains(extendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "AddAllAttributesToExistingField" && x.ExtendedPropertyName == "CustomEP" && x.ExtendedPropertyValue == "CustomValue");

            Assert.Contains(extendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "ExistingField_RenameExistingField_NewName" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
            Assert.Contains(extendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "ExistingField_RenameExistingField_NewName" && x.ExtendedPropertyName == "PII" && x.ExtendedPropertyValue == "true");

            // TableToBeAdded
            Assert.Contains(extendedProperties, x => x.TableName == "TableToBeAdded" && x.ColumnName == "PhiFieldToBeAdded" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");

            Assert.Contains(extendedProperties, x => x.TableName == "TableToBeAdded" && x.ColumnName == "PiiFieldToBeAdded" && x.ExtendedPropertyName == "PII" && x.ExtendedPropertyValue == "true");

            Assert.Contains(extendedProperties, x => x.TableName == "TableToBeAdded" && x.ColumnName == "AllAttributesToBeAdded" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
            Assert.Contains(extendedProperties, x => x.TableName == "TableToBeAdded" && x.ColumnName == "AllAttributesToBeAdded" && x.ExtendedPropertyName == "PII" && x.ExtendedPropertyValue == "true");
            Assert.Contains(extendedProperties, x => x.TableName == "TableToBeAdded" && x.ColumnName == "AllAttributesToBeAdded" && x.ExtendedPropertyName == "CustomEP" && x.ExtendedPropertyValue == "CustomValue");

            Assert.Equal(11, extendedProperties.Count);
        }

        private void ResetUpdateMigrationFilesAndRevertSnapshot() {
            var migrationFiles = Directory.GetFiles(Path.Join(_projectPath, "Migrations"), "*_UpdatedModelsMigration.cs");
            foreach (var file in migrationFiles) {
                File.Delete(file);
            }

            var designerFiles = Directory.GetFiles(Path.Join(_projectPath, "Migrations"), "*_UpdatedModelsMigration.Designer.cs");
            foreach (var file in designerFiles) {
                File.Delete(file);
            }

            File.WriteAllText(Path.Join(_projectPath, "Migrations", "TestDbContextModelSnapshot.cs"), snapshotOfSnapshot);
        }

        private async Task UpdateEntitiesByCopyingFromTemplatesAsync() {
            // Copy FinalStateEntities.txt to OverwritableEntities.cs
            var finalStateEntities = await File.ReadAllTextAsync(Path.Join(_templatesPath, "FinalStateEntities.txt"));
            await File.WriteAllTextAsync(Path.Join(_entitiesPath, "OverwritableEntities.cs"), finalStateEntities);

            // Copy FinalTestDbContext.txt to TestDbContext.cs
            var finalTestDbContext = await File.ReadAllTextAsync(Path.Join(_templatesPath, "FinalTestDbContext.txt"));
            await File.WriteAllTextAsync(Path.Join(_projectPath, "TestDbContext.cs"), finalTestDbContext);
        }

        private async Task ResetFilesToInitialTemplatesAsync() {
            // Copy InitialStateEntities.txt to OverwritableEntities.cs
            var initialStateEntities = await File.ReadAllTextAsync(Path.Join(_templatesPath, "InitialStateEntities.txt"));
            await File.WriteAllTextAsync(Path.Join(_entitiesPath, "OverwritableEntities.cs"), initialStateEntities);

            // Copy InitialTestDbContext.txt to TestDbContext.cs
            var initialTestDbContext = await File.ReadAllTextAsync(Path.Join(_templatesPath, "InitialTestDbContext.txt"));
            await File.WriteAllTextAsync(Path.Join(_projectPath, "TestDbContext.cs"), initialTestDbContext);
        }

        private async Task AssertCanCreateRecordInExistingTableAsync() {
            var newEntity = new ExistingTable
            {
                ExternalId = Guid.NewGuid(),
                AddAttributeToExistingField = "Test",
                AddAllAttributesToExistingField = true,
                RemovePiiAttributeFromExistingField = Guid.NewGuid(),
                RemoveAllAttributesFromExistingField = true
                // There are two other fields that can't be set because they are updated during the test and would break the build if included here
            };
            _context.ExistingTable.Add(newEntity);
            await _context.SaveChangesAsync();
            Assert.NotNull(_context.ExistingTable.Find(newEntity.Id));
        }

        private static async Task RunMigrationsAddAndApplyPowershellCommand(string migrationName) {
            var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));

            var addMigrationProcessStartInfo = new ProcessStartInfo {
                FileName = "dotnet",
                Arguments = $"ef migrations add {migrationName} --project . --startup-project . --context TestDbContext",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = projectDir
            };

            using (var addMigrationProcess = Process.Start(addMigrationProcessStartInfo)) {
                addMigrationProcess.WaitForExit(30000); // Wait for 30 seconds
                var addMigrationOutput = await addMigrationProcess.StandardOutput.ReadToEndAsync();
                var addMigrationError = await addMigrationProcess.StandardError.ReadToEndAsync();

                if (addMigrationProcess.ExitCode != 0) {
                    throw new InvalidOperationException($"Migration add failed: {addMigrationOutput} {addMigrationError}");
                }
            }

            var updateDatabaseProcessStartInfo = new ProcessStartInfo {
                FileName = "dotnet",
                Arguments = "ef database update --project . --startup-project . --context TestDbContext",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = projectDir
            };

            using (var updateDatabaseProcess = Process.Start(updateDatabaseProcessStartInfo)) {
                updateDatabaseProcess.WaitForExit(30000); // Wait for 30 seconds
                var updateDatabaseOutput = await updateDatabaseProcess.StandardOutput.ReadToEndAsync();
                var updateDatabaseError = await updateDatabaseProcess.StandardError.ReadToEndAsync();

                if (updateDatabaseProcess.ExitCode != 0) {
                    throw new InvalidOperationException($"Database update failed: {updateDatabaseOutput} {updateDatabaseError}");
                }
            }
        }

        private void AssertInitialMigrationSetupEPsCorrectly() {
            var initialExtendedProperties = Utilities.GetAllExtendedPropertiesFromDatabase(_context);
            Assert.NotNull(initialExtendedProperties);

            // Initial ExistingTable EPs
            Assert.Contains(initialExtendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "RemovePiiAttributeFromExistingField" && x.ExtendedPropertyName == "PII" && x.ExtendedPropertyValue == "true");

            Assert.Contains(initialExtendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "RemoveAllAttributesFromExistingField" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
            Assert.Contains(initialExtendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "RemoveAllAttributesFromExistingField" && x.ExtendedPropertyName == "PII" && x.ExtendedPropertyValue == "true");
            Assert.Contains(initialExtendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "RemoveAllAttributesFromExistingField" && x.ExtendedPropertyName == "CustomEP" && x.ExtendedPropertyValue == "CustomValue");

            Assert.Contains(initialExtendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "RemoveFieldAndAttributeFromExistingField" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");

            Assert.Contains(initialExtendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "ExistingField_RenameExistingField" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
            Assert.Contains(initialExtendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "ExistingField_RenameExistingField" && x.ExtendedPropertyName == "PII" && x.ExtendedPropertyValue == "true");

            // Initial TableToBeRemoved EPs
            Assert.Contains(initialExtendedProperties, x => x.TableName == "TableToBeRemoved" && x.ColumnName == "PhiFieldToBeRemoved" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");

            Assert.Contains(initialExtendedProperties, x => x.TableName == "TableToBeRemoved" && x.ColumnName == "PiiFieldToBeRemoved" && x.ExtendedPropertyName == "PII" && x.ExtendedPropertyValue == "true");

            Assert.Contains(initialExtendedProperties, x => x.TableName == "TableToBeRemoved" && x.ColumnName == "AllAttributesToBeRemoved" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
            Assert.Contains(initialExtendedProperties, x => x.TableName == "TableToBeRemoved" && x.ColumnName == "AllAttributesToBeRemoved" && x.ExtendedPropertyName == "PII" && x.ExtendedPropertyValue == "true");
            Assert.Contains(initialExtendedProperties, x => x.TableName == "TableToBeRemoved" && x.ColumnName == "AllAttributesToBeRemoved" && x.ExtendedPropertyName == "CustomEP" && x.ExtendedPropertyValue == "CustomValue");
            
            Assert.Equal(12, initialExtendedProperties.Count);
        }
    }
}
