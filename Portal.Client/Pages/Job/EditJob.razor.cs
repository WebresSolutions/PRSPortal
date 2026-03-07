using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
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
    /// If submitting
    /// </summary>
    private bool _isSubmitting;
    /// <summary>
    /// The model
    /// </summary>
    private JobDetailsDto? _model;
    /// <summary>
    /// The List of job contacts
    /// </summary>
    private ListContactDto? _jobContact;

    /// <summary>
    /// List of councils
    /// </summary>
    private CouncilPartialDto[] _councils = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadJobData();
        Result<CouncilPartialDto[]>? councilResult = await _apiService.GetCouncils();
        if (councilResult?.IsSuccess == true && councilResult.Value is not null)
            _councils = councilResult.Value;
        if (_model is { PrimaryContact: not null })
        {
            _jobContact = new()
            {
                ContactId = _model.ContactId,
                FullName = _model.PrimaryContact.ContactName,
                Phone = _model.PrimaryContact.Phone
            };
        }
    }

    private async Task LoadJobData()
    {
        IsLoading = true;
        try
        {
            Result<JobDetailsDto>? result = await _apiService.Job(JobId);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                _model = result.Value;
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
        if (_model!.JobNumber <= 0)
        {
            _snackbar?.Add("Job number must be greater than 0.", Severity.Warning);
            return;
        }

        _isSubmitting = true;
        try
        {
            Result<JobDetailsDto> result = await _apiService.UpdateJob(_model);
            if (result.IsSuccess && result.Value is not null)
            {
                _snackbar?.Add("Job created successfully.", Severity.Success);
                _navigationManager.NavigateTo($"/jobs/view/{result.Value.JobId}");
            }
            else
                _snackbar?.Add(result.ErrorDescription ?? "Failed to create job.", Severity.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void OnContactChanged(ListContactDto? value)
    {
        _jobContact = value;
        _model!.ContactId = value?.ContactId ?? 0;
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