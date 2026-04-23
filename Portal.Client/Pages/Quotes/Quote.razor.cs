using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Portal.Client.Components.Quotes;
using Portal.Shared.DTO.Quote;
using Portal.Shared.ResponseModels;
using System.Globalization;

namespace Portal.Client.Pages.Quotes;

public partial class Quote
{
    internal static readonly CultureInfo QuoteCurrencyCulture = new("en-US");

    [Parameter]
    public required int QuoteId { get; set; }

    private QuoteDetailsDto? _quote;
    private int? _loadedForId;
    private QuotePdfDto? _quotePdf;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadQuoteAsync();
        _breadCrumbService.SetBreadCrumbItems(
          [
            new("Quotes", href: "/quotes", disabled: false),
            new($"{_quote?.QuoteReference}", href: $"/quotes/{QuoteId}", disabled: true)
          ]);
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
    }

    private async Task LoadQuoteAsync()
    {
        try
        {
            base.IsLoading = true;
            Result<QuoteDetailsDto> result = await _apiService.GetQuoteDetails(QuoteId);
            if (result.IsSuccess && result.Value is not null)
                _quote = result.Value;
            else
            {
                _quote = null;
                if (result.Error is not ErrorType.NotFound)
                    _snackbar?.Add(result.ErrorDescription ?? "Could not load quote.", Severity.Warning);
            }
            base.IsLoading = false;

            Result<QuotePdfDto> pdfResult = await _apiService.GetQuotePdf(QuoteId);
            if (pdfResult.IsSuccess && pdfResult.Value is not null)
                _quotePdf = pdfResult.Value;
            else
            {
                _quotePdf = null;
                if (pdfResult.Error is not ErrorType.NotFound)
                    _snackbar?.Add(pdfResult.ErrorDescription ?? "Could not load quote PDF.", Severity.Warning);
            }
        }
        catch (Exception)
        {
            _quote = null;
            _snackbar?.Add("Error loading quote.", Severity.Error);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnSendQuoteAsync(MouseEventArgs _)
    {
        IDialogReference sendResult = await _dialog.ShowAsync<SendQuoteConfirmation>("Send Quote", new DialogParameters { ["QuoteDetails"] = _quote }, new() { NoHeader = true });

        if (sendResult.Result.IsCanceled)
            return;

        await LoadQuoteAsync();
    }
}
