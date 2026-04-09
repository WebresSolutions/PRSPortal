using MudBlazor;

namespace Portal.Client.Services.Instances;

public class BreadCrumbService
{
    /// <summary>
    /// Please subscribe to the OnChange event to be notified when the breadcrumb items are updated.
    /// </summary>
    private event Action? OnChange;

    /// <summary>
    /// The list of bread crumb items to be displeyd
    /// </summary>
    private List<BreadcrumbItem> BreadCrumbItems { get; set; } = [];

    /// <summary>
    /// Registers a callback to be invoked when a change occurs.
    /// </summary>
    /// <param name="callback">The method to call when the change event is triggered. Cannot be null.</param>
    public void Subscribe(Action callback) => OnChange += callback;

    /// <summary>
    /// Unsubscribes the specified callback from the OnChange event notifications.
    /// </summary>
    /// <param name="callback">The callback method to remove from the OnChange event. Cannot be null.</param>
    public void Dispose(Action callback) => OnChange -= callback;

    /// <summary>
    /// Returns the collection of breadcrumb items representing the current navigation path.    
    /// </summary>
    /// <returns>A list of <see cref="BreadcrumbItem"/> objects that make up the breadcrumb trail. The list may be empty if no
    /// navigation has occurred.</returns>
    public List<BreadcrumbItem> GetBreadCrumbItems() => BreadCrumbItems;

    /// <summary>
    /// Sets the breadcrumb items
    /// </summary>
    /// <param name="items"></param>
    public void SetBreadCrumbItems(List<BreadcrumbItem> items)
    {
        BreadCrumbItems.Clear();
        BreadCrumbItems.Add(new BreadcrumbItem("Home", "/", icon: Icons.Material.Filled.Home));
        BreadCrumbItems.AddRange(items);
        NotifyStateChanged();
    }

    /// <summary>
    /// Invokes that the state has been changed
    /// </summary>
    private void NotifyStateChanged() => OnChange?.Invoke();
}
