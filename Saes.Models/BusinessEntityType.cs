using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("BusinessEntityType", Schema = "HumanResources")]
[Index("Name", Name = "UQ_BusinessEntityType_Name", IsUnique = true)]
public partial class BusinessEntityType
{
    [Key]
    [Column("BusinessEntityTypeID")]
    public int BusinessEntityTypeId { get; set; }

    [StringLength(128)]
    public string Name { get; set; } = null!;

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [InverseProperty("BusinessEntityType")]
    public virtual ICollection<BusinessEntity> BusinessEntities { get; set; } = new List<BusinessEntity>();
}
