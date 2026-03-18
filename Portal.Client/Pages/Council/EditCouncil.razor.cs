using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Councils;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Council;

public partial class EditCouncil
{
    [Parameter]
    public int CouncilId { get; set; }

    private CouncilUpdateDto? _model;
    private bool _isSubmitting;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCouncilData();
    }

    private async Task LoadCouncilData()
    {
        IsLoading = true;
        try
        {
            Result<CouncilDetailsDto>? result = await _apiService.GetCouncilDetails(CouncilId);
            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                CouncilDetailsDto d = result.Value;
                _model = new CouncilUpdateDto
                {
                    CouncilId = d.CouncilId,
                    CouncilName = d.CouncilName,
                    Phone = d.Phone,
                    Fax = d.Fax,
                    Email = d.Email,
                    Website = d.Website,
                    Address = d.Address != null
                        ? new AddressDTO(d.Address.AddressId, d.Address.State, d.Address.StateId, d.Address.Suburb ?? "", d.Address.Street ?? "", d.Address.PostCode ?? "")
                        {
                            LatLng = d.Address.LatLng ?? new LatLngDto(-37.8136, 144.9631)
                        }
                        : null
                };
            }
            else
            {
                _snackbar?.Add("Error loading council details", Severity.Error);
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
        if (_model is null) return;

        _isSubmitting = true;
        try
        {
            Result<CouncilDetailsDto> result = await _apiService.UpdateCouncil(_model);
            if (result.IsSuccess && result.Value is not null)
            {
                _snackbar?.Add("Council updated successfully.", Severity.Success);
                _navigationManager.NavigateTo($"/councils/{result.Value.CouncilId}");
            }
            else
                _snackbar?.Add(result.ErrorDescription ?? "Failed to update council.", Severity.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
