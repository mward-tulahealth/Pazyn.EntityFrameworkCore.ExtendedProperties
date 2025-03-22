using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties.Tests.Entities {
    [Table("ExistingTable")]
    [Comment("Existing table for integration tests")]
    // This entity represents the state of an existing table before the UpdateMigration migration is applied.
    // It is used to create the `InitialCreation` migration.
    // The `ExistingTable` entity represents the state of the same table after the migration is applied.
    public class ExistingTable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Comment("Auto incrementing id that is for internal use only")]
        public int Id { get; set; }

        [Comment("Public unique identifier of this point transaction")]
        public Guid ExternalId { get; set; }

        // Field to test adding an attribute after initial migration
        public string AddAttributeToExistingField { get; set; }

        // Field to test removing an attribute after initial migration
        [PHI]
        public bool RemoveAttributeFromExistingField { get; set; }

        // Field to test removing the field and attribute after initial migration
        [PHI]
        public int RemoveFieldAndAttributeFromExistingField { get; set; }

        // Field to test renaming with an attribute after initial migration
        [PHI]
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
        public string FieldToBeRemoved { get; set; }
    }
}
