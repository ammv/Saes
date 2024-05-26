using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Keyless]
public partial class ViewLogChange
{
    [Column("LogID")]
    public int? LogId { get; set; }

    [StringLength(256)]
    public string Table { get; set; } = null!;

    [StringLength(256)]
    public string Column { get; set; } = null!;

    [StringLength(512)]
    public string? OldValue { get; set; }

    [StringLength(512)]
    public string? NewValue { get; set; }
}
