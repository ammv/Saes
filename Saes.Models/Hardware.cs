using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("Hardware", Schema = "Office")]
[Index("SerialNumber", "OrganizationId", Name = "UQ_Hardware_SerialNumber", IsUnique = true)]
public partial class Hardware
{
    [Key]
    [Column("HardwareID")]
    public int HardwareId { get; set; }

    [Column("OrganizationID")]
    public int? OrganizationId { get; set; }

    [StringLength(256)]
    public string? Name { get; set; }

    [StringLength(32)]
    public string? SerialNumber { get; set; }

    [StringLength(512)]
    public string? Note { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [InverseProperty("Hardware")]
    public virtual ICollection<JournalInstanceForCihconnectedHardware> JournalInstanceForCihconnectedHardwares { get; set; } = new List<JournalInstanceForCihconnectedHardware>();

    public virtual Organization? Organization { get; set; }
}
