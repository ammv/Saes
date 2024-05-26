using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("Right", Schema = "Authorization")]
[Index("RightGroupId", "Code", Name = "UQ_Right_GroupID_Code", IsUnique = true)]
public partial class Right
{
    [Key]
    [Column("RightID")]
    public int RightId { get; set; }

    [Column("RightGroupID")]
    public int? RightGroupId { get; set; }

    [StringLength(256)]
    public string? Code { get; set; }

    [StringLength(256)]
    public string? Name { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("RightGroupId")]
    [InverseProperty("Rights")]
    public virtual RightGroup? RightGroup { get; set; }

    [InverseProperty("Right")]
    public virtual ICollection<UserRoleRight> UserRoleRights { get; set; } = new List<UserRoleRight>();
}
