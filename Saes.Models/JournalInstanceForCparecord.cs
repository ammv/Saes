using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("JournalInstanceForCPARecord", Schema = "AccountingCPI")]
public partial class JournalInstanceForCparecord
{
    [Column("OrganizationID")]
    public int? OrganizationId { get; set; }

    [Key]
    [Column("JournalInstanceForCPARecordID")]
    public int JournalInstanceForCparecordId { get; set; }

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

    [StringLength(256)]
    public string? DateAndNumberCoverLetterSend { get; set; }

    [StringLength(256)]
    public string? DateAndNumberConfirmationSend { get; set; }

    [StringLength(256)]
    public string? DateAndNumberCoverLetterReturn { get; set; }

    [StringLength(256)]
    public string? DateAndNumberConfirmationReturn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CommissioningDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DecommissioningDate { get; set; }

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

    [InverseProperty("Record")]
    public virtual ICollection<JournalInstanceCpareceiver> JournalInstanceCpareceivers { get; set; } = new List<JournalInstanceCpareceiver>();

    public virtual Organization? Organization { get; set; }

    [ForeignKey("ReceivedFromId")]
    [InverseProperty("JournalInstanceForCparecords")]
    public virtual BusinessEntity? ReceivedFrom { get; set; }

    [ForeignKey("SignFileId")]
    [InverseProperty("JournalInstanceForCparecords")]
    public virtual File? SignFile { get; set; }
}
