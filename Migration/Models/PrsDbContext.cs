using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Migration.Models;

namespace Migration;

public partial class PrsDbContext : DbContext
{
    public PrsDbContext(DbContextOptions<PrsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<AppFile> AppFiles { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<ApplicationSetting> ApplicationSettings { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<ContactType> ContactTypes { get; set; }

    public virtual DbSet<Council> Councils { get; set; }

    public virtual DbSet<CouncilContact> CouncilContacts { get; set; }

    public virtual DbSet<Dashboard> Dashboards { get; set; }

    public virtual DbSet<DashboardContent> DashboardContents { get; set; }

    public virtual DbSet<DashboardItem> DashboardItems { get; set; }

    public virtual DbSet<FileType> FileTypes { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobAssignmentType> JobAssignmentTypes { get; set; }

    public virtual DbSet<JobColour> JobColours { get; set; }

    public virtual DbSet<JobFile> JobFiles { get; set; }

    public virtual DbSet<JobNote> JobNotes { get; set; }

    public virtual DbSet<JobQuote> JobQuotes { get; set; }

    public virtual DbSet<JobStatus> JobStatuses { get; set; }

    public virtual DbSet<JobStatusHistory> JobStatusHistories { get; set; }

    public virtual DbSet<JobTask> JobTasks { get; set; }

    public virtual DbSet<JobTaskType> JobTaskTypes { get; set; }

    public virtual DbSet<JobType> JobTypes { get; set; }

    public virtual DbSet<JobUser> JobUsers { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationType> NotificationTypes { get; set; }

    public virtual DbSet<Quote> Quotes { get; set; }

    public virtual DbSet<QuoteItem> QuoteItems { get; set; }

    public virtual DbSet<QuoteNote> QuoteNotes { get; set; }

    public virtual DbSet<QuoteStatus> QuoteStatuses { get; set; }

    public virtual DbSet<QuoteStatusHistory> QuoteStatusHistories { get; set; }

    public virtual DbSet<QuoteTemplate> QuoteTemplates { get; set; }

    public virtual DbSet<QuoteTemplateItem> QuoteTemplateItems { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<ScheduleColour> ScheduleColours { get; set; }

    public virtual DbSet<ScheduleTrack> ScheduleTracks { get; set; }

    public virtual DbSet<ScheduleUser> ScheduleUsers { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<TechnicalContact> TechnicalContacts { get; set; }

    public virtual DbSet<TechnicalContactType> TechnicalContactTypes { get; set; }

    public virtual DbSet<TimesheetEntry> TimesheetEntries { get; set; }

    public virtual DbSet<TimesheetEntryType> TimesheetEntryTypes { get; set; }

    public virtual DbSet<XeroAccess> XeroAccesses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("postgis");

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("address_pkey");

            entity.ToTable("address", tb => tb.HasComment("Physical addresses for contacts and jobs"));

            entity.HasIndex(e => e.CreatedByUserId, "idx_address_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_address_deleted_at");

            entity.HasIndex(e => e.Geohash, "idx_address_geo_hash");

            entity.HasIndex(e => e.Geom, "idx_address_geom").HasMethod("gist");

            entity.HasIndex(e => e.SearchVector, "idx_address_search_vector").HasMethod("gin");

            entity.HasIndex(e => e.StateId, "idx_address_state_id");

            entity.HasIndex(e => e.Street, "idx_address_street_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.Suburb, "idx_address_suburb_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.Id, "idx_council_contact_address_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft delete TIMESTAMPTZ - NULL means active")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Geohash)
                .HasMaxLength(12)
                .HasColumnName("geohash");
            entity.Property(e => e.Geom)
                .HasColumnType("geometry(Point,4326)")
                .HasColumnName("geom");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.PostCode)
                .HasMaxLength(10)
                .HasColumnName("post_code");
            entity.Property(e => e.SearchVector)
                .HasComputedColumnSql("((setweight(to_tsvector('english'::regconfig, (COALESCE(street, ''::character varying))::text), 'A'::\"char\") || setweight(to_tsvector('english'::regconfig, (COALESCE(suburb, ''::character varying))::text), 'B'::\"char\")) || setweight(to_tsvector('english'::regconfig, (COALESCE(post_code, ''::character varying))::text), 'C'::\"char\"))", true)
                .HasColumnName("search_vector");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("street");
            entity.Property(e => e.Suburb)
                .HasMaxLength(100)
                .HasColumnName("suburb");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.AddressCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("address_created_by_user_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.AddressModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("address_modified_by_user_id_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.StateId)
                .HasConstraintName("address_state_id_fkey");
        });

        modelBuilder.Entity<AppFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("app_file_pkey");

            entity.ToTable("app_file", tb => tb.HasComment("File metadata and storage references"));

            entity.HasIndex(e => e.ExternalId, "app_file_external_id_key").IsUnique();

            entity.HasIndex(e => e.CreatedByUserId, "idx_file_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_file_deleted_at");

            entity.HasIndex(e => e.ExternalId, "idx_file_external_id");

            entity.HasIndex(e => e.FileHash, "idx_file_hash");

            entity.HasIndex(e => e.FileTypeId, "idx_file_type");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ContentSize).HasColumnName("content_size");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft delete TIMESTAMPTZ - NULL means active")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description)
                .HasMaxLength(400)
                .HasColumnName("description");
            entity.Property(e => e.ExternalId)
                .HasMaxLength(255)
                .HasComment("Reference to external storage system (S3, etc)")
                .HasColumnName("external_id");
            entity.Property(e => e.FileExtension)
                .HasMaxLength(10)
                .HasColumnName("file_extension");
            entity.Property(e => e.FileHash)
                .HasMaxLength(64)
                .HasComment("SHA-256 hash for duplicate detection")
                .HasColumnName("file_hash");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FilePath)
                .HasMaxLength(255)
                .HasColumnName("file_path");
            entity.Property(e => e.FileTypeId).HasColumnName("file_type_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("modified_on");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.AppFileCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("app_file_created_by_user_id_fkey");

            entity.HasOne(d => d.FileType).WithMany(p => p.AppFiles)
                .HasForeignKey(d => d.FileTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("app_file_file_type_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.AppFileModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("app_file_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("app_user_pkey");

            entity.ToTable("app_user", tb => tb.HasComment("Application users with authentication and profile information"));

            entity.HasIndex(e => e.Email, "app_user_email_key").IsUnique();

            entity.HasIndex(e => e.IdentityId, "app_user_identity_id_key").IsUnique();

            entity.HasIndex(e => e.DeactivatedAt, "idx_app_user_deactivated_at");

            entity.HasIndex(e => e.Email, "idx_app_user_email");

            entity.HasIndex(e => e.IdentityId, "idx_app_user_identity_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeactivatedAt)
                .HasComment("NULL = active user, TIMESTAMPTZ = deactivated user")
                .HasColumnName("deactivated_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .HasColumnName("display_name");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IdentityId)
                .HasMaxLength(255)
                .HasColumnName("identity_id");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
            entity.Property(e => e.LegacyUserId).HasColumnName("legacy_user_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.XeroEmployeeId)
                .HasMaxLength(255)
                .HasColumnName("xero_employee_id");
            entity.Property(e => e.XeroLoginEmail)
                .HasMaxLength(100)
                .HasColumnName("xero_login_email");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.InverseModifiedByUser)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("app_user_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<ApplicationSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("application_setting_pkey");

            entity.ToTable("application_setting");

            entity.HasIndex(e => e.Key, "application_settings_key_unique").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.Key)
                .HasMaxLength(255)
                .HasColumnName("key");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("modified_at");
            entity.Property(e => e.Value)
                .HasColumnType("jsonb")
                .HasColumnName("value");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contact_pkey");

            entity.ToTable("contact", tb => tb.HasComment("Client or vendor contact information"));

            entity.HasIndex(e => e.AddressId, "idx_contact_address_id");

            entity.HasIndex(e => e.CreatedByUserId, "idx_contact_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_contact_deleted_at");

            entity.HasIndex(e => e.Email, "idx_contact_email");

            entity.HasIndex(e => e.Email, "idx_contact_email_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.FullName, "idx_contact_name_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.ParentContactId, "idx_contact_parent_contact_id");

            entity.HasIndex(e => e.Phone, "idx_contact_phone_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.SearchVector, "idx_contact_search_vector").HasMethod("gin");

            entity.HasIndex(e => e.TypeId, "idx_contact_type_id");

            entity.HasIndex(e => e.Id, "idx_council_contact_contact_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft delete TIMESTAMPTZ - NULL means active")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fax)
                .HasMaxLength(50)
                .HasColumnName("fax");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.FullName)
                .HasMaxLength(201)
                .HasComputedColumnSql("(((first_name)::text || ' '::text) || (last_name)::text)", true)
                .HasColumnName("full_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.Mobile)
                .HasMaxLength(50)
                .HasColumnName("mobile");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.ParentContactId).HasColumnName("parent_contact_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.SearchVector)
                .HasComputedColumnSql("(((setweight(to_tsvector('english'::regconfig, (COALESCE(first_name, ''::character varying))::text), 'A'::\"char\") || setweight(to_tsvector('english'::regconfig, (COALESCE(last_name, ''::character varying))::text), 'B'::\"char\")) || setweight(to_tsvector('english'::regconfig, (COALESCE(email, ''::character varying))::text), 'C'::\"char\")) || setweight(to_tsvector('english'::regconfig, (COALESCE(phone, ''::character varying))::text), 'D'::\"char\"))", true)
                .HasColumnName("search_vector");
            entity.Property(e => e.TypeId).HasColumnName("type_id");

            entity.HasOne(d => d.Address).WithMany(p => p.Contacts)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("contact_address_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ContactCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contact_created_by_user_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.ContactModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("contact_modified_by_user_id_fkey");

            entity.HasOne(d => d.ParentContact).WithMany(p => p.InverseParentContact)
                .HasForeignKey(d => d.ParentContactId)
                .HasConstraintName("contact_parent_contact_id_fkey");

            entity.HasOne(d => d.Type).WithMany(p => p.Contacts)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contact_type_id_fkey");
        });

        modelBuilder.Entity<ContactType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contact_type_pkey");

            entity.ToTable("contact_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Council>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("council_pkey");

            entity.ToTable("council", tb => tb.HasComment("Council information"));

            entity.HasIndex(e => e.AddressId, "idx_council_address_id");

            entity.HasIndex(e => e.Id, "idx_council_conact_council_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fax)
                .HasMaxLength(50)
                .HasColumnName("fax");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.Website)
                .HasMaxLength(255)
                .HasColumnName("website");

            entity.HasOne(d => d.Address).WithMany(p => p.Councils)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("council_address_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CouncilCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("council_created_by_user_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.CouncilModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("council_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<CouncilContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("council_contact_pkey");

            entity.ToTable("council_contact");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.CouncilId).HasColumnName("council_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");

            entity.HasOne(d => d.Address).WithMany(p => p.CouncilContacts)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("council_contact_address_id_fkey");

            entity.HasOne(d => d.Contact).WithMany(p => p.CouncilContacts)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("council_contact_contact_id_fkey");

            entity.HasOne(d => d.Council).WithMany(p => p.CouncilContacts)
                .HasForeignKey(d => d.CouncilId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("council_contact_council_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CouncilContactCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("council_contact_created_by_user_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.CouncilContactModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("council_contact_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<Dashboard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dashboard_pkey");

            entity.ToTable("dashboard");

            entity.HasIndex(e => e.UserId, "idx_user_dashboard_user");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DashboardX)
                .HasDefaultValue(12)
                .HasColumnName("dashboard_x");
            entity.Property(e => e.DashboardY)
                .HasDefaultValue(12)
                .HasColumnName("dashboard_y");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasDefaultValueSql("'Dashboard'::character varying")
                .HasColumnName("name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Dashboards)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("dashboard_user_id_fkey");
        });

        modelBuilder.Entity<DashboardContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dashboard_content_pkey");

            entity.ToTable("dashboard_content", tb => tb.HasComment("Holds widgets defined in the front end."));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<DashboardItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dashboard_item_pkey");

            entity.ToTable("dashboard_item");

            entity.HasIndex(e => e.ContentId, "idx_dashboard_item_content");

            entity.HasIndex(e => e.DashboardId, "idx_dashboard_item_dashboard_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Colspan).HasColumnName("colspan");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.CustomTitle)
                .HasMaxLength(100)
                .HasColumnName("custom_title");
            entity.Property(e => e.DashboardId).HasColumnName("dashboard_id");
            entity.Property(e => e.IsHidden)
                .HasDefaultValue(false)
                .HasColumnName("is_hidden");
            entity.Property(e => e.PositionX).HasColumnName("position_x");
            entity.Property(e => e.PositionY).HasColumnName("position_y");
            entity.Property(e => e.Rowspan).HasColumnName("rowspan");
            entity.Property(e => e.Settings)
                .HasColumnType("jsonb")
                .HasColumnName("settings");

            entity.HasOne(d => d.Content).WithMany(p => p.DashboardItems)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dashboard_item_content_id_fkey");

            entity.HasOne(d => d.Dashboard).WithMany(p => p.DashboardItems)
                .HasForeignKey(d => d.DashboardId)
                .HasConstraintName("dashboard_item_dashboard_id_fkey");
        });

        modelBuilder.Entity<FileType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("file_type_pkey");

            entity.ToTable("file_type", tb => tb.HasComment("File type and metadata"));

            entity.HasIndex(e => e.JobTypeId, "idx_file_type_job_type_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.JobType).WithMany(p => p.FileTypes)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("file_type_job_type_id_fkey");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoice_pkey");

            entity.ToTable("invoice");

            entity.HasIndex(e => e.ContactId, "idx_invoice_contact_id");

            entity.HasIndex(e => e.CreatedByUserId, "idx_invoice_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_invoice_deleted_at");

            entity.HasIndex(e => e.JobId, "idx_invoice_job_id");

            entity.HasIndex(e => e.ModifiedByUserId, "idx_invoice_modified_by");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(10, 2)
                .HasColumnName("total_price");

            entity.HasOne(d => d.Contact).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("invoice_contact_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.InvoiceCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoice_created_by_user_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("invoice_job_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.InvoiceModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("invoice_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_pkey");

            entity.ToTable("job", tb => tb.HasComment("Main job/project tracking with invoicing"));

            entity.HasIndex(e => e.AddressId, "idx_job_address_id");

            entity.HasIndex(e => e.JobColourId, "idx_job_colour_id");

            entity.HasIndex(e => e.ContactId, "idx_job_contact_id");

            entity.HasIndex(e => e.CouncilId, "idx_job_council_id");

            entity.HasIndex(e => e.CreatedByUserId, "idx_job_created_by");

            entity.HasIndex(e => e.CreatedOn, "idx_job_created_on");

            entity.HasIndex(e => e.DeletedAt, "idx_job_deleted_at");

            entity.HasIndex(e => e.InvoiceNumber, "idx_job_invoice_number");

            entity.HasIndex(e => e.JobNumber, "idx_job_number");

            entity.HasIndex(e => e.JobNumber, "idx_job_number_text_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.StatusId, "idx_job_status_id");

            entity.HasIndex(e => e.InvoiceNumber, "job_invoice_number_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.CouncilId).HasColumnName("council_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft delete TIMESTAMPTZ - NULL means active")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(255)
                .HasColumnName("invoice_number");
            entity.Property(e => e.JobColourId).HasColumnName("job_colour_id");
            entity.Property(e => e.JobNumber)
                .HasMaxLength(50)
                .HasComment("Used in junction with the job type to identify the job. With either be type Construction or Surveying")
                .HasColumnName("job_number");
            entity.Property(e => e.LatestClientUpdate).HasColumnName("latest_client_update");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TargetDeliveryDate).HasColumnName("target_delivery_date");

            entity.HasOne(d => d.Address).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("job_address_id_fkey");

            entity.HasOne(d => d.Contact).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_contact_id_fkey");

            entity.HasOne(d => d.Council).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.CouncilId)
                .HasConstraintName("job_council_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.JobCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_created_by_user_id_fkey");

            entity.HasOne(d => d.JobColour).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.JobColourId)
                .HasConstraintName("job_job_colour_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.JobModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("job_modified_by_user_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("job_status_id_fkey");

            entity.HasMany(d => d.JobTypes).WithMany(p => p.Jobs)
                .UsingEntity<Dictionary<string, object>>(
                    "JobToType",
                    r => r.HasOne<JobType>().WithMany()
                        .HasForeignKey("JobTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("job_to_type_job_type_id_fkey"),
                    l => l.HasOne<Job>().WithMany()
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("job_to_type_job_id_fkey"),
                    j =>
                    {
                        j.HasKey("JobId", "JobTypeId").HasName("job_to_type_pkey");
                        j.ToTable("job_to_type", tb => tb.HasComment("Jobs can have multiple types. "));
                        j.HasIndex(new[] { "JobId" }, "idx_job_to_type_job");
                        j.HasIndex(new[] { "JobTypeId" }, "idx_job_to_type_type");
                        j.IndexerProperty<int>("JobId").HasColumnName("job_id");
                        j.IndexerProperty<int>("JobTypeId").HasColumnName("job_type_id");
                    });
        });

        modelBuilder.Entity<JobAssignmentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_assignment_type_pkey");

            entity.ToTable("job_assignment_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<JobColour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_colour_pkey");

            entity.ToTable("job_colour", tb => tb.HasComment("Colour of job"));

            entity.HasIndex(e => e.Color, "job_color_unique").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasColumnName("color");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<JobFile>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.FileId }).HasName("job_file_pkey");

            entity.ToTable("job_file", tb => tb.HasComment("Files attached to specific jobs"));

            entity.HasIndex(e => e.CreatedByUserId, "idx_job_file_created_by");

            entity.HasIndex(e => e.FileId, "idx_job_file_file_id");

            entity.HasIndex(e => e.JobId, "idx_job_file_job_id");

            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.FileId).HasColumnName("file_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.JobFiles)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_file_created_by_user_id_fkey");

            entity.HasOne(d => d.File).WithMany(p => p.JobFiles)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_file_file_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.JobFiles)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_file_job_id_fkey");
        });

