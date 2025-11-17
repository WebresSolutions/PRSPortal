using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Portal.Data.Models;

namespace Portal.Data;

public partial class PrsDbContext : DbContext
{
    public PrsDbContext(DbContextOptions<PrsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<AppFile> AppFiles { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Council> Councils { get; set; }

    public virtual DbSet<CouncilContact> CouncilContacts { get; set; }

    public virtual DbSet<FileType> FileTypes { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobColour> JobColours { get; set; }

    public virtual DbSet<JobFile> JobFiles { get; set; }

    public virtual DbSet<JobNote> JobNotes { get; set; }

    public virtual DbSet<JobQuote> JobQuotes { get; set; }

    public virtual DbSet<JobType> JobTypes { get; set; }

    public virtual DbSet<NoteType> NoteTypes { get; set; }

    public virtual DbSet<Quote> Quotes { get; set; }

    public virtual DbSet<QuoteNote> QuoteNotes { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<ScheduleTrack> ScheduleTracks { get; set; }

    public virtual DbSet<ScheduleUser> ScheduleUsers { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<TimesheetEntry> TimesheetEntries { get; set; }

    public virtual DbSet<UserJob> UserJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("address_pkey");

            entity.ToTable("address", tb => tb.HasComment("Physical addresses for contacts and jobs"));

            entity.HasIndex(e => e.CreatedByUserId, "idx_address_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_address_deleted_at");

            entity.HasIndex(e => e.StateId, "idx_address_state_id");

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
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.PostCode)
                .HasMaxLength(10)
                .HasColumnName("post_code");
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
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft delete TIMESTAMPTZ - NULL means active")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ExternalId)
                .HasMaxLength(255)
                .HasComment("Reference to external storage system (S3, etc)")
                .HasColumnName("external_id");
            entity.Property(e => e.FileHash)
                .HasMaxLength(64)
                .HasComment("SHA-256 hash for duplicate detection")
                .HasColumnName("file_hash");
            entity.Property(e => e.FileTypeId).HasColumnName("file_type_id");
            entity.Property(e => e.Filename)
                .HasMaxLength(255)
                .HasColumnName("filename");
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
                .OnDelete(DeleteBehavior.ClientSetNull)
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
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
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

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.InverseModifiedByUser)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("app_user_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contact_pkey");

            entity.ToTable("contact", tb => tb.HasComment("Client or vendor contact information"));

            entity.HasIndex(e => e.AddressId, "idx_contact_address_id");

            entity.HasIndex(e => e.CreatedByUserId, "idx_contact_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_contact_deleted_at");

            entity.HasIndex(e => e.Email, "idx_contact_email");

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
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");

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

        modelBuilder.Entity<FileType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("file_type_pkey");

            entity.ToTable("file_type", tb => tb.HasComment("File type and metadata"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
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

            entity.HasIndex(e => e.JobTypeId, "idx_job_type_id");

            entity.HasIndex(e => e.InvoiceNumber, "job_invoice_number_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.ConstructionNumber).HasColumnName("construction_number");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.CouncilId).HasColumnName("council_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DateSent).HasColumnName("date_sent");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft delete TIMESTAMPTZ - NULL means active")
                .HasColumnName("deleted_at");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(255)
                .HasColumnName("invoice_number");
            entity.Property(e => e.JobColourId).HasColumnName("job_colour_id");
            entity.Property(e => e.JobTypeId).HasColumnName("job_type_id");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.PaymentReceived).HasColumnName("payment_received");
            entity.Property(e => e.SurveyNumber).HasColumnName("survey_number");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(10, 2)
                .HasComment("Total job price - consider calculating from timesheet entries")
                .HasColumnName("total_price");

            entity.HasOne(d => d.Address).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("job_address_id_fkey");

            entity.HasOne(d => d.Contact).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.ContactId)
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

            entity.HasOne(d => d.JobType).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.JobTypeId)
                .HasConstraintName("job_job_type_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.JobModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("job_modified_by_user_id_fkey");
        });

        modelBuilder.Entity<JobColour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_colour_pkey");

            entity.ToTable("job_colour", tb => tb.HasComment("Colour of job"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
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

            entity.ToTable("job_note", tb => tb.HasComment("Notes specifically attached to a job (One-to-Many: Job to Note)."));

            entity.HasIndex(e => e.CreatedByUserId, "idx_job_note_created_by");

            entity.HasIndex(e => e.DeletedAt, "idx_job_note_deleted_at");

            entity.HasIndex(e => e.JobId, "idx_job_note_job_id");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");

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

        modelBuilder.Entity<JobType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_type_pkey");

            entity.ToTable("job_type", tb => tb.HasComment("Type of job"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Abbreviation)
                .HasMaxLength(10)
                .HasColumnName("abbreviation");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<NoteType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("note_type_pkey");

            entity.ToTable("note_type", tb => tb.HasComment("Type of note"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quote_pkey");

            entity.ToTable("quote", tb => tb.HasComment("A quote for a job"));

            entity.HasIndex(e => e.AddressId, "idx_quote_address_id");

            entity.HasIndex(e => e.CreatedByUserId, "idx_quote_created_by");

            entity.HasIndex(e => e.ModifiedByUserId, "idx_quote_modified_by");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");

            entity.HasOne(d => d.Address).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("quote_address_id_fkey");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.QuoteCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quote_created_by_user_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.QuoteModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("quote_modified_by_user_id_fkey");
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

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schedule_pkey");

            entity.ToTable("schedule", tb => tb.HasComment("User schedules for work hours."));

            entity.HasIndex(e => e.CreatedByUserId, "idx_schedule_created_by");

            entity.HasIndex(e => e.EndTime, "idx_schedule_end_time");

            entity.HasIndex(e => e.JobId, "idx_schedule_job_id");

            entity.HasIndex(e => e.StartTime, "idx_schedule_start_time");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.EndTime)
                .HasComment("End time of the schedule")
                .HasColumnName("end_time");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");
            entity.Property(e => e.Notes).HasColumnName("notes");
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
            entity.HasKey(e => e.Id).HasName("schedule_users_pkey");

            entity.ToTable("schedule_users");

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
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ScheduleTrackId).HasColumnName("schedule_track_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ScheduleUserCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_users_created_by_user_id_fkey");

            entity.HasOne(d => d.ScheduleTrack).WithMany(p => p.ScheduleUsers)
                .HasForeignKey(d => d.ScheduleTrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_users_schedule_track_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ScheduleUserUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedule_users_user_id_fkey");
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

        modelBuilder.Entity<TimesheetEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("timesheet_entry_pkey");

            entity.ToTable("timesheet_entry", tb => tb.HasComment("Time tracking entries for jobs and their associated note content."));

            entity.HasIndex(e => e.CreatedByUserId, "idx_timesheet_created_by");

            entity.HasIndex(e => e.DateFrom, "idx_timesheet_date_from");

            entity.HasIndex(e => e.DateTo, "idx_timesheet_date_to");

            entity.HasIndex(e => e.JobId, "idx_timesheet_job_id");

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

            entity.HasOne(d => d.User).WithMany(p => p.TimesheetEntryUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("timesheet_entry_user_id_fkey");
        });

        modelBuilder.Entity<UserJob>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.JobId }).HasName("user_job_pkey");

            entity.ToTable("user_job", tb => tb.HasComment("Many-to-many relationship between users and jobs"));

            entity.HasIndex(e => e.DeletedAt, "idx_user_job_deleted_at");

            entity.HasIndex(e => e.JobId, "idx_user_job_job_id");

            entity.HasIndex(e => e.UserId, "idx_user_job_user_id");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_on");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft delete TIMESTAMPTZ - NULL means active assignment")
                .HasColumnName("deleted_at");
            entity.Property(e => e.LegacyId).HasColumnName("legacy_id");
            entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");
            entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.UserJobCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_job_created_by_user_id_fkey");

            entity.HasOne(d => d.Job).WithMany(p => p.UserJobs)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_job_job_id_fkey");

            entity.HasOne(d => d.ModifiedByUser).WithMany(p => p.UserJobModifiedByUsers)
                .HasForeignKey(d => d.ModifiedByUserId)
                .HasConstraintName("user_job_modified_by_user_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserJobUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_job_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
