using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("KeyHolder", Schema = "AccountingCPI")]
[Index("SerialNumber", Name = "UQ_KeyHolder_SerialNumber", IsUnique = true)]
public partial class KeyHolder
{
    [Key]
    [Column("KeyHolderID")]
    public int KeyHolderId { get; set; }

    [StringLength(128)]
    public string SerialNumber { get; set; } = null!;

    [Column("TypeID")]
    public int TypeId { get; set; }

    [Column("UserCPI")]
    public int? UserCpi { get; set; }

    [Column("SignFileID")]
    public int? SignFileId { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("SignFileId")]
    [InverseProperty("KeyHolders")]
    public virtual File? SignFile { get; set; }

    [ForeignKey("TypeId")]
    [InverseProperty("KeyHolders")]
    public virtual KeyHolderType Type { get; set; } = null!;

    [ForeignKey("UserCpi")]
    [InverseProperty("KeyHolders")]
    public virtual BusinessEntity? UserCpiNavigation { get; set; }
}
