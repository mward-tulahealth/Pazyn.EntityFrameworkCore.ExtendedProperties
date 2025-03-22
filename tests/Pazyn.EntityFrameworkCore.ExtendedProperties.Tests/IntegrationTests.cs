using Microsoft.EntityFrameworkCore;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Tests.Entities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Tests {
    public class IntegrationTests : IAsyncLifetime {
        private readonly string _connectionString = "Server=localhost;Database=TestDatabase;User ID=sa;Password=tKtFqRz9B^BgQdqp&4YF;Trusted_Connection=True;";
        private readonly string _projectPath = "../../../";
        private readonly string _entitiesPath = "../../../Entities/";
        private readonly string _templatesPath = "../../../Templates/";
        private TestDbContext _context;
        private string snapshotOfSnapshot;

        public async Task InitializeAsync() {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlServer(_connectionString)
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
            
            var extendedProperties = Utilities.GetAllExtendedPropertiesFromDatabase(_context);

            // Assert
            Assert.True(_context.Database.CanConnect());
            Assert.NotNull(extendedProperties);
            Assert.Equal(2, extendedProperties.Count);
            Assert.Contains(extendedProperties, x => x.TableName == "TableToBeAdded" && x.ColumnName == "FieldToBeAdded" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
            Assert.Contains(extendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "ExistingField_RenameExistingField_NewName" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
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
                RemoveAttributeFromExistingField = true
                // There are two other fields that can't be set because they change and would break the build if included here
            };
            _context.ExistingTable.Add(newEntity);
            await _context.SaveChangesAsync();
            Assert.NotNull(_context.ExistingTable.Find(newEntity.Id));
        }

        private static async Task RunMigrationsAddAndApplyPowershellCommand(string migrationName) {
            var projectDir = @"c:\Users\Owner\source\repos\Pazyn.EntityFrameworkCore.ExtendedProperties\tests\Pazyn.EntityFrameworkCore.ExtendedProperties.Tests";

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
                addMigrationProcess.WaitForExit();
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
                updateDatabaseProcess.WaitForExit();
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
            Assert.Equal(4, initialExtendedProperties.Count);
            Assert.Contains(initialExtendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "RemoveAttributeFromExistingField" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
            Assert.Contains(initialExtendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "RemoveFieldAndAttributeFromExistingField" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
            Assert.Contains(initialExtendedProperties, x => x.TableName == "ExistingTable" && x.ColumnName == "ExistingField_RenameExistingField" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
            Assert.Contains(initialExtendedProperties, x => x.TableName == "TableToBeRemoved" && x.ColumnName == "FieldToBeRemoved" && x.ExtendedPropertyName == "PHI" && x.ExtendedPropertyValue == "true");
        }
    }
}
