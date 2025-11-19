using Microsoft.EntityFrameworkCore;
using Migration.Display;
using Migration.SourceDb;
using Portal.Data;
using Portal.Data.Models;
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
    private FrozenDictionary<int, Portal.Data.Models.Council>? _councilsCache;
    /// <summary>
    /// Cache of contacts to avoid multiple DB hits. Key is the legacy ID.
    /// </summary>
    private FrozenDictionary<int, Portal.Data.Models.Contact>? _contactsCache;
    /// <summary>
    /// Cache of jobs to avoid multiple DB hits. Key is the legacy ID.
    /// </summary>
    private FrozenDictionary<int, Portal.Data.Models.Job>? _jobsCache;

    /// <summary>
    /// Cashe of users to avoid multiple DB hits. Key is the legacy ID, value is the new user ID.
    /// </summary>
    private readonly FrozenDictionary<int, int> _Users = users;

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
            List<Portal.Data.Models.Contact> councilsToCreate = [];
            SourceDb.Contact[] oldContacts = [.. _sourceDBContext.Contacts.AsNoTracking()];
            List<Portal.Data.Models.Contact> contacts = [.. _destinationContext.Contacts.AsNoTracking()];

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

            Dictionary<int, Address> addressesToAdd = [];
            List<Portal.Data.Models.Contact> contactToAdd = [];
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
                Portal.Data.Models.Contact newContact = new()
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

                Address address = Helpers.CreateAddress(_destinationContext, oldContact.State, oldContact.Address ?? "", oldContact.Suburb ?? "", oldContact.Postcode);
                addressesToAdd.Add((int)oldContact.Id, address);
                contactToAdd.Add(newContact);
                index++;
            }
            _destinationContext.Addresses.AddRange(addressesToAdd.Values);
            _destinationContext.SaveChanges();

            foreach (Portal.Data.Models.Contact contact in contactToAdd)
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
            foreach (Portal.Data.Models.Contact contact in contacts)
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
            List<Portal.Data.Models.Council> councilsToCreate = [];
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

            Dictionary<int, Address> addressesToAdd = [];
            List<Portal.Data.Models.Council> councilsToAdd = [];
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
                Portal.Data.Models.Council newCouncil = new()
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

                Address address = Helpers.CreateAddress(_destinationContext, oldCouncil.State, oldCouncil.Address ?? "", oldCouncil.Suburb ?? "", oldCouncil.Postcode);
                addressesToAdd.Add((int)oldCouncil.Id, address);
                councilsToAdd.Add(newCouncil);
            }

            _destinationContext.Addresses.AddRange(addressesToAdd.Values);
            _destinationContext.SaveChanges();

            foreach (Portal.Data.Models.Council council in councilsToAdd)
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
            Portal.Data.Models.Job[] existingJobsCount = [.. _destinationContext.Jobs.Include(x => x.Address).AsNoTracking()];
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

            List<Portal.Data.Models.Job> jobsToCreate = [];
            Dictionary<int, Address> jobAddresses = [];

            for (int i = 0; i < jobs.Length; i++)
            {
                SourceDb.Job oldJob = jobs[i];
                if (i % 100 == 0)
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
                Address address = Helpers.CreateAddress(_destinationContext, oldJob.State, oldJob.Address, oldJob.Suburb ?? "", oldJob.Postcode);
                if (_Users.TryGetValue(oldJob.CreatedUser ?? 0, out int userId))
                    userId = _Users.First().Value; // Default to first user if not found

                _councilsCache!.TryGetValue(oldJob.CouncilId ?? 0, out Portal.Data.Models.Council? council);
                _contactsCache!.TryGetValue(oldJob.ContactId, out Portal.Data.Models.Contact? contact);

                if (contact is null)
                    throw new Exception($"Contact with LegacyId {oldJob.ContactId} not found for Job LegacyId {oldJob.Id}");

                Portal.Data.Models.Job newJob = new()
                {
                    ConstructionNumber = oldJob.SetoutJobNumber,
                    SurveyNumber = oldJob.CadastralJobNumber,
                    CreatedByUserId = 95,
                    JobTypeId = oldJob.SetoutJobNumber is not null ? 1 : 2,
                    LegacyId = (int)oldJob.Id,
                    Contact = contact,
                    ContactId = contact.Id,
                    Council = council,
                    CouncilId = council?.Id,
                    DeletedAt = oldJob.DeletedDate is null ? null : Helpers.GetValidDateWithTimezone(oldJob.DeletedDate),
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

            foreach (Portal.Data.Models.Job job in jobsToCreate)
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
    /// </summary>
    /// <param name="progressCallback"></param>
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
            JobNote[] existingNotes = [.. _destinationContext.JobNotes.AsNoTracking()];
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


            List<JobNote> notesToCreate = [];
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

                JobNote newNote = new()
                {
                    Note = note.Note1,
                    CreatedByUserId = _Users.GetValueOrDefault(note.CreatedUser ?? 0, 95),
                    CreatedOn = Helpers.GetValidDateWithTimezone(note.Created),
                    ModifiedByUserId = note.ModifiedUser is not null ? _Users.GetValueOrDefault(note.ModifiedUser ?? 0, 95) : null,
                    ModifiedOn = Helpers.GetValidDateWithTimezone(note.Modified),
                    DeletedAt = Helpers.GetValidDateWithTimezone(note.DeletedDate),
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

            // Get all of the jobs
            JobsUser[] userJobs = [.. _sourceDBContext.JobsUsers.AsNoTracking()];
            UserJob[] userJobsNew = [.. _destinationContext.UserJobs.AsNoTracking()];
            if (userJobs.Length == userJobsNew.Length)
            {
                progressCallback.Invoke(new MigrationProgress
                {
                    CurrentStep = "Migrating Job Users",
                    CurrentItem = "Job users already exist",
                    CurrentItemIndex = 0,
                    TotalItems = 0
                });
                return;
            }

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Job Users",
                CurrentItem = $"Found {userJobs.Length} jobs",
                CurrentItemIndex = 0,
                TotalItems = notes.Length
            });

            List<UserJob> userJobsToCreate = [];

            index = 0;
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
                    int jobId = 0;
                    // For some reason there are some user jobs which reference a job which does not exist.
                    if (!_jobsCache!.TryGetValue(userJob.JobId, out Portal.Data.Models.Job? job))
                        continue;

                    jobId = job.Id;
                    int userId = _Users.GetValueRefOrNullRef(userJob.UserId);
                    UserJob newUserJob = new()
                    {
                        CreatedByUserId = _Users.GetValueOrDefault(userJob.CreatedUser ?? 0, 95),
                        CreatedOn = Helpers.GetValidDateWithTimezone(userJob.Created),
                        ModifiedByUserId = userJob.ModifiedUser is not null ? _Users.GetValueOrDefault(userJob.ModifiedUser ?? 0, 95) : null,
                        ModifiedOn = Helpers.GetValidDateWithTimezone(userJob.Modified),
                        DeletedAt = Helpers.GetValidDateWithTimezone(userJob.DeletedDate),
                        JobId = jobId,
                        LegacyId = (int)userJob.Id,
                        UserId = userId
                    };

                    userJobsToCreate.Add(newUserJob);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
                index++;
            }

            _destinationContext.BulkInsert(userJobsToCreate);
            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Job Users",
                CurrentItem = $"Completed migrating job users",
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
}
