using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

[Table("Address", Schema = "Person")]
public partial class Address
{
    [Key]
    [Column("AddressID")]
    public int AddressId { get; set; }

    [StringLength(128)]
    public string? Country { get; set; }

    [StringLength(128)]
    public string? Region { get; set; }

    [StringLength(128)]
    public string? City { get; set; }

    [StringLength(128)]
    public string? District { get; set; }

    [StringLength(128)]
    public string? Street { get; set; }

    [StringLength(16)]
    public string? BuildingNumber { get; set; }

    [StringLength(16)]
    public string? Corpus { get; set; }

    public int? Floor { get; set; }

    [StringLength(16)]
    public string? Flat { get; set; }

    [StringLength(10)]
    public string? PostalIndex { get; set; }

    public bool SysIsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SysModifiedDate { get; set; }

    [InverseProperty("BusinessAddress")]
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
