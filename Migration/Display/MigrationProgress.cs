namespace Migration.Display;

public class MigrationProgress
{
    public MigrationProgress() { }
    public MigrationProgress(string currentStep, string currentItem, int currentIndex, int totalItems)
    {
        CurrentStep = currentStep;
        CurrentItem = currentItem;
        CurrentItemIndex = currentIndex;
        TotalItems = totalItems;
    }

    public string CurrentStep { get; set; } = string.Empty;
    public string CurrentItem { get; set; } = string.Empty;
    public int CurrentItemIndex { get; set; }
    public int TotalItems { get; set; }
    public double Percentage => TotalItems > 0 ? (double)CurrentItemIndex / TotalItems * 100 : 0;
}




