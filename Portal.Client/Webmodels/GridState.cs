using Microsoft.AspNetCore.Components;

namespace Portal.Client.Webmodels;

/// <summary>
/// A grid state.
/// </summary>  
public class GridState(int gridColumns, int gridRows, double cellSizePx, double gridPadding)
{
    /// <summary>
    /// The list of grid items.
    /// </summary>
    public List<GridItem> Items { get; set; } = [];

    /// <summary>
    /// The number of columns in the grid.
    /// </summary>
    public int GridColumns { get; set; } = gridColumns;

    /// <summary>
    /// The number of rows in the grid.
    /// </summary>
    public int GridRows { get; set; } = gridRows;

    /// <summary>
    /// The size of each cell in the grid.
    /// </summary>
    public double CellSizePx { get; set; } = cellSizePx;

    /// <summary>
    /// The padding of the grid.
    /// </summary>
    public double GridPadding { get; set; } = gridPadding;

    /// <summary>
    /// The CSS for the grid.
    /// </summary>
    public string AsCss() => @$"
            grid-template-columns: repeat({GridColumns}, {CellSizePx}px);
            grid-template-rows: repeat({GridRows}, {CellSizePx}px);
            background-size: calc({CellSizePx}px + {GridPadding}px) calc({CellSizePx}px + {GridPadding}px);
            gap: {GridPadding}px;";

    /// <summary>
    /// Whether the resize is valid.
    /// </summary>
    /// <param name="item">The grid item to check.</param>
    /// <returns>True if the resize is valid, false otherwise.</returns>
    public bool IsResizeValid(GridItem item) => item.X + item.ColSpan <= GridColumns && item.Y + item.RowSpan <= GridRows;

    /// <summary>
    /// Whether the drag is valid.
    /// </summary>
    /// <param name="item">The grid item to check.</param>
    /// <returns>True if the drag is valid, false otherwise.</returns>
    public bool IsDragValid(GridItem item) => item.X + item.ColSpan <= GridColumns && item.Y + item.RowSpan <= GridRows;
}

/// <summary>
/// A grid item.
/// </summary>
public class GridItem
{
    /// <summary>
    /// The x-coordinate of the grid item.
    /// </summary>
    public double X { get; set; }
    /// <summary>
    /// The y-coordinate of the grid item.
    /// </summary>
    public double Y { get; set; }
    /// <summary>
    /// The number of columns the grid item spans.
    /// </summary>
    public int ColSpan { get; set; } = 1;
    /// <summary>
    /// The number of rows the grid item spans.
    /// </summary>
    public int RowSpan { get; set; } = 1;
    /// <summary>
    /// The unique identifier for the grid item.
    /// </summary>
    public required string ItemId { get; set; }
    /// <summary>
    /// Whether the grid item is hidden.
    /// </summary>
    public bool IsHidden { get; set; }
    /// <summary>
    /// The content to be displayed in the grid item.
    /// </summary>
    public RenderFragment? Content { get; set; }
    /// <summary>
    /// The CSS for the grid item.
    /// </summary>
    /// <returns>A CSS string for the grid item.</returns>
    public string AsCss() => $"grid-column: {X + 1} / span {ColSpan}; grid-row: {Y + 1} / span {RowSpan};";
}