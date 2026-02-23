using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Contact;
using Portal.Shared.DTO.Councils;
using Portal.Shared.DTO.Job;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Job;

public partial class CreateJob
{
    /// <summary>
    /// The List of job contacts
    /// </summary>
    private ListContactDto? JobContact;
    /// <summary>
    /// List of job creation dtos
    /// </summary>
    private JobCreationDto Model = null!;
    /// <summary>
    /// List of councils
    /// </summary>
    private CouncilPartialDto[] Councils = [];
    /// <summary>
    /// Flag for if submittiing the new job
    /// </summary>
    private bool Submitting;

    /// <summary>
    /// On initialized method for loading data on the first page load.
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await base.OnInitializedAsync();
        Model = new()
        {
            JobType = JobTypeEnum.Construction,
            JobNumber = 1,
            Address = new()
            {
                LatLng = new(-37.8136, 144.9631)
            }
        };

        Result<CouncilPartialDto[]>? councilResult = await _apiService.GetCouncils();
        if (councilResult?.IsSuccess == true && councilResult.Value is not null)
            Councils = councilResult.Value;


        IsLoading = false;

    }

    private void OnContactChanged(ListContactDto? value)
    {
        JobContact = value;
        Model.ContactId = value?.ContactId ?? 0;
    }


    private async Task SubmitAsync()
    {
        if (Model.JobNumber <= 0)
        {
            _snackbar?.Add("Job number must be greater than 0.", Severity.Warning);
            return;
        }

        Submitting = true;
        try
        {
            Result<int> result = await _apiService.CreateJob(Model);
            if (result.IsSuccess && result.Value > 0)
            {
                _snackbar?.Add("Job created successfully.", Severity.Success);
                _navigationManager.NavigateTo($"/jobs/view/{result.Value}");
            }
            else
                _snackbar?.Add(result.ErrorDescription ?? "Failed to create job.", Severity.Error);
        }
        finally
        {
            Submitting = false;
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
        Result<PagedResponse<ListContactDto>>? contactResult = await _apiService.GetAllContacts(500, 1, search, null, SortDirectionEnum.Asc);

        if (contactResult?.IsSuccess == true && contactResult.Value?.Result is not null)
            return contactResult.Value.Result;
        else
            return [];
    }


}
