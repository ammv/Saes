using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Keyless]
public partial class ViewLog
{
    [Column("LogID")]
    public int LogId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }

    [StringLength(14)]
    [Unicode(false)]
    public string? Action { get; set; }

    [StringLength(256)]
    public string Table { get; set; } = null!;

    [Column("TableRecordID")]
    public int TableRecordId { get; set; }

    [StringLength(32)]
    public string User { get; set; } = null!;

    [StringLength(64)]
    public string UserRole { get; set; } = null!;
}
