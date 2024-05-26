using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("KeyDocumentType", Schema = "AccountingCPI")]
[Index("Name", Name = "UQ_KeyDocumentType_Name", IsUnique = true)]
public partial class KeyDocumentType
{
    [Key]
    [Column("KeyDocumentTypeID")]
    public int KeyDocumentTypeId { get; set; }

    [StringLength(256)]
    public string Name { get; set; } = null!;

    [StringLength(256)]
    public string? Note { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [InverseProperty("KeyDocumentType")]
    public virtual ICollection<JournalTechnicalRecord> JournalTechnicalRecords { get; set; } = new List<JournalTechnicalRecord>();
}
