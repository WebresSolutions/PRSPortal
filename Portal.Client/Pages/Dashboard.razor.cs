using Portal.Client.Components;
using Portal.Client.Components.DashBoard;
using Portal.Client.Components.Users;
using Portal.Client.Webmodels;

namespace Portal.Client.Pages;

public partial class Dashboard
{
    private static int _colspan = 1;
    private static int _rowspan = 1;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _breadCrumbService.SetBreadCrumbItems(
        [
        ]);
    }

    private readonly Dictionary<string, GridItem> Components = new()
    {
        {
            "UserDetails",
            new GridItem
            {
                ItemId = "UserDetails",
                X = 0,
                Y = 1,
                ColSpan = _colspan,
                RowSpan = _rowspan,
                Content = builder =>
                {
                    builder.OpenComponent<UserDetailsComponent>(0);
                    builder.CloseComponent();
                }
            }
        },
        {
            "UserNotes",
            new GridItem
            {
                ItemId = "UserNotes",
                Y = 0,
                X= 0,
                ColSpan = _colspan,
                RowSpan = _rowspan,
                Content = builder =>
                {
                    builder.OpenComponent<UserNotes>(0);
                    builder.CloseComponent();
                }
            }
        },
        {
            "UserJobs",
            new GridItem
            {
                ItemId = "UserJobs",
                Y = 0,
                X= 1,
                ColSpan = _colspan,
                RowSpan = _rowspan,
                Content = builder =>
                {
                    builder.OpenComponent<UserJobs>(0);
                    builder.CloseComponent();
                }
            }
        }
    };
}

