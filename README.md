# Custom attribute to SQL ExtendedProperties

- This project was created to allow setting custom attributes on Entity Framework Core entity properties which can be tracked during a migration and added as sql extended property key/value pairs.

- This project was originally forked from here: https://github.com/bopazyn/Pazyn.EntityFrameworkCore.ExtendedProperties

## Usage
If we had an EF property that we'd like to mark in sql extended properties, we can add a custom attribute to denote that it should be added during a migration:

```csharp
public class EFTable {
    [PHI] // Just add this attribute
    public string SSN_Property { get; set; }
}
```

Any new custom attribute can be defined. The attribute definition must use base class [ExtendedPropertyAttribute](src/Pazyn.EntityFrameworkCore.ExtendedProperties/ExtendedPropertyAttribute.cs) to map what the extended property's key/value pair will be set to:

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class PHIAttribute : ExtendedPropertyAttribute {
    public PHIAttribute() : base("PHI", "true") { // key = "PHI", value = "true"
    }
}
```

You can add multiple custom attributes to be mapped to extended properties, as long as each uses a unique key.

You can even call the [ExtendedPropertyAttribute](src/Pazyn.EntityFrameworkCore.ExtendedProperties/ExtendedPropertyAttribute.cs) directly:

```csharp
public class EFTable {
    [ExtendedProperty("CustomEpKey", "CustomEpValue")]
    public string SSN_Property { get; set; }
}
```

The custom code will create custom migration operations, which will be added to the migration cs file like this:

```csharp
protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.AddSqlExtendedProperty(
        table: "Member", 
        column: "SsnLastFour", 
        key: "PHI", 
        value: "true"
    );

    migrationBuilder.RemoveSqlExtendedProperty(
        table: "Member", 
        column: "LastName", 
        key: "CustomProperty"
    );
}
```

After the migration has been applied, you can see that the extended properties were added by running something like this query:

```sql
SELECT tbl.TABLE_NAME, col.COLUMN_NAME, hprop.name AS ExtendedPropertyName, hprop.value AS ExtendedPropertyValue
FROM INFORMATION_SCHEMA.TABLES AS tbl
INNER JOIN INFORMATION_SCHEMA.COLUMNS AS col ON col.TABLE_NAME = tbl.TABLE_NAME AND col.TABLE_SCHEMA = tbl.TABLE_SCHEMA
INNER JOIN sys.columns AS sc ON sc.object_id = OBJECT_ID(tbl.TABLE_SCHEMA + '.' + tbl.TABLE_NAME) AND sc.name = col.COLUMN_NAME
LEFT JOIN sys.extended_properties hprop ON hprop.major_id = sc.object_id AND hprop.minor_id = sc.column_id
WHERE tbl.TABLE_SCHEMA = 'dbo' AND hprop.name IS NOT NULL AND hprop.name <> 'MS_Description' AND hprop.value IS NOT NULL
```

## Setup
Add a reference to `LiveTula.Services.Common.ExtendedProperties.csproj` in your csproj files for projects that contain:
- `DesignTimeDbContextFactory`
- EF Core Entities

Remove this line if it's included under `Microsoft.EntityFrameworkCore.Design`'s PackageReference to ensure we use the correct custom migration code:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.2">
    <PrivateAssets>all</PrivateAssets>
    <!-- MAKE SURE YOU'VE REMOVED THE FOLLOWING LINE: -->
    <!-- <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets> -->
</PackageReference>
```

Set up or modify a class that implements the `IDesignTimeDbContextFactory<DatabaseContext>` interface:

```csharp
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext> {
    var builder = new DbContextOptionsBuilder<DatabaseContext>();
    builder.UseSqlServer(connectionString);

    // Replace default services with custom implementations
    builder.ReplaceService<IMigrationsModelDiffer, CustomMigrationsModelDiffer>();
    builder.ReplaceService<IMigrationsSqlGenerator, ExtendedPropertiesMigrationsSqlGenerator>();
    builder.ReplaceService<ICSharpMigrationOperationGenerator, CustomCSharpMigrationOperationGenerator>();
}
```

Set up or modify a class that implements the `IDesignTimeServices` interface:

```csharp
public class CustomDesignTimeService : IDesignTimeServices {
    public void ConfigureDesignTimeServices(IServiceCollection serviceCollection) {
        serviceCollection.AddSingleton<IMigrationsCodeGenerator, CustomCSharpMigrationsGenerator>();
        serviceCollection.AddSingleton<ICSharpMigrationOperationGenerator, CustomCSharpMigrationOperationGenerator>();
    }
}
```

## How it works

### Full EF Core Migration step-by-step
1. Run `dotnet ef migrations add` in dotnet-cli.
1. EF locates `DbContext` if one wasn't set in the command.
1. EF creates an instance of `DbContext` (using factory if available):
    - In our case, it uses:
        - `DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>`
            - This can pull the connection string from a config like `appsettings.json`.
