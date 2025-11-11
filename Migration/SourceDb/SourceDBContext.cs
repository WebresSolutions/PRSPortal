using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Migration.SourceDb;

public partial class SourceDBContext : DbContext
{
    public SourceDBContext()
    {
    }

    public SourceDBContext(DbContextOptions<SourceDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Checklist> Checklists { get; set; }

    public virtual DbSet<ChecklistTemplate> ChecklistTemplates { get; set; }

    public virtual DbSet<ChecklistTemplateItem> ChecklistTemplateItems { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<ContactAlert> ContactAlerts { get; set; }

    public virtual DbSet<Council> Councils { get; set; }

    public virtual DbSet<CouncilContact> CouncilContacts { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobAlert> JobAlerts { get; set; }

    public virtual DbSet<JobChecklist> JobChecklists { get; set; }

    public virtual DbSet<JobQuote> JobQuotes { get; set; }

    public virtual DbSet<JobsUser> JobsUsers { get; set; }

    public virtual DbSet<Leaf> Leaves { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<Postcode> Postcodes { get; set; }

    public virtual DbSet<Proposal> Proposals { get; set; }

    public virtual DbSet<Quote> Quotes { get; set; }

    public virtual DbSet<QuoteNote> QuoteNotes { get; set; }

    public virtual DbSet<ReferralAuthority> ReferralAuthorities { get; set; }

    public virtual DbSet<ReferralAuthorityContact> ReferralAuthorityContacts { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<ScheduleGroup> ScheduleGroups { get; set; }

    public virtual DbSet<ScheduleTrack> ScheduleTracks { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskType> TaskTypes { get; set; }

    public virtual DbSet<TechnicalContact> TechnicalContacts { get; set; }

    public virtual DbSet<TechnicalRole> TechnicalRoles { get; set; }

    public virtual DbSet<Timecard> Timecards { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=prs_database;user id=adminuser;password=j4clas&P+e@4lv@zwvtB", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.4.6-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Checklist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("checklists")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Date1)
                .HasColumnType("datetime")
                .HasColumnName("date1");
            entity.Property(e => e.Date2)
                .HasColumnType("datetime")
                .HasColumnName("date2");
            entity.Property(e => e.Date3)
                .HasColumnType("datetime")
                .HasColumnName("date3");
            entity.Property(e => e.Date4)
                .HasColumnType("datetime")
                .HasColumnName("date4");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Fields)
                .HasMaxLength(2000)
                .HasColumnName("fields");
            entity.Property(e => e.Headers)
                .HasMaxLength(2000)
                .HasColumnName("headers");
            entity.Property(e => e.InvoicedDate)
                .HasColumnType("datetime")
                .HasColumnName("invoiced_date");
            entity.Property(e => e.Memo1)
                .HasColumnType("text")
                .HasColumnName("memo1");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.OtherId1).HasColumnName("other_id1");
            entity.Property(e => e.Searchable).HasColumnName("searchable");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.Text1)
                .HasMaxLength(200)
                .HasColumnName("text1");
            entity.Property(e => e.Text2)
                .HasMaxLength(200)
                .HasColumnName("text2");
            entity.Property(e => e.Text3)
                .HasMaxLength(200)
                .HasColumnName("text3");
            entity.Property(e => e.Text4)
                .HasMaxLength(200)
                .HasColumnName("text4");
        });

        modelBuilder.Entity<ChecklistTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("checklist_templates")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ChecklistTemplateItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("checklist_template_items")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChecklistTemplateId).HasColumnName("checklist_template_id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Date1)
                .HasColumnType("datetime")
                .HasColumnName("date1");
            entity.Property(e => e.Date2)
                .HasColumnType("datetime")
                .HasColumnName("date2");
            entity.Property(e => e.Date3)
                .HasColumnType("datetime")
                .HasColumnName("date3");
            entity.Property(e => e.Date4)
                .HasColumnType("datetime")
                .HasColumnName("date4");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Fields)
                .HasMaxLength(2000)
                .HasColumnName("fields");
            entity.Property(e => e.Headers)
                .HasMaxLength(2000)
                .HasColumnName("headers");
            entity.Property(e => e.InvoicedDate)
                .HasColumnType("datetime")
                .HasColumnName("invoiced_date");
            entity.Property(e => e.Memo1)
                .HasColumnType("text")
                .HasColumnName("memo1");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.OtherId1).HasColumnName("other_id1");
            entity.Property(e => e.Searchable).HasColumnName("searchable");
            entity.Property(e => e.Text1)
                .HasMaxLength(200)
                .HasColumnName("text1");
            entity.Property(e => e.Text2)
                .HasMaxLength(200)
                .HasColumnName("text2");
            entity.Property(e => e.Text3)
                .HasMaxLength(200)
                .HasColumnName("text3");
            entity.Property(e => e.Text4)
                .HasMaxLength(200)
                .HasColumnName("text4");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("contacts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.Fax)
                .HasMaxLength(200)
                .HasColumnName("fax");
            entity.Property(e => e.Firstname)
                .HasMaxLength(200)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(200)
                .HasColumnName("lastname");
            entity.Property(e => e.Mobile)
                .HasMaxLength(200)
                .HasColumnName("mobile");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Phone)
                .HasMaxLength(200)
                .HasColumnName("phone");
            entity.Property(e => e.Postcode)
                .HasMaxLength(200)
                .HasColumnName("postcode");
            entity.Property(e => e.State)
                .HasMaxLength(200)
                .HasColumnName("state");
            entity.Property(e => e.Suburb)
                .HasMaxLength(200)
                .HasColumnName("suburb");
            entity.Property(e => e.Title)
                .HasMaxLength(20)
                .HasColumnName("title");
        });

        modelBuilder.Entity<ContactAlert>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("contact_alerts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AlertType)
                .HasMaxLength(20)
                .HasColumnName("alert_type");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Text)
                .HasMaxLength(500)
                .HasColumnName("text");
        });

        modelBuilder.Entity<Council>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("councils")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.Fax)
                .HasMaxLength(200)
                .HasColumnName("fax");
            entity.Property(e => e.Mobile)
                .HasMaxLength(200)
                .HasColumnName("mobile");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(200)
                .HasColumnName("phone");
            entity.Property(e => e.Postcode)
                .HasMaxLength(200)
                .HasColumnName("postcode");
            entity.Property(e => e.State)
                .HasMaxLength(3)
                .HasColumnName("state");
            entity.Property(e => e.Suburb)
                .HasMaxLength(200)
                .HasColumnName("suburb");
            entity.Property(e => e.Website)
                .HasMaxLength(200)
                .HasColumnName("website");
        });

        modelBuilder.Entity<CouncilContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("council_contacts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.CouncilId).HasColumnName("council_id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.Fax)
                .HasMaxLength(200)
                .HasColumnName("fax");
            entity.Property(e => e.Firstname)
                .HasMaxLength(200)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(200)
                .HasColumnName("lastname");
            entity.Property(e => e.Mobile)
                .HasMaxLength(200)
                .HasColumnName("mobile");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Phone)
                .HasMaxLength(200)
                .HasColumnName("phone");
            entity.Property(e => e.Postcode)
                .HasMaxLength(200)
                .HasColumnName("postcode");
            entity.Property(e => e.State)
                .HasMaxLength(200)
                .HasColumnName("state");
            entity.Property(e => e.Suburb)
                .HasMaxLength(200)
                .HasColumnName("suburb");
            entity.Property(e => e.Title)
                .HasMaxLength(20)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("groups")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Lft).HasColumnName("lft");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Rght).HasColumnName("rght");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("invoices")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Invoice1)
                .HasColumnType("text")
                .HasColumnName("invoice");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.PaymentReceivedDate)
                .HasColumnType("datetime")
                .HasColumnName("payment_received_date");
            entity.Property(e => e.PurchaseOrderNumber)
                .HasMaxLength(200)
                .HasColumnName("purchase_order_number");
            entity.Property(e => e.ReadyDate)
                .HasColumnType("datetime")
                .HasColumnName("ready_date");
            entity.Property(e => e.SentDate)
                .HasColumnType("datetime")
                .HasColumnName("sent_date");
            entity.Property(e => e.SetoutInvoice).HasColumnName("setout_invoice");
            entity.Property(e => e.SetoutJobNumber).HasColumnName("setout_job_number");
            entity.Property(e => e.Suffix)
                .HasMaxLength(20)
                .HasColumnName("suffix");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("jobs")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.CadastralJobNumber).HasColumnName("cadastral_job_number");
            entity.Property(e => e.Colour)
                .HasMaxLength(10)
                .HasColumnName("colour");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.CouncilId).HasColumnName("council_id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Details)
                .HasColumnType("text")
                .HasColumnName("details");
            entity.Property(e => e.EndCustomer)
                .HasMaxLength(200)
                .HasColumnName("end_customer");
            entity.Property(e => e.LastPlanReference)
                .HasMaxLength(200)
                .HasColumnName("last_plan_reference");
            entity.Property(e => e.MapReference)
                .HasMaxLength(200)
                .HasColumnName("map_reference");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Postcode)
                .HasMaxLength(200)
                .HasColumnName("postcode");
            entity.Property(e => e.SetoutJobNumber).HasColumnName("setout_job_number");
            entity.Property(e => e.State)
                .HasMaxLength(200)
                .HasColumnName("state");
            entity.Property(e => e.Suburb)
                .HasMaxLength(200)
                .HasColumnName("suburb");
        });

        modelBuilder.Entity<JobAlert>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("job_alerts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AlertType)
                .HasMaxLength(20)
                .HasColumnName("alert_type");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Text)
                .HasMaxLength(500)
                .HasColumnName("text");
        });

        modelBuilder.Entity<JobChecklist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("job_checklists")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AfChecked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("af_checked");
            entity.Property(e => e.AfCheckedDate)
                .HasColumnType("datetime")
                .HasColumnName("af_checked_date");
            entity.Property(e => e.AfComplete)
                .HasDefaultValueSql("'0'")
                .HasColumnName("af_complete");
            entity.Property(e => e.AfCompleteDate)
                .HasColumnType("datetime")
                .HasColumnName("af_complete_date");
            entity.Property(e => e.AfSent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("af_sent");
            entity.Property(e => e.AfSentDate)
                .HasColumnType("datetime")
                .HasColumnName("af_sent_date");
            entity.Property(e => e.BdChecked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("bd_checked");
            entity.Property(e => e.BdCheckedDate)
                .HasColumnType("datetime")
                .HasColumnName("bd_checked_date");
            entity.Property(e => e.BdComplete)
                .HasDefaultValueSql("'0'")
                .HasColumnName("bd_complete");
            entity.Property(e => e.BdCompleteDate)
                .HasColumnType("datetime")
                .HasColumnName("bd_complete_date");
            entity.Property(e => e.BdSent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("bd_sent");
            entity.Property(e => e.BdSentDate)
                .HasColumnType("datetime")
                .HasColumnName("bd_sent_date");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.EsChecked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("es_checked");
            entity.Property(e => e.EsCheckedDate)
                .HasColumnType("datetime")
                .HasColumnName("es_checked_date");
            entity.Property(e => e.EsComplete)
                .HasDefaultValueSql("'0'")
                .HasColumnName("es_complete");
            entity.Property(e => e.EsCompleteDate)
                .HasColumnType("datetime")
                .HasColumnName("es_complete_date");
            entity.Property(e => e.EsSent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("es_sent");
            entity.Property(e => e.EsSentDate)
                .HasColumnType("datetime")
                .HasColumnName("es_sent_date");
            entity.Property(e => e.FpChecked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("fp_checked");
            entity.Property(e => e.FpCheckedDate)
                .HasColumnType("datetime")
                .HasColumnName("fp_checked_date");
            entity.Property(e => e.FpComplete)
                .HasDefaultValueSql("'0'")
                .HasColumnName("fp_complete");
            entity.Property(e => e.FpCompleteDate)
                .HasColumnType("datetime")
                .HasColumnName("fp_complete_date");
            entity.Property(e => e.FpSent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("fp_sent");
            entity.Property(e => e.FpSentDate)
                .HasColumnType("datetime")
                .HasColumnName("fp_sent_date");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.MgaChecked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("mga_checked");
            entity.Property(e => e.MgaCheckedDate)
                .HasColumnType("datetime")
                .HasColumnName("mga_checked_date");
            entity.Property(e => e.MgaComplete)
                .HasDefaultValueSql("'0'")
                .HasColumnName("mga_complete");
            entity.Property(e => e.MgaCompleteDate)
                .HasColumnType("datetime")
                .HasColumnName("mga_complete_date");
            entity.Property(e => e.MgaSent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("mga_sent");
            entity.Property(e => e.MgaSentDate)
                .HasColumnType("datetime")
                .HasColumnName("mga_sent_date");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.PfChecked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("pf_checked");
            entity.Property(e => e.PfCheckedDate)
                .HasColumnType("datetime")
                .HasColumnName("pf_checked_date");
            entity.Property(e => e.PfComplete)
                .HasDefaultValueSql("'0'")
                .HasColumnName("pf_complete");
            entity.Property(e => e.PfCompleteDate)
                .HasColumnType("datetime")
                .HasColumnName("pf_complete_date");
            entity.Property(e => e.PfSent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("pf_sent");
            entity.Property(e => e.PfSentDate)
                .HasColumnType("datetime")
                .HasColumnName("pf_sent_date");
            entity.Property(e => e.ReChecked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("re_checked");
            entity.Property(e => e.ReCheckedDate)
                .HasColumnType("datetime")
                .HasColumnName("re_checked_date");
            entity.Property(e => e.ReComplete)
                .HasDefaultValueSql("'0'")
                .HasColumnName("re_complete");
            entity.Property(e => e.ReCompleteDate)
                .HasColumnType("datetime")
                .HasColumnName("re_complete_date");
            entity.Property(e => e.ReSent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("re_sent");
            entity.Property(e => e.ReSentDate)
                .HasColumnType("datetime")
                .HasColumnName("re_sent_date");
            entity.Property(e => e.SdChecked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("sd_checked");
            entity.Property(e => e.SdCheckedDate)
                .HasColumnType("datetime")
                .HasColumnName("sd_checked_date");
            entity.Property(e => e.SdComplete)
                .HasDefaultValueSql("'0'")
                .HasColumnName("sd_complete");
            entity.Property(e => e.SdCompleteDate)
                .HasColumnType("datetime")
                .HasColumnName("sd_complete_date");
            entity.Property(e => e.SdSent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("sd_sent");
            entity.Property(e => e.SdSentDate)
                .HasColumnType("datetime")
                .HasColumnName("sd_sent_date");
            entity.Property(e => e.UlChecked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("ul_checked");
            entity.Property(e => e.UlCheckedDate)
                .HasColumnType("datetime")
                .HasColumnName("ul_checked_date");
            entity.Property(e => e.UlComplete)
                .HasDefaultValueSql("'0'")
                .HasColumnName("ul_complete");
            entity.Property(e => e.UlCompleteDate)
                .HasColumnType("datetime")
                .HasColumnName("ul_complete_date");
            entity.Property(e => e.UlSent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("ul_sent");
            entity.Property(e => e.UlSentDate)
                .HasColumnType("datetime")
                .HasColumnName("ul_sent_date");
        });

        modelBuilder.Entity<JobQuote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("job_quotes")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_bin");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");
        });

        modelBuilder.Entity<JobsUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("jobs_users")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Leaf>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("leaves")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_bin");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.LeaveEndDate).HasColumnName("leave_end_date");
            entity.Property(e => e.LeaveEndTimeHour).HasColumnName("leave_end_time_hour");
            entity.Property(e => e.LeaveEndTimeMinute).HasColumnName("leave_end_time_minute");
            entity.Property(e => e.LeaveStartDate).HasColumnName("leave_start_date");
            entity.Property(e => e.LeaveStartTimeHour).HasColumnName("leave_start_time_hour");
            entity.Property(e => e.LeaveStartTimeMinute).HasColumnName("leave_start_time_minute");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("notes")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionRequired).HasColumnName("action_required");
            entity.Property(e => e.ActionTaken).HasColumnName("action_taken");
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Note1)
                .HasColumnType("text")
                .HasColumnName("note");
        });

        modelBuilder.Entity<Postcode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("postcodes")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BspName)
                .HasMaxLength(50)
                .HasColumnName("bsp_name");
            entity.Property(e => e.BspNumber)
                .HasMaxLength(5)
                .HasColumnName("bsp_number");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");
            entity.Property(e => e.Comments)
                .HasMaxLength(50)
                .HasColumnName("comments");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.DeliveryOffice)
                .HasMaxLength(50)
                .HasColumnName("delivery_office");
            entity.Property(e => e.Locality)
                .HasMaxLength(50)
                .HasColumnName("locality");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.ParcelZone)
                .HasMaxLength(5)
                .HasColumnName("parcel_zone");
            entity.Property(e => e.Postcode1)
                .HasMaxLength(5)
                .HasColumnName("postcode");
            entity.Property(e => e.PresortIndicator)
                .HasMaxLength(5)
                .HasColumnName("presort_indicator");
            entity.Property(e => e.State)
                .HasMaxLength(3)
                .HasColumnName("state");
        });

        modelBuilder.Entity<Proposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("proposals")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Invoice)
                .HasColumnType("text")
                .HasColumnName("invoice");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");
            entity.Property(e => e.ReadyDate)
                .HasColumnType("datetime")
                .HasColumnName("ready_date");
            entity.Property(e => e.SentDate)
                .HasColumnType("datetime")
                .HasColumnName("sent_date");
            entity.Property(e => e.Suffix)
                .HasMaxLength(20)
                .HasColumnName("suffix");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("quotes")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(200)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.ContactOther)
                .HasMaxLength(200)
                .HasColumnName("contact_other");
            entity.Property(e => e.ContactTechnical)
                .HasMaxLength(200)
                .HasColumnName("contact_technical");
            entity.Property(e => e.CouncilId).HasColumnName("council_id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.CtSiNeeded)
                .HasMaxLength(200)
                .HasColumnName("ct_si_needed");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Details)
                .HasColumnType("text")
                .HasColumnName("details");
            entity.Property(e => e.MapReference)
                .HasMaxLength(200)
                .HasColumnName("map_reference");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.OtherDetails)
                .HasColumnType("text")
                .HasColumnName("other_details");
            entity.Property(e => e.Postcode)
                .HasMaxLength(200)
                .HasColumnName("postcode");
            entity.Property(e => e.State)
                .HasMaxLength(200)
                .HasColumnName("state");
            entity.Property(e => e.Suburb)
                .HasMaxLength(200)
                .HasColumnName("suburb");
        });

        modelBuilder.Entity<QuoteNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("quote_notes")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_bin");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionRequired).HasColumnName("action_required");
            entity.Property(e => e.ActionTaken).HasColumnName("action_taken");
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Note)
                .HasColumnType("text")
                .HasColumnName("note");
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");
        });

        modelBuilder.Entity<ReferralAuthority>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("referral_authorities")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.Fax)
                .HasMaxLength(200)
                .HasColumnName("fax");
            entity.Property(e => e.Mobile)
                .HasMaxLength(200)
                .HasColumnName("mobile");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(200)
                .HasColumnName("phone");
            entity.Property(e => e.Postcode)
                .HasMaxLength(200)
                .HasColumnName("postcode");
            entity.Property(e => e.State)
                .HasMaxLength(3)
                .HasColumnName("state");
            entity.Property(e => e.Suburb)
                .HasMaxLength(200)
                .HasColumnName("suburb");
            entity.Property(e => e.Website)
                .HasMaxLength(200)
                .HasColumnName("website");
        });

        modelBuilder.Entity<ReferralAuthorityContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("referral_authority_contacts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.Fax)
                .HasMaxLength(200)
                .HasColumnName("fax");
            entity.Property(e => e.Firstname)
                .HasMaxLength(200)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(200)
                .HasColumnName("lastname");
            entity.Property(e => e.Mobile)
                .HasMaxLength(200)
                .HasColumnName("mobile");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Phone)
                .HasMaxLength(200)
                .HasColumnName("phone");
            entity.Property(e => e.Postcode)
                .HasMaxLength(200)
                .HasColumnName("postcode");
            entity.Property(e => e.ReferralAuthorityId).HasColumnName("referral_authority_id");
            entity.Property(e => e.State)
                .HasMaxLength(200)
                .HasColumnName("state");
            entity.Property(e => e.Suburb)
                .HasMaxLength(200)
                .HasColumnName("suburb");
            entity.Property(e => e.Title)
                .HasMaxLength(20)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("schedules")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Colour)
                .HasMaxLength(10)
                .HasColumnName("colour");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");
            entity.Property(e => e.ScheduleTrackId)
                .HasDefaultValueSql("'0'")
                .HasColumnName("schedule_track_id");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.TypeId)
                .HasDefaultValueSql("'0'")
                .HasColumnName("type_id");
        });

        modelBuilder.Entity<ScheduleGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("schedule_groups")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ScheduleTrack>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("schedule_tracks")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssigneeUserId1).HasColumnName("assignee_user_id_1");
            entity.Property(e => e.AssigneeUserId2).HasColumnName("assignee_user_id_2");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Ordinal).HasColumnName("ordinal");
            entity.Property(e => e.ScheduleGroupId).HasColumnName("schedule_group_id");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tasks")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActiveDate)
                .HasColumnType("datetime")
                .HasColumnName("active_date");
            entity.Property(e => e.ChecklistTemplateId).HasColumnName("checklist_template_id");
            entity.Property(e => e.CompletedDate)
                .HasColumnType("datetime")
                .HasColumnName("completed_date");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Details)
                .HasColumnType("text")
                .HasColumnName("details");
            entity.Property(e => e.InvoiceNotRequired).HasColumnName("invoice_not_required");
            entity.Property(e => e.InvoicedDate)
                .HasColumnType("datetime")
                .HasColumnName("invoiced_date");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.PurchaseOrderNumber)
                .HasMaxLength(200)
                .HasColumnName("purchase_order_number");
            entity.Property(e => e.QuotedDate)
                .HasColumnType("datetime")
                .HasColumnName("quoted_date");
            entity.Property(e => e.QuotedPrice).HasColumnName("quoted_price");
            entity.Property(e => e.SetoutTask).HasColumnName("setout_task");
            entity.Property(e => e.TaskType)
                .HasMaxLength(100)
                .HasColumnName("task_type");
        });

        modelBuilder.Entity<TaskType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("task_types")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TechnicalContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("technical_contacts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Role)
                .HasMaxLength(100)
                .HasColumnName("role");
        });

        modelBuilder.Entity<TechnicalRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("technical_roles")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Timecard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("timecards")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Details)
                .HasColumnType("text")
                .HasColumnName("details");
            entity.Property(e => e.Hours).HasColumnName("hours");
            entity.Property(e => e.Minutes).HasColumnName("minutes");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("users")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("active");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CreatedUser).HasColumnName("created_user");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DeletedDate)
                .HasColumnType("datetime")
                .HasColumnName("deleted_date");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .HasColumnName("firstname");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .HasColumnName("lastname");
            entity.Property(e => e.Modified)
                .HasColumnType("datetime")
                .HasColumnName("modified");
            entity.Property(e => e.ModifiedUser).HasColumnName("modified_user");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.SigningName)
                .HasMaxLength(50)
                .HasColumnName("signing_name");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
            entity.Property(e => e.WorkRate).HasColumnName("work_rate");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
