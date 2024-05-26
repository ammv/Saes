using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("BusinessEntity", Schema = "HumanResources")]
public partial class BusinessEntity
{
    [Key]
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    [Column("BusinessEntityTypeID")]
    public int BusinessEntityTypeId { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("BusinessEntityTypeId")]
    [InverseProperty("BusinessEntities")]
    public virtual BusinessEntityType BusinessEntityType { get; set; } = null!;

    [InverseProperty("BusinessEntity")]
    public virtual Employee? Employee { get; set; }

    [InverseProperty("Receiver")]
    public virtual ICollection<JournalInstanceCpareceiver> JournalInstanceCpareceivers { get; set; } = new List<JournalInstanceCpareceiver>();

    [InverseProperty("Destructor")]
    public virtual ICollection<JournalInstanceForCihdestructor> JournalInstanceForCihdestructors { get; set; } = new List<JournalInstanceForCihdestructor>();

    [InverseProperty("Installer")]
    public virtual ICollection<JournalInstanceForCihinstaller> JournalInstanceForCihinstallers { get; set; } = new List<JournalInstanceForCihinstaller>();

    [InverseProperty("Cpiuser")]
    public virtual ICollection<JournalInstanceForCihrecord> JournalInstanceForCihrecordCpiusers { get; set; } = new List<JournalInstanceForCihrecord>();

    [InverseProperty("ReceivedFrom")]
    public virtual ICollection<JournalInstanceForCihrecord> JournalInstanceForCihrecordReceivedFroms { get; set; } = new List<JournalInstanceForCihrecord>();

    [InverseProperty("ReceivedFrom")]
    public virtual ICollection<JournalInstanceForCparecord> JournalInstanceForCparecords { get; set; } = new List<JournalInstanceForCparecord>();

    [InverseProperty("UserCpiNavigation")]
    public virtual ICollection<KeyHolder> KeyHolders { get; set; } = new List<KeyHolder>();

    [InverseProperty("BusinessEntity")]
    public virtual Organization? Organization { get; set; }
}
