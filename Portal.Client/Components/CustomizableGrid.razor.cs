using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Portal.Client.Webmodels;

namespace Portal.Client.Components;

public partial class CustomizableGrid
{
    [Parameter]
    public (int, int) GridRowsColumns { get; set; }

    [Parameter]
    public required Dictionary<string, RenderFragment> Components { get; set; }

    /// <summary>
    /// Tracks the history of the grid when moving the components around.
    /// </summary>
    private Stack<GridState> gridHistory = new();
    private bool isEditing = false;
    private GridState? GridState;
    private GridItem? draggedItem;
    private (double, double)? resizeStartPos;

    /// <summary>
    /// Initializes the grid state and default grid items from the configured components.
    /// </summary>
    protected override void OnInitialized()
    {
        GridState = new(GridRowsColumns.Item1, GridRowsColumns.Item2, 120, 8)
        {
            // Initialize grid with some items
            Items =
            [
                new GridItem { X = 0, Y = 0, ItemId = "item1", ColSpan = 5, RowSpan = 3, Content = Components["UserDetails"]},
                new GridItem { X = 5, Y = 0, ItemId = "item2", ColSpan = 5, RowSpan = 3, Content = Components["SystemSettings"]},
                new GridItem { X = 0, Y = 3, ItemId = "item3", ColSpan = 6, RowSpan = 2, Content = Components["UserNotes"]},
                new GridItem { X = 6, Y = 3, ItemId = "item4", ColSpan = 4, RowSpan = 2},
                new GridItem { X = 0, Y = 5, ItemId = "item5", ColSpan = 10, RowSpan = 1},
            ]
        };
    }

    /// <summary>
    /// Begins a drag operation for the specified grid item.
    /// </summary>
    /// <param name="item">The grid item to be dragged. Cannot be null.</param>
    private void HandleDragStart(GridItem item)
    {
        draggedItem = item;
    }

    /// <summary>
    /// Handles a drop event by updating the position of the dragged item based on the drop location.
    /// </summary>
    /// <param name="e">The event data containing information about the drop location and context.</param>
    private void HandleDrop(DragEventArgs e)
    {
        // If we were resizing, don't move the item!
        if (isResizing || draggedItem == null)
        {
            isResizing = false;
            return;
        }

        double oldX = draggedItem.X;
        double oldY = draggedItem.Y;

        draggedItem.X = (int)Math.Round(e.OffsetX / GridState!.CellSizePx);
        draggedItem.Y = (int)Math.Round(e.OffsetY / GridState!.CellSizePx);

        if (!GridState!.IsDragValid(draggedItem))
        {
            draggedItem.X = oldX;
            draggedItem.Y = oldY;
        }

        draggedItem = null;
    }

    private bool isResizing = false;

    /// <summary>
    /// Begins a resize operation for the specified grid item and records the starting pointer position.
    /// </summary>
    /// <param name="e">The drag event args containing client coordinates.</param>
    /// <param name="item">The grid item being resized.</param>
    private void HandleResizeStart(DragEventArgs e, GridItem item)
    {
        isResizing = true; // Flag to prevent 'HandleDrop' from firing for moves
        resizeStartPos = (e.ClientX, e.ClientY);
        draggedItem = item;
    }

    /// <summary>
    /// Updates the grid item's column and row span during a resize drag, keeping the item within grid bounds.
    /// </summary>
    /// <param name="e">The drag event args with current client coordinates.</param>
    private void HandleResize(DragEventArgs e)
    {
        if (resizeStartPos is null || draggedItem is null || e.ClientX == 0)
            return;

        // Calculate how many cells the mouse has moved
        double diffX = e.ClientX - resizeStartPos.Value.Item1;
        double diffY = e.ClientY - resizeStartPos.Value.Item2;

        // Convert pixels to Grid Units
        int colDelta = (int)Math.Round(diffX / (GridState!.CellSizePx * 2));
        int rowDelta = (int)Math.Round(diffY / (GridState!.CellSizePx * 2));

        if (colDelta != 0 || rowDelta != 0)
        {
            draggedItem.ColSpan = Math.Max(1, draggedItem.ColSpan + colDelta);
            draggedItem.RowSpan = Math.Max(1, draggedItem.RowSpan + rowDelta);

            if (!GridState!.IsResizeValid(draggedItem))
            {
                draggedItem.ColSpan = Math.Max(1, draggedItem.ColSpan - colDelta);
                draggedItem.RowSpan = Math.Max(1, draggedItem.RowSpan - rowDelta);
            }

            resizeStartPos = (e.ClientX, e.ClientY);
        }
    }

    /// <summary>
    /// Cleans up state when a resize operation ends.
    /// </summary>
    private void HandleResizeEnd()
    {
        isResizing = false;
        resizeStartPos = null;
        draggedItem = null;
    }
}