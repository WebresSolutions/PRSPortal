using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Quote.PartailQuote;
using Portal.Shared.ResponseModels;
using System.Globalization;

namespace Portal.Client.Pages.Quotes;

public partial class QuoteAcceptance
{
    private static readonly CultureInfo QuoteCurrencyCulture = new("en-US");

    [SupplyParameterFromQuery(Name = "token")]
    [Parameter]
    public string? Token { get; set; }

    private QuotePartialDetailsDto? _quoteDetails;
    private QuoteStatusEnum? _chosenStatus;
    private bool _isLoading = true;
    private bool _isSubmitting;
    private string? _loadError;
    private string _pageQuoteRef = "PRS Portal";

    private ClientQuoteSubmissionDto _model = new();

    private bool CanShowResponseActions =>
        _quoteDetails is not null &&
        _quoteDetails.QuoteStatus is QuoteStatusEnum.Sent or QuoteStatusEnum.ClientReview;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        _isLoading = true;
        _quoteDetails = null;
        _loadError = null;
        _pageQuoteRef = "PRS Portal";

        if (string.IsNullOrWhiteSpace(Token))
        {
            _loadError = "Invalid link. No fee proposal token was supplied.";
            _snackbar.Add(_loadError, Severity.Error);
            _isLoading = false;
            return;
        }

        string token = Token!.Trim();
        Result<QuotePartialDetailsDto> res = await _apiService.GetPartialQuoteDetails(token);
        if (res.IsSuccess)
        {
            _quoteDetails = res.Value;
            _pageQuoteRef = res.Value!.QuoteRef;
        }
        else
        {
            _loadError = res.ErrorDescription ?? "Could not load this fee proposal.";
            _snackbar.Add(_loadError, Severity.Warning);
            StateHasChanged();
        }

        _isLoading = false;
    }

    private async Task SubmitApproveAsync()
    {
        if (_quoteDetails is null || string.IsNullOrWhiteSpace(Token))
            return;

        if (!_model.AddressIsCorrect || !_model.ContactDetailsAreCorrect)
        {
            _snackbar.Add("Please confirm that the address and contact details are correct before accepting.", Severity.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_model.SignedName))
        {
            _snackbar.Add("You must sign your name before you can accept this quote.", Severity.Warning);
            return;
        }

        _model.Status = QuoteStatusEnum.Accepted;
        await SubmitResponseAsync(_model);
    }

    private async Task SubmitRejectAsync()
    {
        if (_quoteDetails is null || string.IsNullOrWhiteSpace(Token))
            return;

        if (string.IsNullOrWhiteSpace(_model.ReasonForRejection))
        {
            _snackbar.Add("Please enter a reason for rejecting this fee proposal.", Severity.Warning);
            return;
        }

        _model.Status = QuoteStatusEnum.Rejected;
        await SubmitResponseAsync(_model);
    }

    private async Task SubmitResponseAsync(ClientQuoteSubmissionDto dto)
    {
        if (string.IsNullOrWhiteSpace(Token))
            return;

        _isSubmitting = true;
        try
        {
            string token = Token.Trim();
            Result<QuotePartialDetailsDto> res = await _apiService.SubmitClientQuote(token, dto);
            if (res.IsSuccess && res.Value is not null)
            {
                _quoteDetails = res.Value;
                _pageQuoteRef = res.Value.QuoteRef;
                string msg = dto.Status is QuoteStatusEnum.Accepted
                    ? "Fee proposal approved. Thank you."
                    : "Fee proposal rejected. Your response has been recorded.";
                _snackbar.Add(msg, Severity.Success);
            }
            else
            {
                _snackbar.Add(res.ErrorDescription ?? "Could not submit your response. Please try again.", Severity.Error);
            }
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void ClearValues()
    {
        _model = new();
        StateHasChanged();
    }
}