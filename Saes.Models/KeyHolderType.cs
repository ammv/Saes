using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("KeyHolderType", Schema = "AccountingCPI")]
[Index("Name", Name = "UQ_KeyHolderType_Name", IsUnique = true)]
public partial class KeyHolderType
{
    [Key]
    [Column("KeyHolderTypeID")]
    public int KeyHolderTypeId { get; set; }

    [StringLength(256)]
    public string Name { get; set; } = null!;

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [InverseProperty("Type")]
    public virtual ICollection<KeyHolder> KeyHolders { get; set; } = new List<KeyHolder>();
}
