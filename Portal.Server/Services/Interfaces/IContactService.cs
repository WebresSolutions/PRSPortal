using Portal.Shared;
using Portal.Shared.DTO.Contact;
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
    /// <param name="filter">Filter parameters including split search fields (name, email, phone, address, contactId) or searchFilter for type-ahead.</param>
    /// <returns>A paged list of contacts</returns>
    Task<Result<PagedResponse<ListContactDto>>> GetAllContacts(ContactFilterDto filter);

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
}

