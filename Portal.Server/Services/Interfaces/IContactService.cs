using Portal.Shared;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

/// <summary>
/// Interface for contact-related business logic operations
/// </summary>
public interface IContactService
{
    /// <summary>
    /// Get all contacts with pagination, sorting, and filtering options.
    /// </summary>
    /// <param name="page">The page number</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="order">Order by direction (ascending or descending)</param>
    /// <param name="searchFilter">The search filter</param>
    /// <param name="orderby">Column to order by</param>
    /// <returns>A paged list of contacts</returns>
    Task<Result<PagedResponse<ListContactDto>>> GetAllContacts(int page, int pageSize, SortDirectionEnum? order, string? searchFilter, string? orderby);

    /// <summary>
    /// Retrieves detailed information for a contact specified by its unique identifier.
    /// </summary>
    /// <remarks>The returned contact details include associated address and parent contact information. If the specified
    /// contact does not exist or has been deleted, the result will indicate a 'NotFound' error.</remarks>
    /// <param name="contactId">The unique identifier of the contact to retrieve. Must refer to an existing, non-deleted contact.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="Result{ContactDetailsDto}"/> with contact details if found; otherwise, an error indicating that the contact was not
    /// found.</returns>
    Task<Result<ContactDetailsDto>> GetContactDetails(int contactId);

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
    Task<Result<PagedResponse<ListJobDto>>> GetContactJobs(int contactId, int page, int pageSize, SortDirectionEnum? order, string? orderby);
}

