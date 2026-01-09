using Microsoft.JSInterop;
using Portal.Shared;

namespace Portal.Client;

public static class Helpers
{
    public static string GetJobColor(JobTypeEnum type)
        => type == JobTypeEnum.Construction ? "var(--colour-construction)" : "var(--colour-survey)";


    public static async Task SetColour(IJSRuntime jsRuntime, string colour, bool primary)
    {
        if (primary)
            await jsRuntime.InvokeVoidAsync("updateCssVariable", "--color-primary", colour);
        else
            await jsRuntime.InvokeVoidAsync("updateCssVariable", "--color-secondary", colour);
    }

    public static async Task<string> GetColour(IJSRuntime jsRuntime, string variableName)
    {
        string css = await jsRuntime.InvokeAsync<string>("getCssVariable", variableName);
        return css;
    }
}
