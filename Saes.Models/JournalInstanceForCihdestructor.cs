using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("JournalInstanceForCIHDestructor", Schema = "AccountingCPI")]
[Index("RecordId", "DestructorId", Name = "UQ_JournalInstanceForCIHDestructor_RecordID_DestructorID", IsUnique = true)]
public partial class JournalInstanceForCihdestructor
{
    [Key]
    [Column("JournalInstanceForCIHDestructorID")]
    public int JournalInstanceForCihdestructorId { get; set; }

    [Column("RecordID")]
    public int RecordId { get; set; }

    [Column("DestructorID")]
    public int DestructorId { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("DestructorId")]
    [InverseProperty("JournalInstanceForCihdestructors")]
    public virtual BusinessEntity Destructor { get; set; } = null!;

    [ForeignKey("RecordId")]
    [InverseProperty("JournalInstanceForCihdestructors")]
    public virtual JournalInstanceForCihrecord Record { get; set; } = null!;
}
