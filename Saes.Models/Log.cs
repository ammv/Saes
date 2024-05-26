using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("Log", Schema = "Audit")]
public partial class Log
{
    [Key]
    [Column("LogID")]
    public int LogId { get; set; }

    [Column("TableDataID")]
    public int? TableDataId { get; set; }

    [Column("TableRowID")]
    public int TableRowId { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string Action { get; set; } = null!;

    [Column("UserSessionID")]
    public int? UserSessionId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }

    [Column("GUID")]
    public Guid Guid { get; set; }

    [ForeignKey("TableDataId")]
    [InverseProperty("Logs")]
    public virtual TableDatum? TableData { get; set; }

    [ForeignKey("UserSessionId")]
    [InverseProperty("Logs")]
    public virtual UserSession? UserSession { get; set; }
}
