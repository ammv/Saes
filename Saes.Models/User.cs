using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("User", Schema = "Authentication")]
[Index("Login", Name = "IX_User_Login")]
[Index("Login", Name = "UQ_User_Login", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(32)]
    public string Login { get; set; } = null!;

    [MaxLength(32)]
    public byte[] PasswordHash { get; set; } = null!;

    [MaxLength(16)]
    public byte[] PasswordSalt { get; set; } = null!;

    [Column("UserRoleID")]
    public int? UserRoleId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastLoginDate { get; set; }

    [StringLength(64)]
    public string? TotpSecretKey { get; set; }

    [Required]
    public bool? TwoFactorEnabled { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("UserRoleId")]
    [InverseProperty("Users")]
    public virtual UserRole? UserRole { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
}
