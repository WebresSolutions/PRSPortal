using MudBlazor;
using Nextended.Core.Extensions;
using Portal.Shared.DTO.Councils;
using Portal.Shared.ResponseModels;
using System.ComponentModel.DataAnnotations;

namespace Portal.Client.Pages.Council;

public partial class CreateCouncil
{
    private CouncilCreationDto _model = null!;
    private bool _submitting;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await base.OnInitializedAsync();
        _model = new()
        {
            Address = new()
            {
                LatLng = new(-37.8136, 144.9631)
            }
        };
        IsLoading = false;
    }

    private async Task SubmitAsync()
    {
        _submitting = true;
        try
        {
            IEnumerable<ValidationResult> res = _model.Validate();
            if (res.IsEmpty())
            {
                Result<int> result = await _apiService.CreateCouncil(_model);
                if (result.IsSuccess && result.Value > 0)
                {
                    _snackbar?.Add("Council created successfully.", Severity.Success);
                    _navigationManager.NavigateTo($"/councils/{result.Value}");
                }
                else
                    _snackbar?.Add(result.ErrorDescription ?? "Failed to create council.", Severity.Error);
            }
            else
            {
                _snackbar?.Add(res.Select(x => x.ErrorMessage).JoinWith(" "), Severity.Error);
            }
        }
        finally
        {
            _submitting = false;
        }
    }
}
