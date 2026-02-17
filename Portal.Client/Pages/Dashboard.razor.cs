using Microsoft.AspNetCore.Components;
using Portal.Client.Components;
using Portal.Client.Components.JobComponents;

namespace Portal.Client.Pages;

public partial class Dashboard
{
    Dictionary<string, RenderFragment> Components = new() {
            { "UserDetails",
                (builder) => {
                    builder.OpenComponent<UserDetailsComponent>(0);
                    builder.CloseComponent();}
            },
            {
            "SystemSettings",
                (builder) => {
                    builder.OpenComponent<Settings>(0);
                    builder.CloseComponent();
                }
            },
            {
            "UserNotes",
                (builder) => {
                    builder.OpenComponent<UserAssignedNotes>(0);
                    builder.CloseComponent();
                }
            }
        };
}

