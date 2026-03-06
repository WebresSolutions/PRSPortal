using Microsoft.AspNetCore.Components;
using MudBlazor;
using Portal.Shared;
using Portal.Shared.DTO.Contact;

namespace Portal.Client.Components.Contact;

public partial class SubContacts : IDisposable
{
    [Parameter]
    public required IEnumerable<SubContactDto> SubContact { get; set; }

    private IEnumerable<SubContactDto> _deletedContacts { get; set; } = [];
    private IEnumerable<SubContactDto> _activeContacts { get; set; } = [];
    private MudDataGrid<SubContactDto>? _subContactGrid;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        _activeContacts = SubContact.Where(c => !c.Deleted);
        _deletedContacts = SubContact.Where(c => c.Deleted);
    }

    private void ChangeTabs(TabTypeEnum tab)
    {
        SubContact = tab switch
        {
            TabTypeEnum.All => _activeContacts,
            TabTypeEnum.Deleted => _deletedContacts,
            _ => SubContact
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _subContactGrid?.Dispose();
    }
}