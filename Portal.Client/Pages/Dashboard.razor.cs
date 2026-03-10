using Portal.Client.Components;
using Portal.Client.Components.Users;
using Portal.Client.Webmodels;

namespace Portal.Client.Pages;

public partial class Dashboard
{
    private readonly Dictionary<string, GridItem> Components = new()
    {
        {
            "UserDetails",
            new GridItem
            {
                ItemId = "UserDetails",
                X = 5,
                Y = 4,
                ColSpan = 5,
                RowSpan = 5,
                Content = builder =>
                {
                    builder.OpenComponent<UserDetailsComponent>(0);
                    builder.CloseComponent();
                }
            }
        },
        {
            "SystemSettings",
            new GridItem
            {
                X = 5,
                Y = 0,
                RowSpan  = 4,
                ColSpan = 5,
                ItemId = "SystemSettings",
                Content = builder =>
                {
                    builder.OpenComponent<Settings>(0);
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
                ColSpan = 5,
                RowSpan = 6,
                Content = builder =>
                {
                    builder.OpenComponent<UserNotes>(0);
                    builder.CloseComponent();
                }
            }
        }
    };
}

