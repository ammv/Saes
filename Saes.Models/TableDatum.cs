using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("TableData", Schema = "Audit")]
[Index("Name", "SchemaName", Name = "IX_TableData_Name")]
[Index("Name", Name = "UQ_TableData_Name", IsUnique = true)]
[Index("RusName", Name = "UQ_TableData_RusName", IsUnique = true)]
[Index("Name", "SchemaName", Name = "UQ_TableData_SchemaNameAndName", IsUnique = true)]
public partial class TableDatum
{
    [Key]
    [Column("TableDataID")]
    public int TableDataId { get; set; }

    [StringLength(256)]
    public string SchemaName { get; set; } = null!;

    [StringLength(256)]
    public string Name { get; set; } = null!;

    [StringLength(256)]
    public string? RusName { get; set; }

    [InverseProperty("TableData")]
    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    [InverseProperty("TableData")]
    public virtual ICollection<TableColumnDatum> TableColumnData { get; set; } = new List<TableColumnDatum>();
}
