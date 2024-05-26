using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("TableColumnData", Schema = "Audit")]
[Index("Name", Name = "IX_TableColumnData_Name")]
[Index("TableDataId", "Name", Name = "UQ_TableColumnData_Name", IsUnique = true)]
public partial class TableColumnDatum
{
    [Key]
    [Column("TableColumnDataID")]
    public int TableColumnDataId { get; set; }

    [Column("TableDataID")]
    public int? TableDataId { get; set; }

    [StringLength(256)]
    public string Name { get; set; } = null!;

    [StringLength(256)]
    public string? RusName { get; set; }

    [ForeignKey("TableDataId")]
    [InverseProperty("TableColumnData")]
    public virtual TableDatum? TableData { get; set; }
}
