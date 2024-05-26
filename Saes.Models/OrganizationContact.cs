using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("OrganizationContact", Schema = "HumanResources")]
public partial class OrganizationContact
{
    [Key]
    [Column("OrganizationContactID")]
    public int OrganizationContactId { get; set; }

    [Column("OrganizationID")]
    public int OrganizationId { get; set; }

    [Column("ContactTypeID")]
    public int ContactTypeId { get; set; }

    [StringLength(128)]
    public string? Value { get; set; }

    [StringLength(512)]
    public string? Note { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [ForeignKey("ContactTypeId")]
    [InverseProperty("OrganizationContacts")]
    public virtual ContactType ContactType { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;
}