1. EF builds the `target` model using the `OnModelCreating` function.
    - Represents model with any new changes made in code.
1. EF builds the `source` model from the EF migrations snapshot.
1. EF compares the target model to the source model using `IMigrationsModelDiffer`
    - We implement a custom class to override the `Diff` function to:
        - Find custom attributes on properties.
        - Compare with extended properties in our SQL `sys.extended_properties` table.
    - This returns `IEnumerable<MigrationOperation>`
        - In our case, this includes custom `MigrationOperation`s:
            - `AddExtendedPropertyOperation`
            - `RemoveExtendedPropertyOperation`
    - The `Diff` function is called twice in a migration:
        - Once to generate migration operations for the c# `Up` function
        - Once to generate migration operations for the c# `Down` function
            - Running the diff for the `Down` function will swap the source and target, which causes issues with how this project works
                - So instead of running the diff as before, this has been overridden to look at the static `UpOperationsHolder` class, which will hold the Add/Remove operations, which we can return reversed for the `Down` function
1. EF creates C# migration code using `ICSharpMigrationOperationGenerator`
    - We implement a custom class to override the `Generate` function to:
        - Use a switch statement to find our new custom types.
        - Append C# code to `StringBuilder` that will call a function handled by the provider (SQL, Postgres, etc).
1. EF sets custom namespace using `IMigrationsCodeGenerator`
    - We implement a custom class to override the `GetNamespaces` function.
1. EF generates SQL scripts using `IMigrationsSqlGenerator`
    - In our case, we implement a custom `SqlServerMigrationsSqlGenerator` class.
    - We implement a custom class to add our custom handlers:
        - `AddSqlExtendedProperty`
        - `RemoveSqlExtendedProperty`

## Debugging
Because the EF migration runs separate from the application, the best way I've found to debug into the custom migration code is by adding the following debugger command into this project (likely in this file: [CustomMigrationsModelDiffer](src/Pazyn.EntityFrameworkCore.ExtendedProperties/CustomMigrationsModelDiffer.cs)) and pick to debug in Visual Studio in the window that opens.
```csharp
Debugger.Launch();
```

## Running test project
Included in the repo is a test project for simulating various migration operations:
- Add attribute to existing column
- Remove attribute from existing column
- Add column + attribute
- Remove column + attribute
- Rename column with attribute
- Add table with columns with attributes
- Remove table with columns with attributes

The test project is rather complex and is described below:

### Custom migrations test steps:
- Create initial tables:
	- ExistingTable
	- TableToBeRemoved
- Migration add
- Migration apply
- Verify PHI tags set correctly
- Modify tables:
	- Update ExistingTable
	- Remove TableToBeRemoved
	- Add TableToBeAdded
- Run `dotnet ef migrations add`
- Migration apply
- Verify PHI tags updated correctly

To simulate changes in entity files for initial setup and updating to add/remove custom attributes I chose to swap out the [OverwritableEntities](tests/Pazyn.EntityFrameworkCore.ExtendedProperties.tests/Entities/OverwritableEntities.cs) and [TestDbContext](tests/Pazyn.EntityFrameworkCore.ExtendedProperties.tests/TestDbContext.cs) files with templates from the [Templates](tests/Pazyn.EntityFrameworkCore.ExtendedProperties.tests/Templates/) directory. This swap happens while the test is running. The test begins by applying the `InitialMigration` migration to create the initial state of the tables and creates the extended properties as defined in [InitialStateEntities](tests/Pazyn.EntityFrameworkCore.ExtendedProperties.tests/Templates/InitialStateEntities.txt).

The test then copies over the [OverwritableEntities](tests/Pazyn.EntityFrameworkCore.ExtendedProperties.tests/Entities/OverwritableEntities.cs) file with [FinalStateEntities](tests/Pazyn.EntityFrameworkCore.ExtendedProperties.tests/Templates/FinalStateEntities.txt) to represent changes that would happen to custom attributes before the next migration: `UpdatedModelsMigration`. This migration is added and applied through a powershell script during the test. This step requires that the test project is built DURING the test. This can cause issues if messing with the test later.

After the second `UpdatedModelsMigration` migration runs, assertions will run to verify that the correct extended properties were added/removed as expected. Finally, the test will remove the temporary local db and remove the `UpdatedModelsMigration` migration files, reset the snapshot, and reset the [OverwritableEntities](tests/Pazyn.EntityFrameworkCore.ExtendedProperties.tests/Entities/OverwritableEntities.cs) and [TestDbContext](tests/Pazyn.EntityFrameworkCore.ExtendedProperties.tests/TestDbContext.cs) files with the initial template files to be ready to run the test fresh.

