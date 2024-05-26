using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("ContactType", Schema = "Person")]
[Index("Name", Name = "UQ_ContactType_Name", IsUnique = true)]
public partial class ContactType
{
    [Key]
    [Column("ContactTypeID")]
    public int ContactTypeId { get; set; }

    [StringLength(64)]
    public string? Name { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [InverseProperty("ContactType")]
    public virtual ICollection<OrganizationContact> OrganizationContacts { get; set; } = new List<OrganizationContact>();
}
