using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("LogAuthentication", Schema = "Audit")]
public partial class LogAuthentication
{
    [Key]
    [Column("LogAuthenticationID")]
    public int LogAuthenticationId { get; set; }

    [StringLength(32)]
    public string? EnteredLogin { get; set; }

    public bool FirstFactorResult { get; set; }

    public bool SecondFactorResult { get; set; }

    [StringLength(1024)]
    public string AuthServiceResponse { get; set; } = null!;

    [Column("UserIDFoundByEnteredLogin")]
    public int? UserIdfoundByEnteredLogin { get; set; }

    [Column("MAC")]
    [StringLength(12)]
    public string? Mac { get; set; }

    [Column("IP")]
    [StringLength(12)]
    public string? Ip { get; set; }

    [StringLength(128)]
    public string? MashineName { get; set; }

    [StringLength(128)]
    public string? MashineUserName { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }

    [InverseProperty("LogAuthentication")]
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
}
