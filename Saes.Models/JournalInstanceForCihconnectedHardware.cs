using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("JournalInstanceForCIHConnectedHardware", Schema = "AccountingCPI")]
[Index("RecordId", "HardwareId", Name = "UQ_RecordCPIForCIH_RecordID_HardwareID", IsUnique = true)]
public partial class JournalInstanceForCihconnectedHardware
{
    [Key]
    [Column("JournalInstanceForCIHConnectedHardwareID")]
    public int JournalInstanceForCihconnectedHardwareId { get; set; }

    [Column("RecordID")]
    public int RecordId { get; set; }

    [Column("HardwareID")]
    public int HardwareId { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("HardwareId")]
    [InverseProperty("JournalInstanceForCihconnectedHardwares")]
    public virtual Hardware Hardware { get; set; } = null!;

    [ForeignKey("RecordId")]
    [InverseProperty("JournalInstanceForCihconnectedHardwares")]
    public virtual JournalInstanceForCihrecord Record { get; set; } = null!;
}
