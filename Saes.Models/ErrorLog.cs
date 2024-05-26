using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("ErrorLog", Schema = "Audit")]
public partial class ErrorLog
{
    [Key]
    [Column("ErrorLogID")]
    public int ErrorLogId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ErrorTime { get; set; }

    [Column("UserSessionID")]
    public int? UserSessionId { get; set; }

    public int ErrorNumber { get; set; }

    public int? ErrorSeverity { get; set; }

    public int? ErrorState { get; set; }

    [StringLength(126)]
    public string? ErrorProcedure { get; set; }

    public int? ErrorLine { get; set; }

    [StringLength(4000)]
    public string ErrorMessage { get; set; } = null!;

    [ForeignKey("UserSessionId")]
    [InverseProperty("ErrorLogs")]
    public virtual UserSession? UserSession { get; set; }
}
