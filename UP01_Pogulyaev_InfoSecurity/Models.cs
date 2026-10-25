using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace InfoSecurity
{
    [Table("Users")]
    public class User
    {
        public User()
        {
            IsActive = true;
            AuditEvents = new HashSet<AuditEvent>();
            AssignedIncidents = new HashSet<SecurityIncident>();
            Employees = new HashSet<Employee>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, StringLength(50)]
        public string Login { get; set; }

        [Required, StringLength(64)]
        public string PasswordHash { get; set; }

        [Required, StringLength(20)]
        public string Role { get; set; }

        [Required, StringLength(150)]
        public string FIO { get; set; }

        [StringLength(260)]
        public string Photo { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<AuditEvent> AuditEvents { get; set; }
        public virtual ICollection<SecurityIncident> AssignedIncidents { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
    }

    [Table("Departments")]
    public class Department
    {
        public Department()
        {
            Employees = new HashSet<Employee>();
            Assets = new HashSet<SecurityAsset>();
            Policies = new HashSet<SecurityPolicy>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<SecurityAsset> Assets { get; set; }
        public virtual ICollection<SecurityPolicy> Policies { get; set; }
    }

    [Table("Employees")]
    public class Employee
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, StringLength(150)]
        public string FIO { get; set; }

        [Required, StringLength(100)]
        public string Position { get; set; }

        [Required, StringLength(100), EmailAddress]
        public string Email { get; set; }

        [StringLength(25), Phone]
        public string Phone { get; set; }

        public int DepartmentID { get; set; }
        public int? UserID { get; set; }

        [ForeignKey(nameof(DepartmentID))]
        public virtual Department Department { get; set; }

        [ForeignKey(nameof(UserID))]
        public virtual User User { get; set; }
    }

    [Table("SecurityAssets")]
    public class SecurityAsset
    {
        public SecurityAsset()
        {
            Incidents = new HashSet<SecurityIncident>();
            Risks = new HashSet<RiskAssessment>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; }

        [Required, StringLength(60)]
        public string AssetType { get; set; }

        [Required, StringLength(50)]
        public string InventoryNumber { get; set; }

        [Range(1, 5)]
        public int Criticality { get; set; }

        public int DepartmentID { get; set; }

        [ForeignKey(nameof(DepartmentID))]
        public virtual Department Department { get; set; }
        public virtual ICollection<SecurityIncident> Incidents { get; set; }
        public virtual ICollection<RiskAssessment> Risks { get; set; }
    }

    [Table("SecurityIncidents")]
    public class SecurityIncident
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; }

        [Required, StringLength(1000)]
        public string Description { get; set; }

        public DateTime IncidentDate { get; set; }

        [Range(1, 5)]
        public int Severity { get; set; }

        [Required, StringLength(40)]
        public string Status { get; set; }

        public int AssetID { get; set; }
        public int? AssignedUserID { get; set; }

        [ForeignKey(nameof(AssetID))]
        public virtual SecurityAsset Asset { get; set; }

        [ForeignKey(nameof(AssignedUserID))]
        public virtual User AssignedUser { get; set; }
    }

    [Table("RiskAssessments")]
    public class RiskAssessment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int AssetID { get; set; }

        [Required, StringLength(200)]
        public string Threat { get; set; }

        [Required, StringLength(300)]
        public string Vulnerability { get; set; }

        [Range(1, 5)]
        public int Probability { get; set; }

        [Range(1, 5)]
        public int Impact { get; set; }

        public DateTime AssessmentDate { get; set; }

        [Required, StringLength(1000)]
        public string Mitigation { get; set; }

        [NotMapped]
        public int RiskScore => Probability * Impact;

        [ForeignKey(nameof(AssetID))]
        public virtual SecurityAsset Asset { get; set; }
    }

    [Table("SecurityPolicies")]
    public class SecurityPolicy
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, StringLength(180)]
        public string Title { get; set; }

        [Required, StringLength(20)]
        public string Version { get; set; }

        public DateTime ApprovalDate { get; set; }
        public int DepartmentID { get; set; }

        [Required, StringLength(2000)]
        public string Content { get; set; }

        [ForeignKey(nameof(DepartmentID))]
        public virtual Department Department { get; set; }
    }

    [Table("AuditEvents")]
    public class AuditEvent
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public DateTime EventDate { get; set; }
        public int UserID { get; set; }

        [Required, StringLength(100)]
        public string Action { get; set; }

        [Required, StringLength(1000)]
        public string Details { get; set; }

        [StringLength(45)]
        public string IpAddress { get; set; }

        [ForeignKey(nameof(UserID))]
        public virtual User User { get; set; }
    }

    public class InfoSecurityEntities : DbContext
    {
        static InfoSecurityEntities()
        {
            Database.SetInitializer<InfoSecurityEntities>(null);
        }

        public InfoSecurityEntities() : base("name=InfoSecurityEntities")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<SecurityAsset> Assets { get; set; }
        public DbSet<SecurityIncident> Incidents { get; set; }
        public DbSet<RiskAssessment> Risks { get; set; }
        public DbSet<SecurityPolicy> Policies { get; set; }
        public DbSet<AuditEvent> AuditEvents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Department>().HasMany(x => x.Employees)
                .WithRequired(x => x.Department).HasForeignKey(x => x.DepartmentID).WillCascadeOnDelete(true);
            modelBuilder.Entity<Department>().HasMany(x => x.Assets)
                .WithRequired(x => x.Department).HasForeignKey(x => x.DepartmentID).WillCascadeOnDelete(true);
            modelBuilder.Entity<Department>().HasMany(x => x.Policies)
                .WithRequired(x => x.Department).HasForeignKey(x => x.DepartmentID).WillCascadeOnDelete(true);

            modelBuilder.Entity<User>().HasMany(x => x.AuditEvents)
                .WithRequired(x => x.User).HasForeignKey(x => x.UserID).WillCascadeOnDelete(true);
            modelBuilder.Entity<User>().HasMany(x => x.AssignedIncidents)
                .WithOptional(x => x.AssignedUser).HasForeignKey(x => x.AssignedUserID).WillCascadeOnDelete(false);
            modelBuilder.Entity<User>().HasMany(x => x.Employees)
                .WithOptional(x => x.User).HasForeignKey(x => x.UserID).WillCascadeOnDelete(false);

            modelBuilder.Entity<SecurityAsset>().HasMany(x => x.Incidents)
                .WithRequired(x => x.Asset).HasForeignKey(x => x.AssetID).WillCascadeOnDelete(true);
            modelBuilder.Entity<SecurityAsset>().HasMany(x => x.Risks)
                .WithRequired(x => x.Asset).HasForeignKey(x => x.AssetID).WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}
