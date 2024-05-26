using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("JournalInstanceCPAReceiver", Schema = "AccountingCPI")]
[Index("RecordId", "ReceiverId", Name = "UQ_JournalInstanceCPAReceiver_RecordID_ReceiverID", IsUnique = true)]
public partial class JournalInstanceCpareceiver
{
    [Key]
    [Column("JournalInstanceCPAReceiverID")]
    public int JournalInstanceCpareceiverId { get; set; }

    [Column("RecordID")]
    public int? RecordId { get; set; }

    [Column("ReceiverID")]
    public int? ReceiverId { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("JournalInstanceCpareceiverId")]
    [InverseProperty("JournalInstanceCpareceiver")]
    public virtual JournalInstanceForCparecord JournalInstanceCpareceiverNavigation { get; set; } = null!;

    [ForeignKey("ReceiverId")]
    [InverseProperty("JournalInstanceCpareceivers")]
    public virtual BusinessEntity? Receiver { get; set; }
}
