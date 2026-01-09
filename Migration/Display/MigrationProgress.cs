namespace Migration.Display;

/// <summary>
/// Represents the current progress of a migration operation
/// Contains information about the current step, item being processed, and overall progress
/// </summary>
public class MigrationProgress
{
    /// <summary>
    /// Initializes a new instance of the MigrationProgress class
    /// </summary>
    public MigrationProgress() { }
    /// <summary>
    /// Initializes a new instance of the MigrationProgress class with specified values
    /// </summary>
    /// <param name="currentStep">The name of the current migration step</param>
    /// <param name="currentItem">Description of the current item being processed</param>
    /// <param name="currentIndex">The index of the current item (1-based)</param>
    /// <param name="totalItems">The total number of items to process</param>
    public MigrationProgress(string currentStep, string currentItem, int currentIndex, int totalItems)
    {
        CurrentStep = currentStep;
        CurrentItem = currentItem;
        CurrentItemIndex = currentIndex;
        TotalItems = totalItems;
    }

    /// <summary>
    /// Gets or sets the name of the current migration step
    /// </summary>
    public string CurrentStep { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the description of the current item being processed
    /// </summary>
    public string CurrentItem { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the index of the current item being processed (1-based)
    /// </summary>
    public int CurrentItemIndex { get; set; }
    /// <summary>
    /// Gets or sets the total number of items to process in the current step
    /// </summary>
    public int TotalItems { get; set; }
    /// <summary>
    /// Gets the percentage of completion for the current step
    /// Calculated as (CurrentItemIndex / TotalItems) * 100
    /// </summary>
    public double Percentage => TotalItems > 0 ? (double)CurrentItemIndex / TotalItems * 100 : 0;
}




