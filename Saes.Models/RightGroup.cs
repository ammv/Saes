using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("RightGroup", Schema = "Authorization")]
[Index("Code", Name = "UQ_RightGroup_Code", IsUnique = true)]
public partial class RightGroup
{
    [Key]
    [Column("RightGroupID")]
    public int RightGroupId { get; set; }

    [StringLength(256)]
    public string? Code { get; set; }

    [StringLength(256)]
    public string? Name { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [InverseProperty("RightGroup")]
    public virtual ICollection<Right> Rights { get; set; } = new List<Right>();
}
