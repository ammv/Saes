using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Saes.Models;

public partial class SaesContext : DbContext
{
    public SaesContext()
    {
    }

    public SaesContext(DbContextOptions<SaesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<BusinessEntity> BusinessEntities { get; set; }

    public virtual DbSet<BusinessEntityType> BusinessEntityTypes { get; set; }

    public virtual DbSet<ContactType> ContactTypes { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeePosition> EmployeePositions { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Hardware> Hardwares { get; set; }

    public virtual DbSet<JournalInstanceCpareceiver> JournalInstanceCpareceivers { get; set; }

    public virtual DbSet<JournalInstanceForCihconnectedHardware> JournalInstanceForCihconnectedHardwares { get; set; }

    public virtual DbSet<JournalInstanceForCihdestructor> JournalInstanceForCihdestructors { get; set; }

    public virtual DbSet<JournalInstanceForCihinstaller> JournalInstanceForCihinstallers { get; set; }

    public virtual DbSet<JournalInstanceForCihrecord> JournalInstanceForCihrecords { get; set; }

    public virtual DbSet<JournalInstanceForCparecord> JournalInstanceForCparecords { get; set; }

    public virtual DbSet<JournalTechnicalRecord> JournalTechnicalRecords { get; set; }

    public virtual DbSet<KeyDocumentType> KeyDocumentTypes { get; set; }

    public virtual DbSet<KeyHolder> KeyHolders { get; set; }

    public virtual DbSet<KeyHolderType> KeyHolderTypes { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<LogAuthentication> LogAuthentications { get; set; }

    public virtual DbSet<LogChange> LogChanges { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<OrganizationContact> OrganizationContacts { get; set; }

    public virtual DbSet<Right> Rights { get; set; }

    public virtual DbSet<RightGroup> RightGroups { get; set; }

    public virtual DbSet<TableColumnDatum> TableColumnData { get; set; }

    public virtual DbSet<TableDatum> TableData { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserRoleRight> UserRoleRights { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<ViewLog> ViewLogs { get; set; }

    public virtual DbSet<ViewLogChange> ViewLogChanges { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=KOMPUTER\\SQLEXPRESS;Initial Catalog=SAES;Integrated Security=True;Connect Timeout=300;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK_Address_AddressID");

            entity.ToTable("Address", "Person", tb =>
                {
                    tb.HasTrigger("Address_AfterInsert");
                    tb.HasTrigger("Address_InsteadOfDelete");
                    tb.HasTrigger("Address_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<BusinessEntity>(entity =>
        {
            entity.HasKey(e => e.BusinessEntityId).HasName("PK_BusinessEntity_BusinessEntityID");

            entity.ToTable("BusinessEntity", "HumanResources", tb =>
                {
                    tb.HasTrigger("BusinessEntity_AfterInsert");
                    tb.HasTrigger("BusinessEntity_InsteadOfDelete");
                    tb.HasTrigger("BusinessEntity_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.BusinessEntityType).WithMany(p => p.BusinessEntities).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<BusinessEntityType>(entity =>
        {
            entity.HasKey(e => e.BusinessEntityTypeId).HasName("PK_BusinessEntityType_BusinessEntityTypeID");

            entity.ToTable("BusinessEntityType", "HumanResources", tb =>
                {
                    tb.HasTrigger("BusinessEntityType_AfterInsert");
                    tb.HasTrigger("BusinessEntityType_InsteadOfDelete");
                    tb.HasTrigger("BusinessEntityType_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<ContactType>(entity =>
        {
            entity.HasKey(e => e.ContactTypeId).HasName("PK_ContactType_ContactTypeID");

            entity.ToTable("ContactType", "Person", tb =>
                {
                    tb.HasTrigger("ContactType_AfterInsert");
                    tb.HasTrigger("ContactType_InsteadOfDelete");
                    tb.HasTrigger("ContactType_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.BusinessEntityId).HasName("PK_Employee_BusinessEntityID");

            entity.ToTable("Employee", "HumanResources", tb =>
                {
                    tb.HasTrigger("Employee_AfterInsert");
                    tb.HasTrigger("Employee_InsteadOfDelete");
                    tb.HasTrigger("Employee_InsteadOfUpdate");
                });

            entity.Property(e => e.BusinessEntityId).ValueGeneratedNever();
            entity.Property(e => e.EmployeeId).ValueGeneratedOnAdd();
            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.BusinessEntity).WithOne(p => p.Employee).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.EmployeePosition).WithMany(p => p.Employees).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Organization).WithMany(p => p.Employees)
                .HasPrincipalKey(p => p.OrganizationId)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<EmployeePosition>(entity =>
        {
            entity.HasKey(e => e.EmployeePositionId).HasName("PK_EmployeePosition_EmployeePositionID");

            entity.ToTable("EmployeePosition", "HumanResources", tb =>
                {
                    tb.HasTrigger("EmployeePosition_AfterInsert");
                    tb.HasTrigger("EmployeePosition_InsteadOfDelete");
                    tb.HasTrigger("EmployeePosition_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.ErrorLogId).HasName("PK_ErrorLog_ErrorLogID");

            entity.Property(e => e.ErrorTime).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK_File_FileID");

            entity.Property(e => e.Guid).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<Hardware>(entity =>
        {
            entity.HasKey(e => e.HardwareId).HasName("PK_Hardware_HardwareID");

            entity.ToTable("Hardware", "Office", tb =>
                {
                    tb.HasTrigger("Hardware_AfterInsert");
                    tb.HasTrigger("Hardware_InsteadOfDelete");
                    tb.HasTrigger("Hardware_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<JournalInstanceCpareceiver>(entity =>
        {
            entity.HasKey(e => e.JournalInstanceCpareceiverId).HasName("PK_JournalInstanceCPAReceiver_JournalInstanceCPAReceiverID");

            entity.ToTable("JournalInstanceCPAReceiver", "AccountingCPI", tb =>
                {
                    tb.HasTrigger("JournalInstanceCPAReceiver_AfterInsert");
                    tb.HasTrigger("JournalInstanceCPAReceiver_InsteadOfDelete");
                    tb.HasTrigger("JournalInstanceCPAReceiver_InsteadOfUpdate");
                });

            entity.Property(e => e.JournalInstanceCpareceiverId).ValueGeneratedOnAdd();
            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.JournalInstanceCpareceiverNavigation).WithOne(p => p.JournalInstanceCpareceiver)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalInstanceCPAReceiver_JournalInstanceForCPARecord_JournalInstanceForCPARecordID");

            entity.HasOne(d => d.Receiver).WithMany(p => p.JournalInstanceCpareceivers).HasConstraintName("FK_JournalInstanceCPAReceiver_BusinessEntity_BusinessEntityID");
        });

        modelBuilder.Entity<JournalInstanceForCihconnectedHardware>(entity =>
        {
            entity.HasKey(e => e.JournalInstanceForCihconnectedHardwareId).HasName("PK_JournalInstanceForCIHConnectedHardware_JournalInstanceForCIHConnectedHardwareID");

            entity.ToTable("JournalInstanceForCIHConnectedHardware", "AccountingCPI", tb =>
                {
                    tb.HasTrigger("JournalInstanceForCIHConnectedHardware_AfterInsert");
                    tb.HasTrigger("JournalInstanceForCIHConnectedHardware_InsteadOfDelete");
                    tb.HasTrigger("JournalInstanceForCIHConnectedHardware_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Hardware).WithMany(p => p.JournalInstanceForCihconnectedHardwares).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Record).WithMany(p => p.JournalInstanceForCihconnectedHardwares)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalInstanceForCIHConnectedHardware_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID");
        });

        modelBuilder.Entity<JournalInstanceForCihdestructor>(entity =>
        {
            entity.HasKey(e => e.JournalInstanceForCihdestructorId).HasName("PK_JournalInstanceForCIHDestructor_JournalInstanceForCIHDestructorID");

            entity.ToTable("JournalInstanceForCIHDestructor", "AccountingCPI", tb =>
                {
                    tb.HasTrigger("JournalInstanceForCIHDestructor_AfterInsert");
                    tb.HasTrigger("JournalInstanceForCIHDestructor_InsteadOfDelete");
                    tb.HasTrigger("JournalInstanceForCIHDestructor_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Destructor).WithMany(p => p.JournalInstanceForCihdestructors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalInstanceForCIHDestructor_BusinessEntity_BusinessEntityID");

            entity.HasOne(d => d.Record).WithMany(p => p.JournalInstanceForCihdestructors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalInstanceForCIHDestructor_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID");
        });

        modelBuilder.Entity<JournalInstanceForCihinstaller>(entity =>
        {
            entity.HasKey(e => e.JournalInstanceForCihinstallerId).HasName("PK_JournalInstanceForCIHInstaller_JournalInstanceForCIHInstallerID");

            entity.ToTable("JournalInstanceForCIHInstaller", "AccountingCPI", tb =>
                {
                    tb.HasTrigger("JournalInstanceForCIHInstaller_AfterInsert");
                    tb.HasTrigger("JournalInstanceForCIHInstaller_InsteadOfDelete");
                    tb.HasTrigger("JournalInstanceForCIHInstaller_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Installer).WithMany(p => p.JournalInstanceForCihinstallers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalInstanceForCIHInstaller_BusinessEntity_BusinessEntityID");

            entity.HasOne(d => d.Record).WithMany(p => p.JournalInstanceForCihinstallers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalInstanceForCIHInstaller_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID");
        });

        modelBuilder.Entity<JournalInstanceForCihrecord>(entity =>
        {
            entity.HasKey(e => e.JournalInstanceForCihrecordId).HasName("PK_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID");

            entity.ToTable("JournalInstanceForCIHRecord", "AccountingCPI", tb =>
                {
                    tb.HasTrigger("JournalInstanceForCIHRecord_AfterInsert");
                    tb.HasTrigger("JournalInstanceForCIHRecord_InsteadOfDelete");
                    tb.HasTrigger("JournalInstanceForCIHRecord_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Cpiuser).WithMany(p => p.JournalInstanceForCihrecordCpiusers).HasConstraintName("FK_JournalInstanceForCIHRecord_CPIUserID_BusinessEntity_BusinessEntityID");

            entity.HasOne(d => d.Organization).WithMany(p => p.JournalInstanceForCihrecords)
                .HasPrincipalKey(p => p.OrganizationId)
                .HasForeignKey(d => d.OrganizationId);

            entity.HasOne(d => d.ReceivedFrom).WithMany(p => p.JournalInstanceForCihrecordReceivedFroms).HasConstraintName("FK_JournalInstanceForCIHRecord_ReceivedFromID_BusinessEntity_BusinessEntityID");

            entity.HasOne(d => d.SignFile).WithMany(p => p.JournalInstanceForCihrecords).HasConstraintName("FK_JournalInstanceForCIHRecord_File_FileID");
        });

        modelBuilder.Entity<JournalInstanceForCparecord>(entity =>
        {
            entity.HasKey(e => e.JournalInstanceForCparecordId).HasName("PK_JournalInstanceForCPARecord_JournalInstanceForCPARecordID");

            entity.ToTable("JournalInstanceForCPARecord", "AccountingCPI", tb =>
                {
                    tb.HasTrigger("JournalInstanceForCPARecord_AfterInsert");
                    tb.HasTrigger("JournalInstanceForCPARecord_InsteadOfDelete");
                    tb.HasTrigger("JournalInstanceForCPARecord_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Organization).WithMany(p => p.JournalInstanceForCparecords)
                .HasPrincipalKey(p => p.OrganizationId)
                .HasForeignKey(d => d.OrganizationId);

            entity.HasOne(d => d.ReceivedFrom).WithMany(p => p.JournalInstanceForCparecords).HasConstraintName("FK_JournalInstanceForCPARecord_BusinessEntity_BusinessEntityID");

            entity.HasOne(d => d.SignFile).WithMany(p => p.JournalInstanceForCparecords).HasConstraintName("FK_JournalInstanceForCPARecord_File_FileID");
        });

        modelBuilder.Entity<JournalTechnicalRecord>(entity =>
        {
            entity.HasKey(e => e.JournalTechnicalRecordId).HasName("PK_JournalTechnicalRecord_JournalTechnicalRecordID");

            entity.ToTable("JournalTechnicalRecord", "AccountingCPI", tb =>
                {
                    tb.HasTrigger("JournalTechnicalRecord_AfterInsert");
                    tb.HasTrigger("JournalTechnicalRecord_InsteadOfDelete");
                    tb.HasTrigger("JournalTechnicalRecord_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Organization).WithMany(p => p.JournalTechnicalRecords)
                .HasPrincipalKey(p => p.OrganizationId)
                .HasForeignKey(d => d.OrganizationId);

            entity.HasOne(d => d.SignFile).WithMany(p => p.JournalTechnicalRecords).HasConstraintName("FK_JournalTechnicalRecord_File_FileID");
        });

        modelBuilder.Entity<KeyDocumentType>(entity =>
        {
            entity.HasKey(e => e.KeyDocumentTypeId).HasName("PK_KeyDocumentType_KeyDocumentTypeID");

            entity.ToTable("KeyDocumentType", "AccountingCPI", tb =>
                {
                    tb.HasTrigger("KeyDocumentType_AfterInsert");
                    tb.HasTrigger("KeyDocumentType_InsteadOfDelete");
                    tb.HasTrigger("KeyDocumentType_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<KeyHolder>(entity =>
        {
            entity.HasKey(e => e.KeyHolderId).HasName("PK_KeyHolder_KeyHolderID");

            entity.ToTable("KeyHolder", "AccountingCPI", tb =>
                {
                    tb.HasTrigger("KeyHolder_AfterInsert");
                    tb.HasTrigger("KeyHolder_InsteadOfDelete");
                    tb.HasTrigger("KeyHolder_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.SignFile).WithMany(p => p.KeyHolders).HasConstraintName("FK_KeyHolder_File_FileID");

            entity.HasOne(d => d.Type).WithMany(p => p.KeyHolders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KeyHolder_KeyHolderType_KeyHolderTypeID");

            entity.HasOne(d => d.UserCpiNavigation).WithMany(p => p.KeyHolders).HasConstraintName("FK_KeyHolder_BusinessEntity_BusinessEntityID");
        });

        modelBuilder.Entity<KeyHolderType>(entity =>
        {
            entity.HasKey(e => e.KeyHolderTypeId).HasName("PK_KeyHolderType_KeyHolderTypeID");

            entity.ToTable("KeyHolderType", "AccountingCPI", tb =>
                {
                    tb.HasTrigger("KeyHolderType_AfterInsert");
                    tb.HasTrigger("KeyHolderType_InsteadOfDelete");
                    tb.HasTrigger("KeyHolderType_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK_Log_LogID");

            entity.Property(e => e.Action).IsFixedLength();
            entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<LogAuthentication>(entity =>
        {
            entity.HasKey(e => e.LogAuthenticationId).HasName("PK_LogAuthentication_LogAuthenticationID");

            entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<LogChange>(entity =>
        {
            entity.HasOne(d => d.AuditLog).WithMany().HasConstraintName("FK_LogChange_Log_LogID");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.BusinessEntityId).HasName("PK_Organization_BusinessEntityID");

            entity.ToTable("Organization", "HumanResources", tb =>
                {
                    tb.HasTrigger("Organization_AfterInsert");
                    tb.HasTrigger("Organization_InsteadOfDelete");
                    tb.HasTrigger("Organization_InsteadOfUpdate");
                });

            entity.Property(e => e.BusinessEntityId).ValueGeneratedNever();
            entity.Property(e => e.OrganizationId).ValueGeneratedOnAdd();
            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.BusinessAddress).WithMany(p => p.Organizations).HasConstraintName("FK_Organization_Address_AddressID");

            entity.HasOne(d => d.BusinessEntity).WithOne(p => p.Organization).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OrganizationContact>(entity =>
        {
            entity.HasKey(e => e.OrganizationContactId).HasName("PK_OrganizationContact_OrganizationContactID");

            entity.ToTable("OrganizationContact", "HumanResources", tb =>
                {
                    tb.HasTrigger("OrganizationContact_AfterInsert");
                    tb.HasTrigger("OrganizationContact_InsteadOfDelete");
                    tb.HasTrigger("OrganizationContact_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ContactType).WithMany(p => p.OrganizationContacts).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Organization).WithMany(p => p.OrganizationContacts)
                .HasPrincipalKey(p => p.OrganizationId)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Right>(entity =>
        {
            entity.HasKey(e => e.RightId).HasName("PK_Right_RightId");

            entity.ToTable("Right", "Authorization", tb =>
                {
                    tb.HasTrigger("Right_AfterInsert");
                    tb.HasTrigger("Right_InsteadOfDelete");
                    tb.HasTrigger("Right_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.RightGroup).WithMany(p => p.Rights).HasConstraintName("FK_Right_RightGroup_RightGroupId");
        });

        modelBuilder.Entity<RightGroup>(entity =>
        {
            entity.HasKey(e => e.RightGroupId).HasName("PK_RightGroup_RightGroupId");

            entity.ToTable("RightGroup", "Authorization", tb =>
                {
                    tb.HasTrigger("RightGroup_AfterInsert");
                    tb.HasTrigger("RightGroup_InsteadOfDelete");
                    tb.HasTrigger("RightGroup_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<TableColumnDatum>(entity =>
        {
            entity.HasKey(e => e.TableColumnDataId).HasName("PK_TableColumnData_TableColumnDataID");

            entity.HasOne(d => d.TableData).WithMany(p => p.TableColumnData).HasConstraintName("FK_TableColumnData_TableData_TableID");
        });

        modelBuilder.Entity<TableDatum>(entity =>
        {
            entity.HasKey(e => e.TableDataId).HasName("PK_TableData_TableDataID");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_User_UserID");

            entity.ToTable("User", "Authentication", tb =>
                {
                    tb.HasTrigger("User_AfterInsert");
                    tb.HasTrigger("User_InsteadOfDelete");
                    tb.HasTrigger("User_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TwoFactorEnabled).HasDefaultValueSql("((1))");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK_UserRole_UserRoleId");

            entity.ToTable("UserRole", "Authorization", tb =>
                {
                    tb.HasTrigger("UserRole_AfterInsert");
                    tb.HasTrigger("UserRole_InsteadOfDelete");
                    tb.HasTrigger("UserRole_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<UserRoleRight>(entity =>
        {
            entity.HasKey(e => e.UserRoleRightId).HasName("PK_UserRoleRight_UserRoleRightId");

            entity.ToTable("UserRoleRight", "Authorization", tb =>
                {
                    tb.HasTrigger("UserRoleRight_AfterInsert");
                    tb.HasTrigger("UserRoleRight_InsteadOfDelete");
                    tb.HasTrigger("UserRoleRight_InsteadOfUpdate");
                });

            entity.Property(e => e.SysModifiedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.UserSessionId).HasName("PK_UserSession_UserSessionID");

            entity.Property(e => e.IsExpired).HasComputedColumnSql("(CONVERT([bit],case when getdate()>[ExpiredAt] then (1) else (0) end,(0)))", false);

            entity.HasOne(d => d.LogAuthentication).WithMany(p => p.UserSessions).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ViewLog>(entity =>
        {
            entity.ToView("viewLog", "Audit");
        });

        modelBuilder.Entity<ViewLogChange>(entity =>
        {
            entity.ToView("viewLogChange", "Audit");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
