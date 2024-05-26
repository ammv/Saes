using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("Organization", Schema = "HumanResources")]
[Index("OrganizationId", Name = "UQ_Organization_OrganizationID", IsUnique = true)]
public partial class Organization
{
    [Key]
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    [Column("OrganizationID")]
    public int OrganizationId { get; set; }

    [StringLength(256)]
    public string? FullName { get; set; }

    [StringLength(256)]
    public string? ShortName { get; set; }

    [Column("BusinessAddressID")]
    public int? BusinessAddressId { get; set; }

    [Column("INN")]
    [StringLength(12)]
    public string? Inn { get; set; }

    [Column("KPP")]
    [StringLength(9)]
    public string? Kpp { get; set; }

    [Column("OKPO")]
    [StringLength(8)]
    public string? Okpo { get; set; }

    [Column("OGRN")]
    [StringLength(13)]
    public string? Ogrn { get; set; }

    [Column("DateOfAssignmentOGRN", TypeName = "date")]
    public DateTime? DateOfAssignmentOgrn { get; set; }

    [StringLength(128)]
    public string? DirectorFullName { get; set; }

    [StringLength(128)]
    public string? ChiefAccountantFullName { get; set; }

    [Column("OKVED")]
    [StringLength(32)]
    public string? Okved { get; set; }

    [Column("IsOwnerJournalAccountingCPI")]
    public bool IsOwnerJournalAccountingCpi { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("BusinessAddressId")]
    [InverseProperty("Organizations")]
    public virtual Address? BusinessAddress { get; set; }

    [ForeignKey("BusinessEntityId")]
    [InverseProperty("Organization")]
    public virtual BusinessEntity BusinessEntity { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<JournalInstanceForCihrecord> JournalInstanceForCihrecords { get; set; } = new List<JournalInstanceForCihrecord>();

    public virtual ICollection<JournalInstanceForCparecord> JournalInstanceForCparecords { get; set; } = new List<JournalInstanceForCparecord>();

    public virtual ICollection<JournalTechnicalRecord> JournalTechnicalRecords { get; set; } = new List<JournalTechnicalRecord>();

    public virtual ICollection<OrganizationContact> OrganizationContacts { get; set; } = new List<OrganizationContact>();
}
