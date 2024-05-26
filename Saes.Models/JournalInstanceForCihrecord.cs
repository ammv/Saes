using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("JournalInstanceForCIHRecord", Schema = "AccountingCPI")]
public partial class JournalInstanceForCihrecord
{
    [Column("OrganizationID")]
    public int? OrganizationId { get; set; }

    [Key]
    [Column("JournalInstanceForCIHRecordID")]
    public int JournalInstanceForCihrecordId { get; set; }

    [Column("NameCPI")]
    [StringLength(512)]
    public string? NameCpi { get; set; }

    [Column("SerialCPI")]
    [StringLength(256)]
    public string? SerialCpi { get; set; }

    public int? InstanceNumber { get; set; }

    [Column("ReceivedFromID")]
    public int? ReceivedFromId { get; set; }

    [StringLength(256)]
    public string? DateAndNumberCoverLetterReceive { get; set; }

    [Column("CPIUserID")]
    public int? CpiuserId { get; set; }

    [StringLength(256)]
    public string? DateAndNumberConfirmationIssue { get; set; }

    [StringLength(256)]
    public string? InstallationDateAndConfirmation { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DestructionDate { get; set; }

    [StringLength(256)]
    public string? DestructionActNumber { get; set; }

    [StringLength(512)]
    public string? Note { get; set; }

    [Column("SignFileID")]
    public int? SignFileId { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("CpiuserId")]
    [InverseProperty("JournalInstanceForCihrecordCpiusers")]
    public virtual BusinessEntity? Cpiuser { get; set; }

    [InverseProperty("Record")]
    public virtual ICollection<JournalInstanceForCihconnectedHardware> JournalInstanceForCihconnectedHardwares { get; set; } = new List<JournalInstanceForCihconnectedHardware>();

    [InverseProperty("Record")]
    public virtual ICollection<JournalInstanceForCihdestructor> JournalInstanceForCihdestructors { get; set; } = new List<JournalInstanceForCihdestructor>();

    [InverseProperty("Record")]
    public virtual ICollection<JournalInstanceForCihinstaller> JournalInstanceForCihinstallers { get; set; } = new List<JournalInstanceForCihinstaller>();

    public virtual Organization? Organization { get; set; }

    [ForeignKey("ReceivedFromId")]
    [InverseProperty("JournalInstanceForCihrecordReceivedFroms")]
    public virtual BusinessEntity? ReceivedFrom { get; set; }

    [ForeignKey("SignFileId")]
    [InverseProperty("JournalInstanceForCihrecords")]
    public virtual File? SignFile { get; set; }
}
