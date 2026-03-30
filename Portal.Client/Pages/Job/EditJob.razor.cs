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
    /// <summary>
    /// The model
    /// </summary>
    private JobUpdateDto? _model;
    /// <summary>
    /// The List of job contacts
    /// </summary>
    private ListContactDto? _jobContact;
    /// <summary>
    /// List of councils
    /// </summary>
    private CouncilPartialDto[] _councils = [];

    private JobTypeStatusDto[] _JobTypeStatusDtos = [];

    private readonly JobTypeEnum[] _jobtypes = [.. Enum.GetValues<JobTypeEnum>().Cast<int>().Select(x => (JobTypeEnum)x)];
    private List<JobTypeEnum> _selectedJobtypes = [];
    private UserDto[] _users = [];
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

    }

    private void OnSelect(IEnumerable<JobTypeEnum> jobTypes)
    {
        _selectedJobtypes = jobTypes?.ToList() ?? [];
    }

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
                    Details = result.Value.Details,
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
                    } : new()
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

    private async Task SubmitAsync()
    {
        try
        {
            if (_model is null)
                return;

            if (_model.JobTypes.Length < 1)
            {
                _snackbar.Add("Must Select at least on Job Type");
                return;
            }

            _model.JobTypes = [.. _selectedJobtypes];

            Result<JobDetailsDto> result = await _apiService.UpdateJob(_model);
            if (result.IsSuccess && result.Value is not null)
            {
                _snackbar?.Add("Job created successfully.", Severity.Success);
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