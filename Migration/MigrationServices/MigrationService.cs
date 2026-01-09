using Microsoft.EntityFrameworkCore;
using Migration.Display;
using Migration.SourceDb;
using System.Collections.Frozen;

namespace Migration.MigrationServices;

internal class MigrationService(
    PrsDbContext _destinationContext,
    SourceDBContext _sourceDBContext,
    FrozenDictionary<int, int> users) : BaseMigrationClass(_destinationContext, _sourceDBContext)
{
    /// <summary>
    /// Cache of councils to avoid multiple DB hits. Key is the legacy ID.
    /// </summary>
    private FrozenDictionary<int, Models.Council>? _councilsCache;
    /// <summary>
    /// Cache of contacts to avoid multiple DB hits. Key is the legacy ID.
    /// </summary>
    private FrozenDictionary<int, Models.Contact>? _contactsCache;
    /// <summary>
    /// Cache of jobs to avoid multiple DB hits. Key is the legacy ID.
    /// </summary>
    private FrozenDictionary<int, Models.Job>? _jobsCache;

    /// <summary>
    /// Cache of users to avoid multiple DB hits. Key is the legacy ID, value is the new user ID.
    /// </summary>
    private readonly FrozenDictionary<int, int> _Users = users;

    /// <summary>
    /// Migrates contacts from the source database to the destination database
    /// Includes address creation and parent contact relationship mapping
    /// </summary>
    /// <param name="progressCallback">Callback function for reporting migration progress</param>
    public void MigrateContacts(Action<MigrationProgress> progressCallback)
    {
        try
        {
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Contacts",
                CurrentItem = "Loading contacts from source database...",
                CurrentItemIndex = 0,
                TotalItems = 0
            });

            // Get all of the councils
            List<Models.Contact> councilsToCreate = [];
            SourceDb.Contact[] oldContacts = [.. _sourceDBContext.Contacts.AsNoTracking()];
            List<Models.Contact> contacts = [.. _destinationContext.Contacts.AsNoTracking()];

            if (contacts.Count == oldContacts.Length)
            {
                _contactsCache = contacts.ToFrozenDictionary(x => x.LegacyId ?? 0, y => y);
                return;
            }

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Contacts",
                CurrentItem = $"Found {oldContacts.Length} councils",
                CurrentItemIndex = 0,
                TotalItems = oldContacts.Length
            });

            Dictionary<int, Models.Address> addressesToAdd = [];
            List<Models.Contact> contactToAdd = [];
            int index = 0;

            foreach (SourceDb.Contact oldContact in oldContacts)
            {
                if (index % 100 == 0)
                {
                    progressCallback.Invoke(new MigrationProgress
                    {
                        CurrentStep = "Migrating Contacts",
                        CurrentItem = $"Processing contact {index + 1}/{oldContacts.Length}",
                        CurrentItemIndex = index + 1,
                        TotalItems = oldContacts.Length
                    });
                }
                Models.Contact newContact = new()
                {
                    FirstName = Helpers.TruncateString(oldContact.Firstname, 50),
                    LastName = Helpers.TruncateString(oldContact.Lastname ?? "", 50),
                    Phone = string.IsNullOrEmpty(oldContact.Phone) ? Helpers.TruncateString(oldContact.Mobile, 50) : Helpers.TruncateString(oldContact.Phone, 50),
                    Fax = Helpers.TruncateString(oldContact.Fax, 50),
                    Email = Helpers.TruncateString(oldContact.Email ?? "", 50),
                    CreatedByUserId = 95,
                    CreatedOn = Helpers.GetValidDateWithTimezone(oldContact.Created),
                    LegacyId = (int)oldContact.Id,
                    ParentContactId = oldContact.ContactId is 0 ? null : oldContact.ContactId
                };

                Models.Address address = Helpers.CreateAddress(_destinationContext, oldContact.State, oldContact.Address ?? "", oldContact.Suburb ?? "", oldContact.Postcode);
                addressesToAdd.Add((int)oldContact.Id, address);
                contactToAdd.Add(newContact);
                index++;
            }
            _destinationContext.Addresses.AddRange(addressesToAdd.Values);
            _destinationContext.SaveChanges();

            foreach (Models.Contact contact in contactToAdd)
            {
                contact.Address = addressesToAdd[contact.LegacyId ?? 0];
                contact.AddressId = addressesToAdd[contact.LegacyId ?? 0].Id;
            }

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Contacts",
                CurrentItem = "Saving contacts to destination database...",
                CurrentItemIndex = oldContacts.Length,
                TotalItems = oldContacts.Length
            });

            _destinationContext.BulkInsert(contactToAdd);
            contacts = [.. _destinationContext.Contacts];
            _contactsCache = contacts.ToFrozenDictionary(x => x.LegacyId ?? 0, y => y);

            // update parent contact ids
            foreach (Models.Contact contact in contacts)
                contact.ParentContactId = contact.ParentContactId.HasValue ? _contactsCache.GetValueOrDefault(contact.ParentContactId ?? 0)?.Id : null;

            _destinationContext.SaveChanges();
            _contactsCache = contacts.ToFrozenDictionary(x => x.LegacyId ?? 0, y => y);

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Finished Migrating Contacts",
                CurrentItem = $"Successfully migrated {_contactsCache.Count} contacts",
                CurrentItemIndex = oldContacts.Length,
                TotalItems = oldContacts.Length
            });

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException?.Message);
            throw;
        }
    }

    /// <summary>
    /// Migrates councils from the source database to the destination database
    /// Includes address creation and council contact information
    /// </summary>
    /// <param name="progressCallback">Callback function for reporting migration progress</param>
    public void MigrateCouncils(Action<MigrationProgress> progressCallback)
    {
        try
        {
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Councils",
                CurrentItem = "Loading councils from source database...",
                CurrentItemIndex = 0,
                TotalItems = 0
            });

            // Get all of the councils
            List<Models.Council> councilsToCreate = [];
            SourceDb.Council[] oldCouncils = [.. _sourceDBContext.Councils.AsNoTracking()];
            if (_destinationContext.CouncilContacts.Count() == oldCouncils.Length)
            {
                _councilsCache = _destinationContext.Councils.ToFrozenDictionary(x => x.LegacyId ?? 0, y => y);
                return;
            }
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Councils",
                CurrentItem = $"Found {oldCouncils.Length} councils",
                CurrentItemIndex = 0,
                TotalItems = oldCouncils.Length
            });

            Dictionary<int, Models.Address> addressesToAdd = [];
            List<Models.Council> councilsToAdd = [];
            int index = 0;

            foreach (SourceDb.Council oldCouncil in oldCouncils)
            {
                progressCallback.Invoke(new MigrationProgress
                {
                    CurrentStep = "Migrating Councils",
                    CurrentItem = $"Processing council {index + 1}/{oldCouncils.Length}",
                    CurrentItemIndex = index + 1,
                    TotalItems = oldCouncils.Length
                });
                Models.Council newCouncil = new()
                {
                    Name = oldCouncil.Name,
                    Phone = oldCouncil.Phone,
                    Fax = oldCouncil.Fax,
                    Email = oldCouncil.Email,
                    Website = oldCouncil.Website,
                    CreatedByUserId = 95,
                    CreatedOn = oldCouncil.Created.HasValue ? DateTime.SpecifyKind(oldCouncil.Created.Value, DateTimeKind.Utc) : DateTime.UtcNow,
                    LegacyId = (int)oldCouncil.Id
                };

                Models.Address address = Helpers.CreateAddress(_destinationContext, oldCouncil.State, oldCouncil.Address ?? "", oldCouncil.Suburb ?? "", oldCouncil.Postcode);
                addressesToAdd.Add((int)oldCouncil.Id, address);
                councilsToAdd.Add(newCouncil);
            }

            _destinationContext.Addresses.AddRange(addressesToAdd.Values);
            _destinationContext.SaveChanges();

            foreach (Models.Council council in councilsToAdd)
            {
                council.Address = addressesToAdd[council.LegacyId ?? 0];
                council.AddressId = addressesToAdd[council.LegacyId ?? 0].Id;
            }

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Councils",
                CurrentItem = "Saving councils to destination database...",
                CurrentItemIndex = oldCouncils.Length,
                TotalItems = oldCouncils.Length
            });
            _destinationContext.Councils.AddRange(councilsToAdd);
            _destinationContext.SaveChanges();
            _councilsCache = councilsToAdd.ToFrozenDictionary(x => x.LegacyId ?? 0, y => y); ;

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Finished Migrating Councils",
                CurrentItem = $"Successfully migrated {_councilsCache.Count} councils",
                CurrentItemIndex = oldCouncils.Length,
                TotalItems = oldCouncils.Length
            });

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Migrates jobs from the source database to the destination database
    /// Includes job color creation, address linking, and contact/council associations
    /// </summary>
    /// <param name="progressCallback">Callback function for reporting migration progress</param>
    public void MigrateJobs(Action<MigrationProgress> progressCallback)
    {
        try
        {
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Jobs",
                CurrentItem = "Loading jobs from source database...",
                CurrentItemIndex = 0,
                TotalItems = 0
            });

            // Get all of the jobs
            SourceDb.Job[] jobs = [.. _sourceDBContext.Jobs.AsNoTracking()];
            Models.Job[] existingJobsCount = [.. _destinationContext.Jobs.Include(x => x.Address).AsNoTracking()];
            if (jobs.Length == existingJobsCount.Length)
            {
                progressCallback.Invoke(new MigrationProgress
                {
                    CurrentStep = "Migrating Jobs",
                    CurrentItem = "Jobs already exists",
                    CurrentItemIndex = 0,
                    TotalItems = 0
                });
                _jobsCache = existingJobsCount.ToFrozenDictionary(x => x.LegacyId ?? 0, y => y);
                return;
            }
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Jobs",
                CurrentItem = $"Found {jobs.Length} jobs",
                CurrentItemIndex = 0,
                TotalItems = jobs.Length
            });

            // Select all of the valid hash colors from the jobs
            List<string?> existingColours = [.. jobs.GroupBy(j => j.Colour)
                .Select(g => g.Key)
                .Where(c => c is not null && c.StartsWith('#') && c.Length == 7)];

            List<Models.JobColour> jobColours = [.. existingColours.Select(x => new Models.JobColour
            {
                Color = x!,
                CreatedAt = DateTime.UtcNow
            })];
            jobColours.Add(new Models.JobColour
            {
                Color = "#FFFFFF",
                CreatedAt = DateTime.UtcNow
            });
            _destinationContext.AddRange(jobColours);
            _destinationContext.SaveChanges();

            List<Models.Job> jobsToCreate = [];
            Dictionary<int, Models.Address> jobAddresses = [];

            // Find list of duplicates
            int?[] duplicateCads = [.. jobs
                .Where(x => x.CadastralJobNumber is not null && x.Deleted == false)
                .GroupBy(x => x.CadastralJobNumber)
                .Select(x => new { CadNumber = x.Key, Count = x.Count() })
                .Where(x => x.Count > 1)
                .Select(x => x.CadNumber)];

            int?[] duplicateSetOuts = [.. jobs
                .Where(x => x.SetoutJobNumber is not null && x.Deleted == false)
                .GroupBy(x => x.SetoutJobNumber)
                .Select(x => new { CadNumber = x.Key, Count = x.Count() })
                .Where(x => x.Count > 1)
                .Select(x => x.CadNumber)];

            for (int i = 0; i < jobs.Length; i++)
            {
                SourceDb.Job oldJob = jobs[i];

                if (i % 1000 == 0)
                {
                    progressCallback.Invoke(new MigrationProgress
                    {
                        CurrentStep = "Migrating Jobs",
                        CurrentItem = $"Processing job {i + 1}/{jobs.Length}: {oldJob.SetoutJobNumber ?? oldJob.CadastralJobNumber ?? 0}",
                        CurrentItemIndex = i + 1,
                        TotalItems = jobs.Length
                    });
                }

                // Find an address
                Models.Address address = Helpers.CreateAddress(_destinationContext, oldJob.State, oldJob.Address, oldJob.Suburb ?? "", oldJob.Postcode);
                if (_Users.TryGetValue(oldJob.CreatedUser ?? 0, out int userId))
                    userId = _Users.First().Value; // Default to first user if not found

                _councilsCache!.TryGetValue(oldJob.CouncilId ?? 0, out Models.Council? council);
                _contactsCache!.TryGetValue(oldJob.ContactId, out Models.Contact? contact);

                if (contact is null)
                    throw new Exception($"Contact with LegacyId {oldJob.ContactId} not found for Job LegacyId {oldJob.Id}");

                int? jobNumber = oldJob.CadastralJobNumber ?? oldJob.SetoutJobNumber;

                Models.Job newJob = new()
                {
                    JobNumber = jobNumber,
                    CreatedByUserId = 95,
                    JobTypeId = oldJob.SetoutJobNumber is not null ? 1 : 2,
                    LegacyId = (int)oldJob.Id,
                    Contact = contact,
                    ContactId = contact.Id,
                    Council = council,
                    CouncilId = council?.Id,
                    DeletedAt = oldJob.DeletedDate is null ? null : Helpers.GetValidDateWithTimezone(oldJob.DeletedDate),
                    JobColour = jobColours.FirstOrDefault(jc => jc.Color == (oldJob.Colour is not null && oldJob.Colour.StartsWith('#') && oldJob.Colour.Length == 7 ? oldJob.Colour : "#FFFFFF")),
                };
                jobAddresses.Add(newJob.LegacyId ?? 0, address);
                jobsToCreate.Add(newJob);
            }

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Jobs",
                CurrentItem = "Saving addresses to destination database...",
                CurrentItemIndex = jobs.Length,
                TotalItems = jobs.Length
            });

            _destinationContext.Addresses.AddRange(jobAddresses.Values);
            _destinationContext.SaveChanges();

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Jobs",
                CurrentItem = "Linking jobs to addresses...",
                CurrentItemIndex = jobs.Length,
                TotalItems = jobs.Length
            });

            foreach (Models.Job job in jobsToCreate)
            {
                job.AddressId = jobAddresses[job.LegacyId ?? 0].Id;
                job.Address = jobAddresses[job.LegacyId ?? 0];
            }

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Jobs",
                CurrentItem = "Saving jobs to destination database...",
                CurrentItemIndex = jobs.Length,
                TotalItems = jobs.Length
            });

            int result = _destinationContext.BulkInsert(jobsToCreate);

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Jobs",
                CurrentItem = $"Successfully migrated {result} jobs",
                CurrentItemIndex = jobsToCreate.Count,
                TotalItems = jobsToCreate.Count
            });

            existingJobsCount = [.. _destinationContext.Jobs.Include(x => x.Address).AsNoTracking()];
            _jobsCache = existingJobsCount.ToFrozenDictionary(x => x.LegacyId ?? 0, y => y);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Migrates the job sub-objects such as notes, tasks, attachments, etc.
    /// Currently migrates job notes with assigned user mapping
    /// </summary>
    /// <param name="progressCallback">Callback function for reporting migration progress</param>
    public void MigratateJobsSubObjects(Action<MigrationProgress> progressCallback)
    {
        try
        {
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Job Notes",
                CurrentItem = "Loading jobs from source database...",
                CurrentItemIndex = 0,
                TotalItems = 0
            });

            // Get all of the jobs
            Note[] notes = [.. _sourceDBContext.Notes.AsNoTracking()];
            Models.JobNote[] existingNotes = [.. _destinationContext.JobNotes.AsNoTracking()];
            if (notes.Length == existingNotes.Length)
            {
                progressCallback.Invoke(new MigrationProgress
                {
                    CurrentStep = "Migrating Job Notes",
                    CurrentItem = "Jobs notes already exist",
                    CurrentItemIndex = 0,
                    TotalItems = 0
                });
                return;
            }

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Job Notes",
                CurrentItem = $"Found {notes.Length} jobs",
                CurrentItemIndex = 0,
                TotalItems = notes.Length
            });

            List<Models.JobNote> notesToCreate = [];
            int index = 0;
            foreach (Note note in notes)
            {
                if (index % 1000 == 0)
                {
                    progressCallback.Invoke(new MigrationProgress
                    {
                        CurrentStep = "Migrating Job Notes",
                        CurrentItem = $"Processing note {index + 1}/{notes.Length}",
                        CurrentItemIndex = index + 1,
                        TotalItems = notes.Length
                    });
                }
                int? assignedUserId = null;
                if (_Users.TryGetValue(note.AssignedTo ?? 0, out int userResult))
                    assignedUserId = userResult;

                Models.JobNote newNote = new()
                {
                    Note = note.Note1,
                    CreatedByUserId = _Users.GetValueOrDefault(note.CreatedUser ?? 0, 95),
                    CreatedOn = Helpers.GetValidDateWithTimezone(note.Created),
                    ModifiedByUserId = note.ModifiedUser is not null ? _Users.GetValueOrDefault(note.ModifiedUser ?? 0, 95) : null,
                    ModifiedOn = Helpers.GetValidDateWithTimezone(note.Modified),
                    DeletedAt = Helpers.GetValidDateWithTimezoneNull(note.DeletedDate),
                    JobId = _jobsCache!.GetValueOrDefault(note.JobId)?.Id ?? throw new Exception($"Job with LegacyId {note.JobId} not found for Note LegacyId {note.Id}"),
                    AssignedUserId = assignedUserId,
                    LegacyId = (int)note.Id,
                    ActionRequired = note.ActionRequired
                };

                notesToCreate.Add(newNote);
                index++;
            }

            _destinationContext.BulkInsert(notesToCreate);
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Job Notes",
                CurrentItem = $"Completed migrating notes",
                CurrentItemIndex = notes.Length,
                TotalItems = notes.Length
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Migrates user-job associations from the source database to the destination database
    /// Links users to jobs they are assigned to
    /// </summary>
    /// <param name="progressCallback">Callback function for reporting migration progress</param>
    public void MigrateUserJobs(Action<MigrationProgress> progressCallback)
    {
        // Get all of the jobs
        JobsUser[] userJobs = [.. _sourceDBContext.JobsUsers.AsNoTracking()];
        Models.UserJob[] userJobsNew = [.. _destinationContext.UserJobs.AsNoTracking()];
        if (userJobs.Length == userJobsNew.Length)
        {
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Job Users",
                CurrentItem = "Job users already exist",
                CurrentItemIndex = 0,
                TotalItems = 0
            });
        }

        progressCallback.Invoke(new MigrationProgress
        {
            CurrentStep = "Migrating Job Users",
            CurrentItem = $"Found {userJobs.Length} jobs",
            CurrentItemIndex = 0,
            TotalItems = userJobs.Length
        });

        List<Models.UserJob> userJobsToCreate = [];

        int index = 0;
        foreach (JobsUser userJob in userJobs)
        {
            if (index % 1000 == 0)
            {
                progressCallback.Invoke(new MigrationProgress
                {
                    CurrentStep = "Migrating User Jobs ",
                    CurrentItem = $"Processing job {index + 1}/{userJobs.Length}",
                    CurrentItemIndex = index + 1,
                    TotalItems = userJobs.Length
                });
            }
            try
            {
                if (_Users.TryGetValue(userJob.UserId, out int userId) &&
                    userId > 0 &&
                    _jobsCache!.TryGetValue(userJob.JobId, out Models.Job? job) &&
                    job is not null)
                {
                    Models.UserJob newUserJob = new()
                    {
                        CreatedByUserId = _Users.GetValueOrDefault(userJob.CreatedUser ?? 0, 95),
                        CreatedOn = Helpers.GetValidDateWithTimezone(userJob.Created),
                        ModifiedByUserId = userJob.ModifiedUser is not null ? _Users.GetValueOrDefault(userJob.ModifiedUser ?? 0, 95) : null,
                        ModifiedOn = Helpers.GetValidDateWithTimezone(userJob.Modified),
                        DeletedAt = Helpers.GetValidDateWithTimezone(userJob.DeletedDate),
                        JobId = job.Id,
                        LegacyId = (int)userJob.Id,
                        UserId = userId
                    };
                    userJobsToCreate.Add(newUserJob);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            index++;
        }

        // Validate all UserJobs have valid UserId and JobId before bulk insert
        List<Models.UserJob> invalidUserJobs = [.. userJobsToCreate.Where(uj => uj.UserId <= 0 || uj.JobId <= 0 || uj.CreatedByUserId <= 0)];
        if (invalidUserJobs.Count != 0)
        {
            throw new Exception($"Found {invalidUserJobs.Count} UserJobs with invalid UserId or JobId. " +
                $"Invalid UserIds: {string.Join(", ", invalidUserJobs.Where(uj => uj.UserId <= 0).Select(uj => uj.UserId).Distinct())}, " +
                $"Invalid JobIds: {string.Join(", ", invalidUserJobs.Where(uj => uj.JobId <= 0).Select(uj => uj.JobId).Distinct())}");
        }

        progressCallback.Invoke(new MigrationProgress
        {
            CurrentStep = "Migrating Job Users",
            CurrentItem = $"Saving Jobs to the database",
            CurrentItemIndex = userJobsToCreate.Count,
            TotalItems = userJobsToCreate.Count
        });
        try
        {
            _destinationContext.BulkInsert(userJobsToCreate);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }

        progressCallback.Invoke(new MigrationProgress
        {
            CurrentStep = "Migrating Job Users",
            CurrentItem = $"Completed migrating job users",
            CurrentItemIndex = userJobsToCreate.Count,
            TotalItems = userJobsToCreate.Count
        });
    }

    /// <summary>
    /// Migrates schedules and schedule tracks from the source database to the destination database
    /// Includes schedule color creation, user assignments, and job associations
    /// </summary>
    /// <param name="progressCallback">Callback function for reporting migration progress</param>
    public void MigrateSchedule(Action<MigrationProgress> progressCallback)
    {
        try
        {

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Schedules",
                CurrentItem = "Loading schedules...",
                CurrentItemIndex = 0,
                TotalItems = 0
            });
            // Get all of the schedule tracks
            SourceDb.ScheduleTrack[] scheduletracksOld = [.. _sourceDBContext.ScheduleTracks.AsNoTracking()];
            Models.ScheduleTrack[] scheduletracksNew = [.. _destinationContext.ScheduleTracks];

            if (scheduletracksOld.Length == scheduletracksNew.Length)
            {
                progressCallback.Invoke(new MigrationProgress
                {
                    CurrentStep = "Migrating Schedule Tracks",
                    CurrentItem = "Schedules exist",
                    CurrentItemIndex = 0,
                    TotalItems = 0
                });
                return;
            }
            Dictionary<int, Models.ScheduleTrack> scheduleTracksToAdd = [];
            Dictionary<int, List<Models.ScheduleUser>> scheduleUsers = [];
            int index = 0;
            foreach (SourceDb.ScheduleTrack scheduleTrackOld in scheduletracksOld)
            {
                if (index % 1000 == 0)
                {
                    progressCallback.Invoke(new MigrationProgress
                    {
                        CurrentStep = "Migrating Schedule Tracks",
                        CurrentItem = $"Processing job {index + 1}/{scheduletracksOld.Length}",
                        CurrentItemIndex = index + 1,
                        TotalItems = scheduletracksOld.Length
                    });
                }

                // Schedule groups currently = 1 = CAD, 2 = Setout
                Models.ScheduleTrack newScheduleTrack = new()
                {
                    JobTypeId = scheduleTrackOld.ScheduleGroupId == 1 ? (int)JobTypeEnum.Surveying : (int)JobTypeEnum.Construction,
                    CreatedByUserId = 95,
                    CreatedOn = Helpers.GetValidDateWithTimezone(scheduleTrackOld.Created),
                    Date = scheduleTrackOld.Date.HasValue ? scheduleTrackOld.Date : null,
                    LegacyId = (int)scheduleTrackOld.Id,
                };
                scheduleTracksToAdd.Add((int)scheduleTrackOld.Id, newScheduleTrack);
                if (scheduleTrackOld.AssigneeUserId1 is not null || scheduleTrackOld.AssigneeUserId2 is not null)
                {
                    List<Models.ScheduleUser> userList = [];

                    if (scheduleTrackOld.AssigneeUserId1 is not null && scheduleTrackOld.AssigneeUserId1 is > 0)
                        userList.Add(new Models.ScheduleUser
                        {
                            CreatedByUserId = 95,
                            CreatedOn = DateTime.UtcNow,
                            UserId = _Users.GetValueRefOrNullRef(scheduleTrackOld.AssigneeUserId1 ?? 0)
                        });

                    if (scheduleTrackOld.AssigneeUserId2 is not null && scheduleTrackOld.AssigneeUserId2 is > 0)
                        userList.Add(new Models.ScheduleUser
                        {
                            CreatedByUserId = 95,
                            CreatedOn = DateTime.UtcNow,
                            UserId = _Users.GetValueRefOrNullRef(scheduleTrackOld.AssigneeUserId2 ?? 0)
                        });

                    scheduleUsers.Add((int)newScheduleTrack.LegacyId, userList);
                }
                index++;
            }
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Schedule Tracks",
                CurrentItem = $"Saving schedule tracks to the database",
                CurrentItemIndex = scheduletracksOld.Length,
                TotalItems = scheduletracksOld.Length
            });
            _destinationContext.AddRange(scheduleTracksToAdd.Values);
            _destinationContext.SaveChanges();

            index = 0;
            // Configure the track Id on the users
            foreach (KeyValuePair<int, List<Models.ScheduleUser>> scheduleUser in scheduleUsers)
            {
                if (index % 1000 == 0)
                {
                    progressCallback.Invoke(new MigrationProgress
                    {
                        CurrentStep = "Migrating Schedule Users",
                        CurrentItem = $"Processing user {index + 1}/{scheduleUsers.Count}",
                        CurrentItemIndex = index + 1,
                        TotalItems = scheduleUsers.Count
                    });
                }
                int scheduleTrackId = scheduleTracksToAdd[scheduleUser.Key].Id;
                foreach (Models.ScheduleUser user in scheduleUser.Value)
                    user.ScheduleTrackId = scheduleTrackId;

                index++;
            }
            // bulk insert the schedule users 
            List<Models.ScheduleUser> scheduleUsersToInsert = [.. scheduleUsers.SelectMany(su => su.Value)];
            _destinationContext.BulkInsert(scheduleUsersToInsert);

            // Get all of the schedules
            SourceDb.Schedule[] schedulesOld = [.. _sourceDBContext.Schedules.AsNoTracking()];
            Models.Schedule[] schedulesNew = [.. _destinationContext.Schedules.AsNoTracking()];

            if (schedulesOld.Length == schedulesNew.Length)
            {
                progressCallback.Invoke(new MigrationProgress
                {
                    CurrentStep = "Migrating Schedules",
                    CurrentItem = "Schedules exist",
                    CurrentItemIndex = 0,
                    TotalItems = 0
                });
                return;
            }

            // Select all of the valid hash colors from the jobs
            List<string?> existingColours = [.. schedulesOld.GroupBy(j => j.Colour)
                .Select(g => g.Key)
                .Where(c => c is not null && c.StartsWith('#') && c.Length == 7)
                .Distinct()];

            List<Models.ScheduleColour> scheduleColours = [.. existingColours.Select(x => new Models.ScheduleColour
            {
                Color = x!,
                CreatedAt = DateTime.UtcNow
            })];
            scheduleColours.Add(new Models.ScheduleColour
            {
                Color = "#FFFFFF",
                CreatedAt = DateTime.UtcNow
            });
            _destinationContext.AddRange(scheduleColours);
            _destinationContext.SaveChanges();
            scheduleColours = [.. _destinationContext.ScheduleColours.AsNoTracking()];

            index = 0;
            List<Models.Schedule> schedulesToAdd = [];

            foreach (SourceDb.Schedule scheduleOld in schedulesOld)
            {
                if (index % 1000 == 0)
                {
                    progressCallback.Invoke(new MigrationProgress
                    {
                        CurrentStep = "Migrating Schedules",
                        CurrentItem = $"Processing schedule {index + 1}/{schedulesOld.Length}",
                        CurrentItemIndex = index + 1,
                        TotalItems = schedulesOld.Length
                    });
                }

                if (scheduleOld.ScheduleTrackId == 33385)
                {
                    ;
                }

                // TODO: fix this later 
                Models.ScheduleColour color = scheduleColours.FirstOrDefault(c => c.Color == (scheduleOld.Colour is not null && scheduleOld.Colour.StartsWith('#') && scheduleOld.Colour.Length == 7 ? scheduleOld.Colour : "#FFFFFF"))
                    ?? scheduleColours.First();

                if (scheduleOld.ScheduleTrackId is not null && scheduleTracksToAdd.TryGetValue(scheduleOld.ScheduleTrackId.Value, out Models.ScheduleTrack? scheduleTrack))
                {
                    Models.Schedule schedule = new()
                    {
                        ScheduleColourId = color.Id,
                        ScheduleTrackId = scheduleTrack.Id,
                        StartTime = Helpers.GetValidDateWithTimezone(scheduleOld.StartTime),
                        EndTime = Helpers.GetValidDateWithTimezone(scheduleOld.EndTime),
                        CreatedByUserId = 95,
                        CreatedOn = Helpers.GetValidDateWithTimezone(scheduleOld.Created),
                        JobId = _jobsCache!.GetValueOrDefault(scheduleOld.JobId)?.Id,
                        Notes = scheduleOld.Notes,
                        LegacyId = (int)scheduleOld.Id
                    };

                    schedulesToAdd.Add(schedule);
                }
            }
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Schedules",
                CurrentItem = $"Saving schedules to the database",
                CurrentItemIndex = schedulesOld.Length,
                TotalItems = schedulesOld.Length
            });
            _destinationContext.BulkInsert(schedulesToAdd);

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Schedules",
                CurrentItem = $"Completed migrating Schedules",
                CurrentItemIndex = schedulesOld.Length,
                TotalItems = schedulesOld.Length
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Migrates tasks from the source database to the destination database
    /// Currently a placeholder implementation - task migration logic to be completed
    /// </summary>
    /// <param name="progressCallback">Callback function for reporting migration progress</param>
    public void MigarateTasks(Action<MigrationProgress> progressCallback)
    {
        progressCallback.Invoke(new MigrationProgress
        {
            CurrentStep = "Migrating Tasks",
            CurrentItem = "Loading jobs from source database...",
            CurrentItemIndex = 0,
            TotalItems = 0
        });

        // Get all of the jobs
        SourceDb.Task[] tasks = [.. _sourceDBContext.Tasks.AsNoTracking()];
        Models.JobTask[] jobTasks = [.. _destinationContext.JobTasks.AsNoTracking()];
        if (tasks.Length == jobTasks.Length)
        {
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Tasks",
                CurrentItem = "Task already exist",
                CurrentItemIndex = 0,
                TotalItems = 0
            });
            return;
        }

        progressCallback.Invoke(new MigrationProgress
        {
            CurrentStep = "Migrating Job Notes",
            CurrentItem = $"Found {tasks.Length} jobs",
            CurrentItemIndex = 0,
            TotalItems = tasks.Length
        });
    }
}
