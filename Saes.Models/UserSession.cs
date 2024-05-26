using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("UserSession", Schema = "Authorization")]
[Index("SessionKey", Name = "IX_UserSession_SessionKey")]
[Index("SessionKey", Name = "UQ_UserSession_SessionKey", IsUnique = true)]
public partial class UserSession
{
    [Key]
    [Column("UserSessionID")]
    public int UserSessionId { get; set; }

    [StringLength(128)]
    public string SessionKey { get; set; } = null!;

    [Column("UserID")]
    public int UserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ExpiredAt { get; set; }

    public bool? IsExpired { get; set; }

    [Column("LogAuthenticationID")]
    public int LogAuthenticationId { get; set; }

    [InverseProperty("UserSession")]
    public virtual ICollection<ErrorLog> ErrorLogs { get; set; } = new List<ErrorLog>();

    [ForeignKey("LogAuthenticationId")]
    [InverseProperty("UserSessions")]
    public virtual LogAuthentication LogAuthentication { get; set; } = null!;

    [InverseProperty("UserSession")]
    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    [ForeignKey("UserId")]
    [InverseProperty("UserSessions")]
    public virtual User User { get; set; } = null!;
}
