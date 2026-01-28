using Microsoft.AspNetCore.Components.Web;

namespace Portal.Client.Pages;

public partial class Dashboard
{
    private GridState GridState = new(10, 10, 120);

    protected override void OnInitialized()
    {
        // Initialize grid with some items
        GridState.Items =
        [
            new GridItem { X = 0, Y = 0, ItemId = "item1", ColSpan = 3, RowSpan = 2},
            new GridItem { X = 3, Y = 2, ItemId = "item2", ColSpan = 2},
            new GridItem { X = 0, Y = 5, ItemId = "item3" , RowSpan=  6},
        ];
    }

    private GridItem? draggedItem;

    private void HandleDragStart(GridItem item)
    {
        draggedItem = item;
    }

    private void HandleDrop(DragEventArgs e)
    {
        if (draggedItem == null) return;

        // We calculate the new X/Y based on the mouse position relative to the grid
        // 150px (cell) + 16px (1rem gap) = 166px total step
        double step = GridState.CellSizePx;

        // Note: e.OffsetX/Y can be tricky depending on which child element you drop on.
        // For a production app, you might use a JS helper to get precise coordinates,
        // but here is the logic for a basic implementation:
        draggedItem.X = Math.Round(e.ClientX / step);
        draggedItem.Y = Math.Round(e.ClientY / step);

        draggedItem = null; // Reset
    }
}

public class GridState
{
    public GridState(int gridColumns, int gridRows, double cellSizePx)
    {
        this.GridColumns = gridColumns;
        this.GridRows = gridRows;
        this.CellSizePx = cellSizePx;
    }

    public List<GridItem> Items { get; set; } = [];
    public int GridColumns { get; set; }
    public int GridRows { get; set; }
    public double CellSizePx { get; set; }
}

public class GridItem
{
    public double X { get; set; }
    public double Y { get; set; }
    public int ColSpan { get; set; } = 1;
    public int RowSpan { get; set; } = 1;
    public required string ItemId { get; set; }

    public string AsCss() => $"grid-column: {X + 1} / span {ColSpan}; grid-row: {Y + 1} / span {RowSpan};";
}