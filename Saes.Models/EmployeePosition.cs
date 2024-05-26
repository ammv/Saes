using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("EmployeePosition", Schema = "HumanResources")]
[Index("Name", Name = "UQ_EmployeePosition_Name", IsUnique = true)]
public partial class EmployeePosition
{
    [Key]
    [Column("EmployeePositionID")]
    public int EmployeePositionId { get; set; }

    [StringLength(64)]
    public string Name { get; set; } = null!;

    [StringLength(512)]
    public string? Note { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [InverseProperty("EmployeePosition")]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
