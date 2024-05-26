using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Keyless]
[Table("LogChange", Schema = "Audit")]
[Index("AuditLogId", "TableColumnDataId", Name = "UQ_LogChange_AuditLogID_TableColumnDataID", IsUnique = true)]
public partial class LogChange
{
    [Column("AuditLogID")]
    public int? AuditLogId { get; set; }

    [Column("TableColumnDataID")]
    public int? TableColumnDataId { get; set; }

    [StringLength(512)]
    public string? OldValue { get; set; }

    [StringLength(512)]
    public string? NewValue { get; set; }

    [ForeignKey("AuditLogId")]
    public virtual Log? AuditLog { get; set; }

    [ForeignKey("TableColumnDataId")]
    public virtual TableColumnDatum? TableColumnData { get; set; }
}
