using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("Employee", Schema = "HumanResources")]
[Index("EmployeeId", Name = "UQ_Employee_EmployeeID", IsUnique = true)]
public partial class Employee
{
    [Key]
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    [Column("EmployeeID")]
    public int EmployeeId { get; set; }

    [Column("OrganizationID")]
    public int OrganizationId { get; set; }

    [StringLength(64)]
    public string FirstName { get; set; } = null!;

    [StringLength(64)]
    public string MiddleName { get; set; } = null!;

    [StringLength(64)]
    public string? LastName { get; set; }

    [Column("EmployeePositionID")]
    public int EmployeePositionId { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("BusinessEntityId")]
    [InverseProperty("Employee")]
    public virtual BusinessEntity BusinessEntity { get; set; } = null!;

    [ForeignKey("EmployeePositionId")]
    [InverseProperty("Employees")]
    public virtual EmployeePosition EmployeePosition { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;
}
