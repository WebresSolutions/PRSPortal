using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Quote;
using Portal.Shared.ResponseModels;

namespace Portal.Client.Pages.Quotes;

public partial class Quote
{
    internal static readonly CultureInfo QuoteCurrencyCulture = new("en-US");

    [Parameter]
    public required int QuoteId { get; set; }

    private QuoteDetailsDto? _quote;
    private int? _loadedForId;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadIfNeededAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadIfNeededAsync();
    }

    private async Task LoadIfNeededAsync()
    {
        if (_loadedForId == QuoteId)
            return;

        _loadedForId = QuoteId;
        IsLoading = true;
        try
        {
            await LoadQuoteAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadQuoteAsync()
    {
        try
        {
            Result<QuoteDetailsDto> result = await _apiService.GetQuoteDetails(QuoteId);
            if (result.IsSuccess && result.Value is not null)
                _quote = result.Value;
            else
            {
                _quote = null;
                if (result.Error is not ErrorType.NotFound)
                    _snackbar?.Add(result.ErrorDescription ?? "Could not load quote.", Severity.Warning);
            }
        }
        catch (Exception)
        {
            _quote = null;
            _snackbar?.Add("Error loading quote.", Severity.Error);
        }

        await InvokeAsync(StateHasChanged);
    }
}
