using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("UserRole", Schema = "Authorization")]
[Index("Name", Name = "UQ_UserRole_Name", IsUnique = true)]
public partial class UserRole
{
    [Key]
    [Column("UserRoleID")]
    public int UserRoleId { get; set; }

    [StringLength(64)]
    public string Name { get; set; } = null!;

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [InverseProperty("UserRole")]
    public virtual ICollection<UserRoleRight> UserRoleRights { get; set; } = new List<UserRoleRight>();

    [InverseProperty("UserRole")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
