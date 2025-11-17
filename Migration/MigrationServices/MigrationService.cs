using Microsoft.EntityFrameworkCore;
using Migration.Display;
using Migration.SourceDb;
using Portal.Data;
using Portal.Data.Models;
using System.Collections.Frozen;

namespace Migration.MigrationServices;

internal class MigrationService(PrsDbContext _destinationContext, SourceDBContext _sourceDBContext, FrozenDictionary<int, int> users) : BaseMigrationClass(_destinationContext, _sourceDBContext)
{
    private FrozenDictionary<int, Portal.Data.Models.Council>? _councilsCache;
    private FrozenDictionary<int, Portal.Data.Models.Contact>? _contactsCache;
    private FrozenDictionary<int, Portal.Data.Models.Job>? _jobsCache;

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
            SourceDb.Contact[] oldContacts = [.. _sourceDBContext.Contacts];
            List<Portal.Data.Models.Contact> contacts = [.. _destinationContext.Contacts];

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

            System.Threading.Tasks.Task.Run(() => _destinationContext.BulkInsert(contactToAdd).Result);
            contacts = [.. _destinationContext.Contacts];
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
            SourceDb.Council[] oldCouncils = [.. _sourceDBContext.Councils];
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
            SourceDb.Job[] jobs = [.. _sourceDBContext.Jobs];
            Portal.Data.Models.Job[] existingJobsCount = [.. _destinationContext.Jobs.Include(x => x.Address)];
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
                if (users.TryGetValue(oldJob.CreatedUser ?? 0, out int userId))
                    userId = users.First().Value; // Default to first user if not found

                _councilsCache!.TryGetValue(oldJob.CouncilId ?? 0, out Portal.Data.Models.Council? council);
                _contactsCache!.TryGetValue(oldJob.ContactId, out Portal.Data.Models.Contact? contact);

                Portal.Data.Models.Job newJob = new()
                {
                    ConstructionNumber = oldJob.SetoutJobNumber,
                    SurveyNumber = oldJob.CadastralJobNumber,
                    CreatedByUserId = 95,
                    JobTypeId = oldJob.SetoutJobNumber is not null ? 1 : 2,
                    LegacyId = (int)oldJob.Id,
                    Contact = contact,
                    ContactId = contact?.Id,
                    Council = council,
                    CouncilId = council?.Id,
                    DeletedAt = Helpers.GetValidDateWithTimezone(oldJob.DeletedDate),
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
                TotalItems = jobs.Lengthsdafasdfew c   fsda112 =
            });
            dsafsdsasdf_destinationContext.BulkInsert(jobsToCreate).Result);

            progressCallback.Invoke(new MigrationProgress
            {
                CurrentStep = "Migrating Jobs",
                CurrentItem = $"Successfully migrated {jobsToCreate.Count} jobs",
                CurrentItemIndex = jobsToCreate.Count,
                TotalItems = jobsToCreate.Count
            });

            existingJobsCount = [.. _destinationContext.Jobs.Include(x => x.Address)];
            _jobsCache = existingJobsCount.ToFrozenDictionary(x => x.LegacyId ?? 0, y => y);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}
