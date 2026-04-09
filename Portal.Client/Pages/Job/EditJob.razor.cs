using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Types;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Job;

public partial class EditJob
{
    /// <summary>
    /// The Job id
    /// </summary>
    [Parameter]
    public int JobId { get; set; }

    #region  Private Fields
    private JobUpdateDto? _model;
    private ListContactDto? _jobContact;
    private CouncilPartialDto[] _councils = [];
    private JobTypeStatusDto[] _JobTypeStatusDtos = [];
    private readonly JobTypeEnum[] _jobtypes = [.. Enum.GetValues<JobTypeEnum>().Cast<int>().Select(x => (JobTypeEnum)x)];
    private List<JobTypeEnum> _selectedJobtypes = [];
    private UserDto[] _users = [];
    #endregion

    /// <summary>
    /// OnInitializedAsync is called when the component is initialized. 
    /// It loads the job data and related data such as councils and users. 
    /// It also handles any errors that occur during loading and displays appropriate messages to the user.
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadJobData();
        Result<CouncilPartialDto[]>? councilResult = await _apiService.GetCouncils();
        if (councilResult?.IsSuccess == true && councilResult.Value is not null)
            _councils = councilResult.Value;

        Result<UserDto[]> users = await _apiService.GetUsersList();
        if (users.IsSuccess && users.Value is not null)
            _users = users.Value;
        else
            _snackbar.Add(users.ErrorDescription ?? "Error occured while loading the users", Severity.Error);

        _breadCrumbService.SetBreadCrumbItems(
          [
                new("Jobs", href: "/jobs", disabled: false),
                new($"Edit", href: $"/jobs/edit", disabled: true)
          ]);
    }

    /// <summary>
    /// Sets the selected job types when the user selects them from the multi select component. 
    /// This is used to update the model before submitting the form
    /// </summary>
    /// <param name="jobTypes"></param>
    private void OnSelect(IEnumerable<JobTypeEnum> jobTypes) => _selectedJobtypes = jobTypes?.ToList() ?? [];

    /// <summary>
    /// Loads the job data for the given JobId and populates the model and related properties. Displays error messages if loading fails
    /// </summary>
    /// <returns></returns>
    private async Task LoadJobData()
    {
        IsLoading = true;
        try
        {
            Result<JobDetailsDto>? result = await _apiService.Job(JobId);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                _model = new()
                {
                    JobId = result.Value.JobId,
                    Address = result.Value.Address,
                    ContactId = result.Value.ContactId,
                    CouncilId = result.Value.CouncilId,
                    Details = result.Value.Description,
                    JobColourId = result.Value.JobColourId,
                    JobStatusId = result.Value.JobStatusId,
                    JobTypes = result.Value.JobTypes,
                    ResponsibleTeamMember = result.Value.AssignedUsers?.FirstOrDefault()?.UserId,
                    Contact = result.Value.PrimaryContact is not null ? new()
                    {
                        ContactId = result.Value.PrimaryContact.ContactId,
                        ContactName = result.Value.PrimaryContact.ContactName,
                        Phone = result.Value.PrimaryContact.Phone ?? "",
                        Email = result.Value.PrimaryContact.Email,
                    } : new(),
                    LatestClientUpdate = result.Value.LatestClientUpdate,
                    TargetDeliveryDate = result.Value.TargetDeliveryDate,
                };
                _selectedJobtypes = [.. _model.JobTypes];
                _JobTypeStatusDtos = result.Value.JobTypeStatuses;
                if (_model is { Contact: not null })
                {
                    _jobContact = new()
                    {
                        ContactId = _model.ContactId,
                        FullName = _model.Contact.ContactName,
                        Phone = _model.Contact.Phone
                    };
                }
            }
            else
            {
                _snackbar?.Add("Error loading job details", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            _snackbar?.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Submits the form to update the job. 
    /// It validates the form data, updates the model with selected job types, and calls the API to update the job.
    ///  Displays success or error messages based on the result of the API call.
    /// </summary>
    /// <returns></returns>
    private async Task SubmitAsync()
    {
        try
        {


            if (_model is null)
                return;

            _model.JobTypes = [.. _selectedJobtypes];

            if (_model.JobTypes.Length < 1)
            {
                _snackbar.Add("Must Select at least on Job Type");
                return;
            }

            if (_model.TargetDeliveryDate is not null && _model.TargetDeliveryDate < DateTime.Today)
            {
                _snackbar.Add("Target Delivery Date cannot be in the past", Severity.Error);
                return;
            }


            Result<JobDetailsDto> result = await _apiService.UpdateJob(_model);
            if (result.IsSuccess && result.Value is not null)
            {
                _snackbar?.Add("Job updated successfully.", Severity.Success);
                _navigationManager.NavigateTo($"/jobs/{result.Value.JobId}");
            }
            else
                _snackbar?.Add(result.ErrorDescription ?? "Failed to create job.", Severity.Error);
        }
        finally
        {

        }
    }

    private void OnContactChanged(ListContactDto? value)
    {
        _jobContact = value;
        _model!.ContactId = value?.ContactId ?? 0;
    }

    private void OnJobStatusIdChanged(int? value)
    {
        if (_model is null)
            return;
        _model.JobStatusId = value;
        StateHasChanged();
    }

    /// <summary>
    /// Used by the type ahead auto complete for searching contacts
    /// </summary>
    /// <param name="search">The search string</param>
    /// <param name="token">The token</param>
    /// <returns></returns>
    public async Task<IEnumerable<ListContactDto>> SearchContacts(string search, CancellationToken token)
    {
        ContactFilterDto filter = new(Page: 1, PageSize: 500, SearchFilter: search, OrderBy: $"{nameof(ListContactDto.FullName)}", Order: Portal.Shared.SortDirectionEnum.Desc);
        Result<PagedResponse<ListContactDto>>? contactResult = await _apiService.GetAllContacts(filter);

        if (contactResult?.IsSuccess == true && contactResult.Value?.Result is not null)
            return contactResult.Value.Result;
        else
            return [];
    }


}