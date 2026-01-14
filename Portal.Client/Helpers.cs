using Microsoft.JSInterop;
using Portal.Shared;

namespace Portal.Client;

/// <summary>
/// Utility class providing helper methods for client-side operations
/// Contains methods for UI styling and JavaScript interop
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Gets the CSS color variable for a job type
    /// </summary>
    /// <param name="type">The job type enum value</param>
    /// <returns>A CSS variable name for the job type color</returns>
    public static string GetJobColor(JobTypeEnum type)
        => type == JobTypeEnum.Construction ? "var(--colour-construction)" : "var(--colour-survey)";

    /// <summary>
    /// Sets a CSS color variable using JavaScript interop
    /// Updates either the primary or secondary color variable in the document
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime instance for interop calls</param>
    /// <param name="colour">The color value to set (e.g., "#FF0000" or "rgb(255,0,0)")</param>
    /// <param name="primary">True to set the primary color variable, false for secondary</param>
    public static async Task SetColour(IJSRuntime jsRuntime, string colour, bool primary)
    {
        if (primary)
            await jsRuntime.InvokeVoidAsync("updateCssVariable", "--color-primary", colour);
        else
            await jsRuntime.InvokeVoidAsync("updateCssVariable", "--color-secondary", colour);
    }

    /// <summary>
    /// Gets the current value of a CSS variable using JavaScript interop
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime instance for interop calls</param>
    /// <param name="variableName">The name of the CSS variable to retrieve (e.g., "--color-primary")</param>
    /// <returns>The current value of the CSS variable</returns>
    public static async Task<string> GetColour(IJSRuntime jsRuntime, string variableName)
    {
        string css = await jsRuntime.InvokeAsync<string>("getCssVariable", variableName);
        return css;
    }
}
