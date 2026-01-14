using Microsoft.JSInterop;

namespace Portal.SharedUIComponents;

/// <summary>
/// Example JavaScript interop class demonstrating how to wrap JavaScript functionality
/// in a .NET class for easy consumption. The associated JavaScript module is loaded on demand when first needed.
/// This class can be registered as a scoped DI service and then injected into Blazor components for use.
/// </summary>
public class ExampleJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    /// <summary>
    /// Lazy-loaded task for importing the JavaScript module
    /// </summary>
    private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/Portal.SharedUIComponents/exampleJsInterop.js").AsTask());

    /// <summary>
    /// Shows a prompt dialog using JavaScript interop
    /// </summary>
    /// <param name="message">The message to display in the prompt</param>
    /// <returns>The user's input from the prompt dialog</returns>
    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    /// <summary>
    /// Disposes of the JavaScript module reference asynchronously
    /// </summary>
    /// <returns>A value task representing the asynchronous disposal operation</returns>
    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
