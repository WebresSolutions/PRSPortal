using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Job;

public partial class CreateJob
{
    /// <summary>
    /// The List of job contacts
    /// </summary>
    private ListContactDto? _jobContact;
    /// <summary>
    /// List of job creation dtos
    /// </summary>
    private JobCreationDto _model = null!;
    /// <summary>
    /// List of councils
    /// </summary>
    private CouncilPartialDto[] _councils = [];
    private JobTypeEnum[] _jobtypes = [];
    private JobTypeStatusDto[] _statuses = [];
    /// <summary>
    /// On initialized method for loading data on the first page load.
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await base.OnInitializedAsync();

        Result<JobTypeStatusDto[]> statusesResult = await _apiService.GetJobStatuses();
        if (statusesResult.IsSuccess)
            _statuses = statusesResult.Value!;

        _model = new()
        {
            JobType = [JobTypeEnum.Construction],
            Address = new()
            {
                LatLng = new(-37.8136, 144.9631)
            },
            StatusId = _statuses.OrderBy(x => x.Sequence).First().Id
        };

        Result<CouncilPartialDto[]>? councilResult = await _apiService.GetCouncils();
        if (councilResult?.IsSuccess == true && councilResult.Value is not null)
            _councils = councilResult.Value;

        IsLoading = false;

    }

    /// <summary>
    /// Handles the selected contact change from the type-ahead and updates the job creation model.
    /// </summary>
    /// <param name="value">The selected contact, or null if cleared.</param>
    private void OnContactChanged(ListContactDto? value)
    {
        _jobContact = value;
        _model.ContactId = value?.ContactId ?? 0;
    }

    /// <summary>
    /// Submits the job creation form, validates the model, and navigates to the new job on success.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SubmitAsync()
    {
        try
        {
            base.IsLoading = true;
            _model.StatusId = _statuses.Where(x => x.JobTypeId == (int)_model.JobType.First()).OrderBy(x => x.Sequence).First().Id;
            Result<int> result = await _apiService.CreateJob(_model);
            if (result.IsSuccess && result.Value > 0)
            {
                _snackbar?.Add("Job created successfully.", Severity.Success);
                _navigationManager.NavigateTo($"/jobs/{result.Value}");
            }
            else
                _snackbar?.Add(result.ErrorDescription ?? "Failed to create job.", Severity.Error);

            base.IsLoading = false;
        }
        finally
        {
        }
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
    private void OnJobStatusIdChanged(int value)
    {
        if (_model is null)
            return;
        _model.StatusId = value;
        StateHasChanged();
    }

}
