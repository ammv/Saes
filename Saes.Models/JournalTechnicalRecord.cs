using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("JournalTechnicalRecord", Schema = "AccountingCPI")]
public partial class JournalTechnicalRecord
{
    [Column("OrganizationID")]
    public int? OrganizationId { get; set; }

    [Key]
    [Column("JournalTechnicalRecordID")]
    public int JournalTechnicalRecordId { get; set; }

    [Column(TypeName = "date")]
    public DateTime? Date { get; set; }

    [Column("TypeAndSerialUsedCPI")]
    [StringLength(256)]
    public string? TypeAndSerialUsedCpi { get; set; }

    [Column("RecordOnMaintenanceCPI")]
    [StringLength(256)]
    public string? RecordOnMaintenanceCpi { get; set; }

    [Column("KeyDocumentTypeID")]
    public int? KeyDocumentTypeId { get; set; }

    [Column("SerialCPIAndKeyDocumentInstanceNumber")]
    [StringLength(256)]
    public string? SerialCpiandKeyDocumentInstanceNumber { get; set; }

    [Column("NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted")]
    [StringLength(256)]
    public string? NumberOneTimeKeyCarrierCpizoneCryptoKeysInserted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DestructionDate { get; set; }

    [StringLength(256)]
    public string? ActNumber { get; set; }

    [StringLength(512)]
    public string? Note { get; set; }

    [Column("SignFileID")]
    public int? SignFileId { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("KeyDocumentTypeId")]
    [InverseProperty("JournalTechnicalRecords")]
    public virtual KeyDocumentType? KeyDocumentType { get; set; }

    public virtual Organization? Organization { get; set; }

    [ForeignKey("SignFileId")]
    [InverseProperty("JournalTechnicalRecords")]
    public virtual File? SignFile { get; set; }
}