        modelBuilder.Entity<JobNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_note_pkey");

            entity.ToTable("job_note", tb => tb.HasComment("Many-to-many relationship between users and jobs"));

            entity.HasIndex(e => e.CreatedByUserId, "idx_job_note_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_job_note_deleted_at");

            entity.HasIndex(e => e.JobId, "idx_job_note_job_id");

            entity.HasIndex(e => e.AssignedUserId, "idx_job_note_user_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ActionRequired)
                .HasDefaultValue(false)
                .HasColumnName("action_required");
            entity.Property(e => e.AssignedUserId).HasColumnName("assigned_user_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft delete TIMESTAMPTZ - NULL means to show")
                .HasColumnName("deleted_at");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.Note).HasColumnName("note");

            entity.HasOne(d => d.AssignedUser).WithMany(p => p.JobNoteAssignedUsers)
                .HasForeignKey(d => d.AssignedUserId)
                .HasConstraintName("job_note_assigned_user_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.JobNoteCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_note_created_by_user_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.JobNotes)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_note_job_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.JobNoteModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("job_note_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<JobQuote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_quote_pkey");

            entity.ToTable("job_quote", tb => tb.HasComment("Links quotes to jobs"));

            entity.HasIndex(e => e.CreatedByUserId, "idx_job_quote_created_by");

            entity.HasIndex(e => e.JobId, "idx_job_quote_job_id");

            entity.HasIndex(e => e.QuoteId, "idx_job_quote_quote_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.JobQuotes)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_quote_created_by_user_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.JobQuotes)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_quote_job_id_fkey");

            entity.HasOne(d => d.Quote).WithMany(p => p.JobQuotes)
                .HasForeignKey(d => d.QuoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_quote_quote_id_fkey");
        });

