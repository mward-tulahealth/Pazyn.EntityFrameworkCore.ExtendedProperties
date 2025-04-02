using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Pazyn.EntityFrameworkCore.ExtendedProperties.Entities;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Tests.Entities {
    [Table("ExistingTable")]
    [Comment("Existing table for integration tests")]
    // InitialStateEntities

    // This entity represents the state of an existing table before the UpdateMigration migration is applied.
    // It is used to create the `InitialCreation` migration.
    // The `ExistingTable` entity represents the state of the same table after the migration is applied.

    // Do not modify this file in `OverwritableEntities.cs` file, only in `InitialStateEntities.txt` template file.
    public class ExistingTable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Comment("Auto incrementing id that is for internal use only")]
        public int Id { get; set; }

        [Comment("Public unique identifier of this point transaction")]
        public Guid ExternalId { get; set; }

        // Field to test adding an attribute after initial migration
        public string AddAttributeToExistingField { get; set; }

        // Field to test adding all attributes after initial migration
        public bool AddAllAttributesToExistingField { get; set; }

        // Field to test removing an attribute after initial migration
        [PII]
        public Guid RemovePiiAttributeFromExistingField { get; set; }

        // Field to test removing attributes after initial migration
        [PHI]
        [PII]
        [ExtendedProperty("CustomEP", "CustomValue")]
        public bool RemoveAllAttributesFromExistingField { get; set; }

        // Field to test removing the field and attribute after initial migration
        [PHI]
        public int RemoveFieldAndAttributeFromExistingField { get; set; }

        // Field to test renaming with an attribute after initial migration
        [PHI]
        [PII]
        public long ExistingField_RenameExistingField { get; set; }
    }

    [Table("TableToBeRemoved")]
    [Comment("Table to be removed for integration tests")]
    // This entity represents the state of a table that will be removed in the test migration.
    // It is used to create the `InitialCreation` migration.
    public class TableToBeRemoved {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Comment("Auto incrementing id that is for internal use only")]
        public int Id { get; set; }

        [Comment("Public unique identifier of this point transaction")]
        public Guid ExternalId { get; private set; }

        // Field to test that attribute is removed after deleting table
        [PHI]
        public string PhiFieldToBeRemoved { get; set; }

        // Field to test that attribute is removed after deleting table
        [PII]
        public bool PiiFieldToBeRemoved { get; set; }

        // Field to test that all attributes are removed after deleting table
        [PHI]
        [PII]
        [ExtendedProperty("CustomEP", "CustomValue")]
        public int AllAttributesToBeRemoved { get; set; }
    }
}
