using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("UserRoleRight", Schema = "Authorization")]
[Index("UserRoleId", "RightId", Name = "UQ_UserRole_UserRoleID_RightID", IsUnique = true)]
public partial class UserRoleRight
{
    [Key]
    [Column("UserRoleRightID")]
    public int UserRoleRightId { get; set; }

    [Column("UserRoleID")]
    public int? UserRoleId { get; set; }

    [Column("RightID")]
    public int? RightId { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("RightId")]
    [InverseProperty("UserRoleRights")]
    public virtual Right? Right { get; set; }

    [ForeignKey("UserRoleId")]
    [InverseProperty("UserRoleRights")]
    public virtual UserRole? UserRole { get; set; }
}
