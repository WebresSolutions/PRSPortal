using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.ResponseModels;
using Quartz.Util;

namespace Portal.Server.Services.Instances;

/// <summary>
/// Service implementation for contact-related business logic
/// Handles contact retrieval, filtering, and data transformation
public class ContactService(PrsDbContext _dbContext, ILogger<ContactService> _logger) : IContactService
/// </summary>ctService
{
    /// <summary>
    /// Retrieves a paged list of contacts with optional filtering and sorting
    /// </summary>
    /// <param name="filter">Filter parameters including split search fields or searchFilter for type-ahead</param>
    /// <returns>A result containing a paged response of contact DTOs</returns>
    public async Task<Result<PagedResponse<ListContactDto>>> GetAllContacts(ContactFilterDto filter)
    {
        Result<PagedResponse<ListContactDto>> result = new();
        try
        {
            IQueryable<Contact> contactQuery = _dbContext.Contacts
                .AsNoTracking()
                .AsQueryable();

            contactQuery = !filter.Deleted ? contactQuery.Where(x => x.DeletedAt == null) : contactQuery.Where(x => x.DeletedAt != null);

            bool hasSplitSearch = !filter.NameEmailPhoneSearch.IsNullOrWhiteSpace() || !filter.AddressSearch.IsNullOrWhiteSpace();

            if (hasSplitSearch)
            {
                if (!filter.NameEmailPhoneSearch.IsNullOrWhiteSpace())
                {
                    string nameEmailSearch = filter.NameEmailPhoneSearch!.Trim();
                    string pattern = PartialMatch(nameEmailSearch);

                    contactQuery = contactQuery.Where(contact =>
                        contact.SearchVector.Matches(EF.Functions.ToTsQuery(pattern)));
                }
                if (!filter.AddressSearch.IsNullOrWhiteSpace())
                {
                    string addressSearch = filter.AddressSearch!.Trim();
                    string pattern = PartialMatch(addressSearch);

                    contactQuery = contactQuery.Where(contact =>
                        contact.Address != null && contact.Address.SearchVector != null && contact.Address.SearchVector.Matches(EF.Functions.ToTsQuery(pattern)));
                }
            }
            else if (!filter.SearchFilter.IsNullOrWhiteSpace())
            {
                string searchFilter = filter.SearchFilter!.Trim();
                bool isNumeric = int.TryParse(searchFilter, out int numericValue);
                string pattern = PartialMatch(searchFilter);

                contactQuery = contactQuery.Where(contact =>
                            (isNumeric && contact.Id == numericValue)
                            || (contact.SearchVector != null && contact.SearchVector.Matches(EF.Functions.ToTsQuery(pattern)))
                            || (contact.Address != null && contact.Address.SearchVector != null && contact.Address.SearchVector.Matches(EF.Functions.ToTsQuery(pattern))));
            }

            if (filter.contactType is not null)
                contactQuery = contactQuery.Where(contact => contact.TypeId == (int)filter.contactType);

            bool isDescending = filter.Order is SortDirectionEnum.Asc;
            contactQuery = filter.OrderBy switch
            {
                nameof(ListContactDto.ContactId) => isDescending
                    ? contactQuery.OrderByDescending(x => x.Id)
                    : contactQuery.OrderBy(x => x.Id),
                nameof(ListContactDto.FullName) => isDescending
                    ? contactQuery.OrderByDescending(x => x.FullName)
                    : contactQuery.OrderBy(x => x.FullName),
                nameof(ListContactDto.Email) => isDescending
                    ? contactQuery.OrderByDescending(x => x.Email)
                    : contactQuery.OrderBy(x => x.Email),
                nameof(ListContactDto.ContactType) => isDescending
                    ? contactQuery.OrderByDescending(x => x.TypeId)
                    : contactQuery.OrderBy(x => x.TypeId),
                nameof(ListContactDto.Phone) => isDescending
                    ? contactQuery.OrderByDescending(x => x.Phone)
                    : contactQuery.OrderBy(x => x.Phone),
                // Address sub-properties - EF Core can handle null navigation properties
                $"{nameof(ListContactDto.Address)}.{nameof(AddressDTO.Suburb)}" => isDescending
                    ? contactQuery.OrderByDescending(x => x.Address!.Suburb)
                    : contactQuery.OrderBy(x => x.Address!.Suburb),
                $"{nameof(ListContactDto.Address)}.{nameof(AddressDTO.Street)}" => isDescending
                    ? contactQuery.OrderByDescending(x => x.Address!.Street)
                    : contactQuery.OrderBy(x => x.Address!.Street),
                $"{nameof(ListContactDto.Address)}.{nameof(AddressDTO.PostCode)}" => isDescending
                    ? contactQuery.OrderByDescending(x => x.Address!.PostCode)
                    : contactQuery.OrderBy(x => x.Address!.PostCode),
                _ => contactQuery.OrderByDescending(x => x.Id) // Default ordering by ContactId
            };

            int skipValue = (filter.Page - 1) * filter.PageSize;
            List<ListContactDto> contacts = await contactQuery
                        .Skip(skipValue)
                        .Take(filter.PageSize)
                        .Select(x => new ListContactDto(
                            x.Id,
                            x.FullName,
                            x.Email,
                            x.Phone,
                            x.Address != null ? new AddressDTO(
                                x.AddressId ?? 1,
                                (StateEnum)x.Address.StateId!,
                                x.Address.StateId ?? 3,
                                x.Address.Suburb,
                                x.Address.Street,
                                x.Address.PostCode) : null,
                            x.ParentContact != null ? new ContactDto(x.ParentContact.Id, x.ParentContact.FullName) : null,
                            (ContactTypeEnum)x.TypeId
                            ))
                        .ToListAsync();
            int total = await contactQuery.CountAsync();
            // Create the paged response
            PagedResponse<ListContactDto> pagedResponse = new(contacts, filter.PageSize, filter.Page, total);
            result.Value = pagedResponse;
            return result;

            static string PartialMatch(string filter) => string.Join(" & ", filter.Split(' ', StringSplitOptions.RemoveEmptyEntries)) + ":*";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all contacts");
            return result.SetError(ErrorType.InternalError, "Failed to get list of contacts");
        }
    }

    /// <summary>
    /// Retrieves detailed information for a contact specified by its unique identifier.
    /// </summary>
    /// <remarks>The returned contact details include associated address and parent contact information. If the specified
    /// contact does not exist or has been deleted, the result will indicate a 'NotFound' error.</remarks>
    /// <param name="contactId">The unique identifier of the contact to retrieve. Must refer to an existing, non-deleted contact.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{ContactDetailsDto}"/> with contact details if found; otherwise, an error indicating that the contact was not
    /// found.</returns>
    public async Task<Result<ContactDetailsDto>> GetContactDetails(int contactId)
    {
        Result<ContactDetailsDto> result = new();
        try
        {
            var contactData = await _dbContext.Contacts
                .AsNoTracking()
                .AsSplitQuery()
                .Where(c => c.Id == contactId && c.DeletedAt == null)
                .Select(c => new
                {
                    c.Id,
                    c.TypeId,
                    TypeName = c.Type.Name,
                    c.FullName,
                    c.FirstName,
                    c.LastName,
                    c.Email,
                    c.Phone,
                    c.Fax,
                    Address = c.Address != null ?
                        new AddressDTO(
                            c.Address.Id,
                            (StateEnum)c.Address.StateId!,
                            c.Address.StateId ?? 3,
                            c.Address.Suburb,
                            c.Address.Street,
                            c.Address.PostCode)
                        : null,
                    ParentContact = c.ParentContact != null ?
                        new ContactDto(c.ParentContact.Id, c.ParentContact.FullName)
                        : null,
                    CreatedBy = c.CreatedByUser.DisplayName ?? c.CreatedByUser.Email ?? "Unknown",
                    c.CreatedOn,
                    subContactCount = c.InverseParentContact.Count(sc => sc.DeletedAt == null && sc.ParentContactId == contactId),
                    techContactCount = c.TechnicalContacts.Count(j => j.DeletedAt == null && j.ContactId == contactId),
                    invoiceCount = c.Invoices.Count(i => i.DeletedAt == null && i.ContactId == contactId),
                    subcontacts = c.InverseParentContact.Select(sub => new SubContactDto(
                        sub.Id,
                        c.Id,
                        sub.TypeId,
                        sub.Type.Name,
                        sub.FullName,
                        "",
                        sub.Phone,
                        sub.Email,
                        sub.DeletedAt != null
                    )).ToArray()
                })
                .FirstOrDefaultAsync();

            if (contactData is null)
                return result.SetError(ErrorType.NotFound, $"Contact not found with Id: {contactId}");

            // Get sub contacts and build single list for job IN clause (parent + sub-contact ids)
            int[] subContacts = await _dbContext.Contacts
                .AsNoTracking()
                .Where(c => c.ParentContactId == contactId && c.DeletedAt == null)
                .Select(sub => sub.Id)
                .ToArrayAsync();
            int[] contactIdsForJobs = [contactId, .. subContacts];
            int jobsCount = await _dbContext.Jobs.CountAsync(j => j.DeletedAt == null && contactIdsForJobs.Contains(j.ContactId));

            result.Value = new ContactDetailsDto(
                contactData.Id,
                contactData.TypeId,
                contactData.TypeName,
                contactData.FullName,
                contactData.FirstName,
                contactData.LastName,
                contactData.Email,
                contactData.Phone,
                contactData.Fax,
                contactData.Address,
                contactData.ParentContact,
                contactData.CreatedBy,
                contactData.CreatedOn,
                jobsCount,
                contactData.techContactCount,
                contactData.invoiceCount,
                contactData.subContactCount,
                contactData.subcontacts
                );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get contact details: {Exception}", ex.Message);
            return result.SetError(ErrorType.InternalError, "An error occurred while getting the contact details");
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> CreateContact(HttpContext httpContext, ContactCreationDto data)
    {
        Result<int> result = new();
        try
        {
            if (await _dbContext.ContactTypes.FirstOrDefaultAsync(t => t.Id == data.TypeId) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid contact type");

            if (data.ParentContactId.HasValue &&
                await _dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == data.ParentContactId.Value && c.DeletedAt == null) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid parent contact");

            Address? address = null;
            if (data.Address is not null)
            {
                address = new Address
                {
                    Street = data.Address.Street ?? "",
                    PostCode = data.Address.PostCode ?? "",
                    Suburb = data.Address.Suburb ?? "",
                    StateId = (int?)data.Address.State ?? (int)StateEnum.VIC,
                    CreatedByUserId = httpContext.UserId(),
                    Country = "AUS"
                };
                if (data.Address.LatLng is not null)
                    address.Geom = new Point(new Coordinate(data.Address.LatLng.Latitude, data.Address.LatLng.Longitude));
                await _dbContext.Addresses.AddAsync(address);
                await _dbContext.SaveChangesAsync();
            }

            Contact contact = new()
            {
                TypeId = data.TypeId,
                ParentContactId = data.ParentContactId,
                FirstName = data.FirstName.Trim(),
                LastName = data.LastName.Trim(),
                Email = data.Email.Trim(),
                Phone = data.Phone?.Trim(),
                Fax = data.Fax?.Trim(),
                AddressId = address?.Id,
                CreatedByUserId = httpContext.UserId(),
                CreatedOn = DateTime.UtcNow
            };
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.SaveChangesAsync();
            return result.SetValue(contact.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create contact");
            return result.SetError(ErrorType.InternalError, "Failed to create contact");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ContactDetailsDto>> UpdateContact(HttpContext httpContext, ContactUpdateDto data)
    {
        Result<ContactDetailsDto> result = new();
        try
        {
            Contact? contact = await _dbContext.Contacts
                .Include(c => c.Address)
                .Where(c => c.Id == data.ContactId && c.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (contact is null)
                return result.SetError(ErrorType.NotFound, "Contact not found");

            if (await _dbContext.ContactTypes.FirstOrDefaultAsync(t => t.Id == data.TypeId) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid contact type");

            if (data.ParentContactId.HasValue &&
                await _dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == data.ParentContactId.Value && c.DeletedAt == null) is null)
                return result.SetError(ErrorType.BadRequest, "Invalid parent contact");

            contact.TypeId = data.TypeId;
            contact.ParentContactId = data.ParentContactId;
            contact.FirstName = data.FirstName.Trim();
            contact.LastName = data.LastName.Trim();
            contact.Email = data.Email.Trim();
            contact.Phone = data.Phone?.Trim();
            contact.Fax = data.Fax?.Trim();
            contact.ModifiedByUserId = httpContext.UserId();
            contact.ModifiedOn = DateTime.UtcNow;

            if (contact.Address is not null && data.Address is not null)
            {
                contact.Address.Street = data.Address.Street ?? "";
                contact.Address.PostCode = data.Address.PostCode ?? "";
                contact.Address.Suburb = data.Address.Suburb ?? "";
                contact.Address.StateId = (int?)data.Address.State ?? (int)StateEnum.VIC;
                contact.Address.ModifiedByUserId = httpContext.UserId();
                contact.Address.ModifiedOn = DateTime.UtcNow;
                if (data.Address.LatLng is not null)
                    contact.Address.Geom = new Point(new Coordinate(data.Address.LatLng.Latitude, data.Address.LatLng.Longitude));
                else
                    contact.Address.Geom = null;
            }
            else if (data.Address is not null)
            {
                Address newAddress = new()
                {
                    Street = data.Address.Street ?? "",
                    PostCode = data.Address.PostCode ?? "",
                    Suburb = data.Address.Suburb ?? "",
                    StateId = (int?)data.Address.State ?? (int)StateEnum.VIC,
                    CreatedByUserId = httpContext.UserId(),
                    Country = "AUS"
                };
                if (data.Address.LatLng is not null)
                    newAddress.Geom = new Point(new Coordinate(data.Address.LatLng.Latitude, data.Address.LatLng.Longitude));
                await _dbContext.Addresses.AddAsync(newAddress);
                await _dbContext.SaveChangesAsync();
                contact.AddressId = newAddress.Id;
                contact.Address = newAddress;
            }

            await _dbContext.SaveChangesAsync();
            return await GetContactDetails(contact.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update contact");
            return result.SetError(ErrorType.InternalError, "Failed to update contact");
        }
    }
}
