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
    private Stack<GridState> _gridHistory = new();
    private bool _isEditing = false;
    private GridState? _gridState;
    private GridItem? _draggedItem;
    private (double, double)? _resizeStartPos;

    /// <summary>
    /// Initializes the grid state and default grid items from the configured components.
    /// </summary>
    protected override void OnInitialized()
    {
        _gridState = new(GridRowsColumns.Item1, GridRowsColumns.Item2, 120, 8)
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
        _draggedItem = item;
    }

    /// <summary>
    /// Handles a drop event by updating the position of the dragged item based on the drop location.
    /// </summary>
    /// <param name="e">The event data containing information about the drop location and context.</param>
    private void HandleDrop(DragEventArgs e)
    {
        // If we were resizing, don't move the item!
        if (_isResizing || _draggedItem == null)
        {
            _isResizing = false;
            return;
        }

        double oldX = _draggedItem.X;
        double oldY = _draggedItem.Y;

        _draggedItem.X = (int)Math.Round(e.OffsetX / _gridState!.CellSizePx);
        _draggedItem.Y = (int)Math.Round(e.OffsetY / _gridState!.CellSizePx);

        if (!_gridState!.IsDragValid(_draggedItem))
        {
            _draggedItem.X = oldX;
            _draggedItem.Y = oldY;
        }

        _draggedItem = null;
    }

    private bool _isResizing = false;

    /// <summary>
    /// Begins a resize operation for the specified grid item and records the starting pointer position.
    /// </summary>
    /// <param name="e">The drag event args containing client coordinates.</param>
    /// <param name="item">The grid item being resized.</param>
    private void HandleResizeStart(DragEventArgs e, GridItem item)
    {
        _isResizing = true; // Flag to prevent 'HandleDrop' from firing for moves
        _resizeStartPos = (e.ClientX, e.ClientY);
        _draggedItem = item;
    }

    /// <summary>
    /// Updates the grid item's column and row span during a resize drag, keeping the item within grid bounds.
    /// </summary>
    /// <param name="e">The drag event args with current client coordinates.</param>
    private void HandleResize(DragEventArgs e)
    {
        if (_resizeStartPos is null || _draggedItem is null || e.ClientX == 0)
            return;

        // Calculate how many cells the mouse has moved
        double diffX = e.ClientX - _resizeStartPos.Value.Item1;
        double diffY = e.ClientY - _resizeStartPos.Value.Item2;

        // Convert pixels to Grid Units
        int colDelta = (int)Math.Round(diffX / (_gridState!.CellSizePx * 2));
        int rowDelta = (int)Math.Round(diffY / (_gridState!.CellSizePx * 2));

        if (colDelta != 0 || rowDelta != 0)
        {
            _draggedItem.ColSpan = Math.Max(1, _draggedItem.ColSpan + colDelta);
            _draggedItem.RowSpan = Math.Max(1, _draggedItem.RowSpan + rowDelta);

            if (!_gridState!.IsResizeValid(_draggedItem))
            {
                _draggedItem.ColSpan = Math.Max(1, _draggedItem.ColSpan - colDelta);
                _draggedItem.RowSpan = Math.Max(1, _draggedItem.RowSpan - rowDelta);
            }

            _resizeStartPos = (e.ClientX, e.ClientY);
        }
    }

    /// <summary>
    /// Cleans up state when a resize operation ends.
    /// </summary>
    private void HandleResizeEnd()
    {
        _isResizing = false;
        _resizeStartPos = null;
        _draggedItem = null;
    }
}