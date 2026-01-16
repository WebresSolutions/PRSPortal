using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Server.Services.Interfaces;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;
using Quartz.Util;

namespace Portal.Server.Services.Instances;

/// <summary>
/// Service implementation for contact-related business logic
/// Handles contact retrieval, filtering, and data transformation
/// </summary>
public class ContactService(PrsDbContext _dbContext, ILogger<ContactService> _logger) : IContactService
{
    /// <summary>
    /// Retrieves a paged list of contacts with optional filtering and sorting
    /// </summary>
    /// <param name="page">The page number to retrieve (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="order">The sort direction (ascending or descending)</param>
    /// <param name="searchFilter">Optional search filter for contact names, emails, or phone numbers</param>
    /// <param name="orderby">Optional field name to sort by</param>
    /// <returns>A result containing a paged response of contact DTOs</returns>
    public async Task<Result<PagedResponse<ListContactDto>>> GetAllContacts(int page, int pageSize, SortDirectionEnum? order, string? searchFilter, string? orderby)
    {
        Result<PagedResponse<ListContactDto>> result = new();
        try
        {
            IQueryable<Contact> contactQuery = _dbContext.Contacts
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .AsQueryable();

            if (!searchFilter.IsNullOrWhiteSpace())
            {
                searchFilter = searchFilter!.Trim();
                bool isNumeric = int.TryParse(searchFilter, out int numericValue);
                contactQuery = contactQuery.Where(contact =>
                            (isNumeric && contact.Id == numericValue)
                            || (contact.SearchVector != null && contact.SearchVector.Matches(searchFilter))
                            || (contact.Address != null && contact.Address.SearchVector != null && contact.Address.SearchVector.Matches(searchFilter)));
            }

            bool isDescending = order is SortDirectionEnum.Desc;
            contactQuery = orderby switch
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
                nameof(ListContactDto.Phone) => isDescending
                    ? contactQuery.OrderByDescending(x => x.Phone)
                    : contactQuery.OrderBy(x => x.Phone),
                // Address sub-properties - EF Core can handle null navigation properties
                $"{nameof(ListContactDto.Address)}.{nameof(AddressDTO.suburb)}" => isDescending
                    ? contactQuery.OrderByDescending(x => x.Address!.Suburb)
                    : contactQuery.OrderBy(x => x.Address!.Suburb),
                $"{nameof(ListContactDto.Address)}.{nameof(AddressDTO.street)}" => isDescending
                    ? contactQuery.OrderByDescending(x => x.Address!.Street)
                    : contactQuery.OrderBy(x => x.Address!.Street),
                $"{nameof(ListContactDto.Address)}.{nameof(AddressDTO.postCode)}" => isDescending
                    ? contactQuery.OrderByDescending(x => x.Address!.PostCode)
                    : contactQuery.OrderBy(x => x.Address!.PostCode),
                _ => contactQuery.OrderByDescending(x => x.Id) // Default ordering by ContactId
            };

            int skipValue = (page - 1) * pageSize;
            List<ListContactDto> contacts = await contactQuery
                        .Skip(skipValue)
                        .Take(pageSize)
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
                            x.ParentContact != null ? new ContactDto(x.ParentContact.Id, x.ParentContact.FullName) : null
                            ))
                        .ToListAsync();
            int total = await contactQuery.CountAsync();
            // Create the paged response
            PagedResponse<ListContactDto> pagedResponse = new(contacts, pageSize, page, total);
            result.Value = pagedResponse;
            return result;
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
                .Where(c => c.Id == contactId && c.DeletedAt == null)
                .Select(c => new
                {
                    c.Id,
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
                    c.CreatedOn
                })
                .FirstOrDefaultAsync();

            if (contactData is null)
                return result.SetError(ErrorType.NotFound, $"Contact not found with Id: {contactId}");

            // Return contact details without jobs (jobs loaded separately)
            result.Value = new ContactDetailsDto(
                contactData.Id,
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
                []); // Empty list - jobs loaded via separate endpoint

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get contact details: {Exception}", ex.Message);
            return result.SetError(ErrorType.InternalError, "An error occurred while getting the contact details");
        }
    }

    /// <summary>
    /// Retrieves the jobs associated with a contact with pagination.
    /// </summary>
    /// <param name="contactId">The unique identifier of the contact.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="order">The sort direction (ascending or descending).</param>
    /// <param name="orderby">Optional field name to sort by.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{PagedResponse{ListJobDto}}"/> object with the paged list of jobs if found; otherwise, contains error information.</returns>
    public async Task<Result<PagedResponse<ListJobDto>>> GetContactJobs(int contactId, int page, int pageSize, SortDirectionEnum? order, string? orderby)
    {
        Result<PagedResponse<ListJobDto>> result = new();
        try
        {
            IQueryable<Data.Models.Job> jobQuery = _dbContext.Jobs
                .AsNoTracking()
                .Where(j => j.ContactId == contactId && j.DeletedAt == null)
                .AsQueryable();

            bool isDescending = order is SortDirectionEnum.Desc;
            jobQuery = orderby switch
            {
                nameof(ListJobDto.JobId) => isDescending
                    ? jobQuery.OrderByDescending(x => x.Id)
                    : jobQuery.OrderBy(x => x.Id),
                nameof(ListJobDto.Contact1) + "." + nameof(ContactDto.fullName) => isDescending
                    ? jobQuery.OrderByDescending(x => x.Contact.FullName)
                    : jobQuery.OrderBy(x => x.Contact.FullName),
                nameof(ListJobDto.JobNumber) => isDescending
                    ? jobQuery.OrderByDescending(x => x.JobNumber)
                    : jobQuery.OrderBy(x => x.JobNumber),
                $"{nameof(ListJobDto.Address)}.{nameof(AddressDTO.suburb)}" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address!.Suburb)
                    : jobQuery.OrderBy(x => x.Address!.Suburb),
                $"{nameof(ListJobDto.Address)}.{nameof(AddressDTO.street)}" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address!.Street)
                    : jobQuery.OrderBy(x => x.Address!.Street),
                $"{nameof(ListJobDto.Address)}.{nameof(AddressDTO.postCode)}" => isDescending
                    ? jobQuery.OrderByDescending(x => x.Address!.PostCode)
                    : jobQuery.OrderBy(x => x.Address!.PostCode),
                _ => jobQuery.OrderByDescending(x => x.Id) // Default ordering by JobId descending
            };

            int skipValue = (page - 1) * pageSize;
            List<ListJobDto> jobs = await jobQuery
                .Skip(skipValue)
                .Take(pageSize)
                .Select(j => new ListJobDto(
                    j.Id,
                    new AddressDTO(j.AddressId!.Value, (StateEnum)j.Address!.StateId!, j.Address!.StateId.Value, j.Address.Suburb, j.Address.Street, j.Address.PostCode),
                    j.Contact != null ? new ContactDto(j.ContactId, j.Contact.FullName) : null,
                    j.Contact != null && j.Contact.ParentContact != null ? new ContactDto(j.Contact.ParentContactId ?? 0, j.Contact.ParentContact!.FullName) : null,
                    j.JobNumber,
                    j.JobType.Name,
                    j.JobType.Id))
                .ToListAsync();

            int total = await jobQuery.CountAsync();

            // Create the paged response
            PagedResponse<ListJobDto> pagedResponse = new(jobs, pageSize, page, total);
            result.Value = pagedResponse;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get contact jobs: {Exception}", ex.Message);
            return result.SetError(ErrorType.InternalError, "An error occurred while getting the contact jobs");
        }
    }
}