        modelBuilder.Entity<JobStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_status_pkey");

            entity.ToTable("job_status", tb => tb.HasComment("The status of a Job"));

            entity.HasIndex(e => e.JobTypeId, "idx_job_status_type_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Colour)
                .HasMaxLength(12)
                .HasColumnName("colour");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Sequence).HasColumnName("sequence");

            entity.HasOne(d => d.JobType).WithMany(p => p.JobStatuses)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_status_job_type_id_fkey");
        });

        modelBuilder.Entity<JobStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_status_history_pkey");

            entity.ToTable("job_status_history", tb => tb.HasComment("Track the status history of the job."));

            entity.HasIndex(e => e.JobId, "idx_job_id");

            entity.HasIndex(e => e.ModifiedByUserId, "idx_job_status_modified_by");

            entity.HasIndex(e => e.StatusIdNew, "idx_job_status_new");

            entity.HasIndex(e => e.StatusIdOld, "idx_job_status_old");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.DateChanged).HasColumnName("date_changed");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.StatusIdNew).HasColumnName("status_id_new");
            entity.Property(e => e.StatusIdOld).HasColumnName("status_id_old");

            entity.HasOne(d => d.Job).WithMany(p => p.JobStatusHistories)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_status_history_job_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.JobStatusHistories)
                .HasForeignKey(d => d.ModifiedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_status_history_modified_by_user_id_fkey");

            entity.HasOne(d => d.StatusIdNewNavigation).WithMany(p => p.JobStatusHistoryStatusIdNewNavigations)
                .HasForeignKey(d => d.StatusIdNew)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_status_history_status_id_new_fkey");

            entity.HasOne(d => d.StatusIdOldNavigation).WithMany(p => p.JobStatusHistoryStatusIdOldNavigations)
                .HasForeignKey(d => d.StatusIdOld)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_status_history_status_id_old_fkey");
        });

        modelBuilder.Entity<JobTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_task_pkey");

            entity.ToTable("job_task");

            entity.HasIndex(e => e.CreatedByUserId, "idx_task_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_task_deleted_at");

            entity.HasIndex(e => e.JobId, "idx_task_job_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ActiveDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("active_date");
            entity.Property(e => e.CompletedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("completed_date");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.InvoiceRequired)
                .HasDefaultValue(false)
                .HasColumnName("invoice_required");
            entity.Property(e => e.InvoicedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("invoiced_date");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.QuotedPrice)
                .HasPrecision(10, 2)
                .HasColumnName("quoted_price");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.JobTaskCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_task_created_by_user_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.JobTasks)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_task_job_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.JobTaskModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("job_task_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<JobTaskType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_task_type_pkey");

            entity.ToTable("job_task_type");

            entity.HasIndex(e => e.JobTypeId, "idx_job_task_type_job_type_id");

            entity.HasIndex(e => e.Name, "idx_job_task_type_name");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.JobType).WithMany(p => p.JobTaskTypes)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_task_type_job_type_id_fkey");
        });

        modelBuilder.Entity<JobType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_type_pkey");

            entity.ToTable("job_type", tb => tb.HasComment("Type of job"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Abbreviation)
                .HasMaxLength(15)
                .HasColumnName("abbreviation");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("Construction = Set out. Survey = CAD.")
                .HasColumnName("name");
        });

        modelBuilder.Entity<JobUser>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.JobId, e.AssignmentTypeId }).HasName("job_user_pkey");

            entity.ToTable("job_user", tb => tb.HasComment("Many-to-many relationship between users and jobs"));

            entity.HasIndex(e => e.AssignmentTypeId, "idx_job_user_assignment_type_id");

            entity.HasIndex(e => e.DeletedAt, "idx_job_user_deleted_at");

            entity.HasIndex(e => e.JobId, "idx_job_user_job_id");

            entity.HasIndex(e => e.UserId, "idx_job_user_user_id");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.AssignmentTypeId).HasColumnName("assignment_type_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft delete TIMESTAMPTZ - NULL means active assignment")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");

            entity.HasOne(d => d.AssignmentType).WithMany(p => p.JobUsers)
                .HasForeignKey(d => d.AssignmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_user_assignment_type_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.JobUserCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_user_created_by_user_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.JobUsers)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_user_job_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.JobUserModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("job_user_modified_by_user_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.JobUserUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_user_user_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_pkey");

            entity.ToTable("notification");

            entity.HasIndex(e => e.NotificationTypeId, "idx_notification_type_id");

            entity.HasIndex(e => e.UserId, "idx_user_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NotificationTypeId).HasColumnName("notification_type_id");
            entity.Property(e => e.Payload)
                .HasColumnType("jsonb")
                .HasColumnName("payload");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.NotificationType).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.NotificationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notification_notification_type_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notification_user_id_fkey");
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_type_pkey");

            entity.ToTable("notification_type");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quote_pkey");

            entity.ToTable("quote", tb => tb.HasComment("A quote for a job"));

            entity.HasIndex(e => e.AddressId, "idx_quote_address_id");

            entity.HasIndex(e => e.ContactId, "idx_quote_contact_id");

            entity.HasIndex(e => e.CreatedByUserId, "idx_quote_created_by");

            entity.HasIndex(e => e.JobId, "idx_quote_job_id");

            entity.HasIndex(e => e.JobTypeId, "idx_quote_job_type_id");

            entity.HasIndex(e => e.ModifiedByUserId, "idx_quote_modified_by");

            entity.HasIndex(e => e.StatusId, "idx_quote_status_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DateAccepted).HasColumnName("date_accepted");
            entity.Property(e => e.DateSentToClient).HasColumnName("date_sent_to_client");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.QuoteReference)
                .HasMaxLength(50)
                .HasColumnName("quote_reference");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TargetDeliveryDate).HasColumnName("target_delivery_date");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(12, 2)
                .HasColumnName("total_price");

            entity.HasOne(d => d.Address).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("quote_address_id_fkey");

            entity.HasOne(d => d.Contact).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("quote_contact_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.QuoteCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_created_by_user_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("quote_job_id_fkey");

            entity.HasOne(d => d.JobType).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_job_type_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.QuoteModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("quote_modified_by_user_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_status_id_fkey");
        });

        modelBuilder.Entity<QuoteItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quote_item_pkey");

            entity.ToTable("quote_item");

            entity.HasIndex(e => e.QuoteId, "idx_quote_item_quote_id");

            entity.HasIndex(e => e.ServiceId, "idx_quote_item_service_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.ServiceNameSnapshot)
                .HasMaxLength(150)
                .HasColumnName("service_name_snapshot");
            entity.Property(e => e.Total)
                .HasPrecision(12, 2)
                .HasColumnName("total");

            entity.HasOne(d => d.Quote).WithMany(p => p.QuoteItems)
                .HasForeignKey(d => d.QuoteId)
                .HasConstraintName("quote_item_quote_id_fkey");

            entity.HasOne(d => d.Service).WithMany(p => p.QuoteItems)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("quote_item_service_id_fkey");
        });

        modelBuilder.Entity<QuoteNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quote_note_pkey");

            entity.ToTable("quote_note", tb => tb.HasComment("Notes specifically attached to a quote (One-to-Many: Quote to Note)."));

            entity.HasIndex(e => e.CreatedByUserId, "idx_quote_note_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_quote_note_deleted_at");

            entity.HasIndex(e => e.QuoteId, "idx_quote_note_quote_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.QuoteNoteCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_note_created_by_user_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.QuoteNoteModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("quote_note_modified_by_user_id_fkey");

            entity.HasOne(d => d.Quote).WithMany(p => p.QuoteNotes)
                .HasForeignKey(d => d.QuoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_note_quote_id_fkey");
        });

        modelBuilder.Entity<QuoteStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quote_status_pkey");

            entity.ToTable("quote_status", tb => tb.HasComment("Holds the status of a quote"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Colour)
                .HasMaxLength(10)
                .HasColumnName("colour");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<QuoteStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quote_status_history_pkey");

            entity.ToTable("quote_status_history", tb => tb.HasComment("Track the status history of the quote."));

            entity.HasIndex(e => e.QuoteId, "idx_quote_history_id");

            entity.HasIndex(e => e.ModifiedByUserId, "idx_quote_status_modified_by");

            entity.HasIndex(e => e.StatusIdNew, "idx_quote_status_new");

            entity.HasIndex(e => e.StatusIdOld, "idx_quote_status_old");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.DateChanged).HasColumnName("date_changed");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");
            entity.Property(e => e.StatusIdNew).HasColumnName("status_id_new");
            entity.Property(e => e.StatusIdOld).HasColumnName("status_id_old");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.QuoteStatusHistories)
                .HasForeignKey(d => d.ModifiedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_status_history_modified_by_user_id_fkey");

            entity.HasOne(d => d.Quote).WithMany(p => p.QuoteStatusHistories)
                .HasForeignKey(d => d.QuoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_status_history_quote_id_fkey");

            entity.HasOne(d => d.StatusIdNewNavigation).WithMany(p => p.QuoteStatusHistoryStatusIdNewNavigations)
                .HasForeignKey(d => d.StatusIdNew)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_status_history_status_id_new_fkey");

            entity.HasOne(d => d.StatusIdOldNavigation).WithMany(p => p.QuoteStatusHistoryStatusIdOldNavigations)
                .HasForeignKey(d => d.StatusIdOld)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_status_history_status_id_old_fkey");
        });

        modelBuilder.Entity<QuoteTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quote_template_pkey");

            entity.ToTable("quote_template", tb => tb.HasComment("Named reusable templates; applying one copies lines into a new quote as quote_item rows."));

            entity.HasIndex(e => e.IsActive, "idx_quote_template_active").HasFilter("(deleted_at IS NULL)");

            entity.HasIndex(e => e.CreatedByUserId, "idx_quote_template_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_quote_template_deleted_at");

            entity.HasIndex(e => e.JobTypeId, "idx_quote_template_job_type_id");

            entity.HasIndex(e => e.SortOrder, "idx_quote_template_sort");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JobTypeId)
                .HasComment("Optional hint e.g. Construction vs Survey when picking a template in the UI.")
                .HasColumnName("job_type_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.QuoteTemplateCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_template_created_by_user_id_fkey");

            entity.HasOne(d => d.JobType).WithMany(p => p.QuoteTemplates)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("quote_template_job_type_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.QuoteTemplateModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("quote_template_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<QuoteTemplateItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quote_template_item_pkey");

            entity.ToTable("quote_template_item", tb => tb.HasComment("Default lines for a quote_template; copy to quote_item when a template is applied."));

            entity.HasIndex(e => e.ServiceId, "idx_quote_template_item_service_id");

            entity.HasIndex(e => e.QuoteTemplateId, "idx_quote_template_item_template_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.DefaultPrice)
                .HasPrecision(12, 2)
                .HasColumnName("default_price");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.QuoteTemplateId).HasColumnName("quote_template_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.ServiceNameSnapshot)
                .HasMaxLength(150)
                .HasColumnName("service_name_snapshot");

            entity.HasOne(d => d.QuoteTemplate).WithMany(p => p.QuoteTemplateItems)
                .HasForeignKey(d => d.QuoteTemplateId)
                .HasConstraintName("quote_template_item_quote_template_id_fkey");

            entity.HasOne(d => d.Service).WithMany(p => p.QuoteTemplateItems)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("quote_template_item_service_id_fkey");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schedule_pkey");

            entity.ToTable("schedule", tb => tb.HasComment("User schedules for work hours."));

            entity.HasIndex(e => e.ScheduleColourId, "idx_schedule_colour_id");

            entity.HasIndex(e => e.CreatedByUserId, "idx_schedule_created_by");

            entity.HasIndex(e => e.EndTime, "idx_schedule_end_time");

            entity.HasIndex(e => e.JobId, "idx_schedule_job_id");

            entity.HasIndex(e => e.StartTime, "idx_schedule_start_time");

            entity.HasIndex(e => e.ScheduleTrackId, "idx_schedule_track_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.EndTime)
                .HasComment("End time of the schedule")
                .HasColumnName("end_time");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.ScheduleColourId).HasColumnName("schedule_colour_id");
            entity.Property(e => e.ScheduleTrackId).HasColumnName("schedule_track_id");
            entity.Property(e => e.StartTime)
                .HasComment("Start time of the schedule")
                .HasColumnName("start_time");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ScheduleCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_created_by_user_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("schedule_job_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.ScheduleModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("schedule_modified_by_user_id_fkey");

            entity.HasOne(d => d.ScheduleColour).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.ScheduleColourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_schedule_colour_id_fkey");

            entity.HasOne(d => d.ScheduleTrack).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.ScheduleTrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_schedule_track_id_fkey");
        });

        modelBuilder.Entity<ScheduleColour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schedule_colour_pkey");

            entity.ToTable("schedule_colour", tb => tb.HasComment("Colour of the schedule"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasColumnName("color");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
        });

        modelBuilder.Entity<ScheduleTrack>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schedule_track_pkey");

            entity.ToTable("schedule_track");

            entity.HasIndex(e => e.CreatedByUserId, "idx_schedule_track_created_by_user_id");

            entity.HasIndex(e => e.JobTypeId, "idx_schedule_track_job_type_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ScheduleTrackCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_track_created_by_user_id_fkey");

            entity.HasOne(d => d.JobType).WithMany(p => p.ScheduleTracks)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_track_job_type_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.ScheduleTrackModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("schedule_track_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<ScheduleUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schedule_user_pkey");

            entity.ToTable("schedule_user");

            entity.HasIndex(e => e.CreatedByUserId, "idx_schedule_users_created_by_id");

            entity.HasIndex(e => e.ScheduleTrackId, "idx_schedule_users_schedule_track_id");

            entity.HasIndex(e => e.UserId, "idx_schedule_users_user_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.ScheduleTrackId).HasColumnName("schedule_track_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ScheduleUserCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_user_created_by_user_id_fkey");

            entity.HasOne(d => d.ScheduleTrack).WithMany(p => p.ScheduleUsers)
                .HasForeignKey(d => d.ScheduleTrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_user_schedule_track_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ScheduleUserUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_user_user_id_fkey");
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("service_type_pkey");

            entity.ToTable("service_type");

            entity.HasIndex(e => e.ServiceName, "idx_service_catalog_name");

            entity.HasIndex(e => e.IsActive, "idx_service_type_active");

            entity.HasIndex(e => e.Code, "idx_service_type_code");

            entity.HasIndex(e => e.Code, "service_type_code_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(150)
                .HasColumnName("service_name");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("states_pkey");

            entity.ToTable("states", tb => tb.HasComment("States or territories for address management"));

            entity.HasIndex(e => e.Abbreviation, "states_abbreviation_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Abbreviation)
                .HasMaxLength(3)
                .HasColumnName("abbreviation");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TechnicalContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("technical_contact_pkey");

            entity.ToTable("technical_contact");

            entity.HasIndex(e => e.ContactId, "technical_contacts_contact_id_idx");

            entity.HasIndex(e => e.CreatedByUserId, "technical_contacts_created_by_user_id_idx");

            entity.HasIndex(e => e.JobId, "technical_contacts_job_id_idx");

            entity.HasIndex(e => e.ModifiedByUserId, "technical_contacts_modified_by_user_id_idx");

            entity.HasIndex(e => e.TypeId, "technical_contacts_type_id_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.TypeId).HasColumnName("type_id");

            entity.HasOne(d => d.Contact).WithMany(p => p.TechnicalContacts)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("technical_contact_contact_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.TechnicalContactCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("technical_contact_created_by_user_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.TechnicalContacts)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("technical_contact_job_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.TechnicalContactModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("technical_contact_modified_by_user_id_fkey");

            entity.HasOne(d => d.Type).WithMany(p => p.TechnicalContacts)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("technical_contact_type_id_fkey");
        });

        modelBuilder.Entity<TechnicalContactType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("technical_contact_type_pkey");

            entity.ToTable("technical_contact_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TimesheetEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("timesheet_entry_pkey");

            entity.ToTable("timesheet_entry", tb => tb.HasComment("Time tracking entries for jobs and their associated note content."));

            entity.HasIndex(e => e.CreatedByUserId, "idx_timesheet_created_by");

            entity.HasIndex(e => e.DateFrom, "idx_timesheet_date_from");

            entity.HasIndex(e => e.DateTo, "idx_timesheet_date_to");

            entity.HasIndex(e => e.JobId, "idx_timesheet_job_id");

            entity.HasIndex(e => e.TypeId, "idx_timesheet_type_id");

            entity.HasIndex(e => e.UserId, "idx_timesheet_user_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DateFrom).HasColumnName("date_from");
            entity.Property(e => e.DateTo)
                .HasComment("NULL indicates ongoing/in-progress entry")
                .HasColumnName("date_to");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.TypeId).HasColumnName("type_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.TimesheetEntryCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("timesheet_entry_created_by_user_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.TimesheetEntries)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("timesheet_entry_job_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.TimesheetEntryModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("timesheet_entry_modified_by_user_id_fkey");

            entity.HasOne(d => d.Type).WithMany(p => p.TimesheetEntries)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("timesheet_entry_type_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.TimesheetEntryUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("timesheet_entry_user_id_fkey");
        });

        modelBuilder.Entity<TimesheetEntryType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("timesheet_entry_type_pkey");

            entity.ToTable("timesheet_entry_type");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<XeroAccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("xero_access_pkey");

            entity.ToTable("xero_access");

            entity.HasIndex(e => e.Expires, "idx_xero_access_expires");

            entity.HasIndex(e => e.Token, "idx_xero_access_token");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.DateRefreshed)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("date_refreshed");
            entity.Property(e => e.Expires).HasColumnName("expires");
            entity.Property(e => e.Token)
                .HasMaxLength(5000)
                .HasColumnName("token");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
