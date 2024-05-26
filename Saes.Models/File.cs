using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("File")]
[Index("Guid", Name = "UQ_File_GUID", IsUnique = true)]
public partial class File
{
    [Key]
    [Column("FileID")]
    public int FileId { get; set; }

    [Column("GUID")]
    public Guid Guid { get; set; }

    public byte[]? Data { get; set; }

    [StringLength(128)]
    public string? Name { get; set; }

    [StringLength(64)]
    public string? ContentType { get; set; }

    [InverseProperty("SignFile")]
    public virtual ICollection<JournalInstanceForCihrecord> JournalInstanceForCihrecords { get; set; } = new List<JournalInstanceForCihrecord>();

    [InverseProperty("SignFile")]
    public virtual ICollection<JournalInstanceForCparecord> JournalInstanceForCparecords { get; set; } = new List<JournalInstanceForCparecord>();

    [InverseProperty("SignFile")]
    public virtual ICollection<JournalTechnicalRecord> JournalTechnicalRecords { get; set; } = new List<JournalTechnicalRecord>();

    [InverseProperty("SignFile")]
    public virtual ICollection<KeyHolder> KeyHolders { get; set; } = new List<KeyHolder>();
}
