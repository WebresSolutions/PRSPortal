using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Application users with authentication and profile information
/// </summary>
public partial class AppUser
{
    public int Id { get; set; }

    public string IdentityId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// NULL = active user, TIMESTAMPTZ = deactivated user
    /// </summary>
    public DateTime? DeactivatedAt { get; set; }

    public int LegacyUserId { get; set; }

    public virtual ICollection<Address> AddressCreatedByUsers { get; set; } = new List<Address>();

    public virtual ICollection<Address> AddressModifiedByUsers { get; set; } = new List<Address>();

    public virtual ICollection<AppFile> AppFileCreatedByUsers { get; set; } = new List<AppFile>();

    public virtual ICollection<AppFile> AppFileModifiedByUsers { get; set; } = new List<AppFile>();

    public virtual ICollection<Contact> ContactCreatedByUsers { get; set; } = new List<Contact>();

    public virtual ICollection<Contact> ContactModifiedByUsers { get; set; } = new List<Contact>();

    public virtual ICollection<CouncilContact> CouncilContactCreatedByUsers { get; set; } = new List<CouncilContact>();

    public virtual ICollection<CouncilContact> CouncilContactModifiedByUsers { get; set; } = new List<CouncilContact>();

    public virtual ICollection<Council> CouncilCreatedByUsers { get; set; } = new List<Council>();

    public virtual ICollection<Council> CouncilModifiedByUsers { get; set; } = new List<Council>();

    public virtual ICollection<Dashboard> Dashboards { get; set; } = new List<Dashboard>();

    public virtual ICollection<AppUser> InverseModifiedByUser { get; set; } = new List<AppUser>();

    public virtual ICollection<Job> JobCreatedByUsers { get; set; } = new List<Job>();

    public virtual ICollection<JobFile> JobFiles { get; set; } = new List<JobFile>();

    public virtual ICollection<Job> JobModifiedByUsers { get; set; } = new List<Job>();

    public virtual ICollection<JobNote> JobNoteAssignedUsers { get; set; } = new List<JobNote>();

    public virtual ICollection<JobNote> JobNoteCreatedByUsers { get; set; } = new List<JobNote>();

    public virtual ICollection<JobNote> JobNoteModifiedByUsers { get; set; } = new List<JobNote>();

    public virtual ICollection<JobQuote> JobQuotes { get; set; } = new List<JobQuote>();

    public virtual ICollection<JobTask> JobTaskCreatedByUsers { get; set; } = new List<JobTask>();

    public virtual ICollection<JobTask> JobTaskModifiedByUsers { get; set; } = new List<JobTask>();

    public virtual ICollection<JobTaskType> JobTaskTypeCreatedByUsers { get; set; } = new List<JobTaskType>();

    public virtual ICollection<JobTaskType> JobTaskTypeModifiedByUsers { get; set; } = new List<JobTaskType>();

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual ICollection<Quote> QuoteCreatedByUsers { get; set; } = new List<Quote>();

    public virtual ICollection<Quote> QuoteModifiedByUsers { get; set; } = new List<Quote>();

    public virtual ICollection<QuoteNote> QuoteNoteCreatedByUsers { get; set; } = new List<QuoteNote>();

    public virtual ICollection<QuoteNote> QuoteNoteModifiedByUsers { get; set; } = new List<QuoteNote>();

    public virtual ICollection<Schedule> ScheduleCreatedByUsers { get; set; } = new List<Schedule>();

    public virtual ICollection<Schedule> ScheduleModifiedByUsers { get; set; } = new List<Schedule>();

    public virtual ICollection<ScheduleTrack> ScheduleTrackCreatedByUsers { get; set; } = new List<ScheduleTrack>();

    public virtual ICollection<ScheduleTrack> ScheduleTrackModifiedByUsers { get; set; } = new List<ScheduleTrack>();

    public virtual ICollection<ScheduleUser> ScheduleUserCreatedByUsers { get; set; } = new List<ScheduleUser>();

    public virtual ICollection<ScheduleUser> ScheduleUserUsers { get; set; } = new List<ScheduleUser>();

    public virtual ICollection<TimesheetEntry> TimesheetEntryCreatedByUsers { get; set; } = new List<TimesheetEntry>();

    public virtual ICollection<TimesheetEntry> TimesheetEntryModifiedByUsers { get; set; } = new List<TimesheetEntry>();

    public virtual ICollection<TimesheetEntry> TimesheetEntryUsers { get; set; } = new List<TimesheetEntry>();

    public virtual ICollection<UserJob> UserJobCreatedByUsers { get; set; } = new List<UserJob>();

    public virtual ICollection<UserJob> UserJobModifiedByUsers { get; set; } = new List<UserJob>();

    public virtual ICollection<UserJob> UserJobUsers { get; set; } = new List<UserJob>();
}
