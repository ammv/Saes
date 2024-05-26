using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("JournalInstanceForCIHInstaller", Schema = "AccountingCPI")]
[Index("RecordId", "InstallerId", Name = "UQ_JournalInstanceForCIHInstaller_RecordID_InstallerID", IsUnique = true)]
public partial class JournalInstanceForCihinstaller
{
    [Key]
    [Column("JournalInstanceForCIHInstallerID")]
    public int JournalInstanceForCihinstallerId { get; set; }

    [Column("RecordID")]
    public int RecordId { get; set; }

    [Column("InstallerID")]
    public int InstallerId { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("InstallerId")]
    [InverseProperty("JournalInstanceForCihinstallers")]
    public virtual BusinessEntity Installer { get; set; } = null!;

    [ForeignKey("RecordId")]
    [InverseProperty("JournalInstanceForCihinstallers")]
    public virtual JournalInstanceForCihrecord Record { get; set; } = null!;
}
