namespace FlyffUniverseLauncher.Classes.Csv;

/// <summary>
/// A single column of a <see cref="CsvTable"/>: a name and its list of rows.
/// </summary>
public class CsvColumn
{
    public CsvColumn(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets the name of the column, which is written in the header line of the file.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the rows of the column.
    /// </summary>
    public List<string?> RowList { get; } = [];

    /// <summary>
    /// Gets the number of rows of the column.
    /// </summary>
    public int RowCount => RowList.Count;

    /// <summary>
    /// Gets a value indicating whether the column holds no usable data (no rows at all, or only empty ones).
    /// </summary>
    public bool ContainsEmptyRows => RowList.Count == 0 || RowList.All(string.IsNullOrEmpty);

    /// <summary>
    /// Adds a row at the end of the column.
    /// </summary>
    /// <param name="value">The value of the row.</param>
    public void AddRow(string? value)
    {
        RowList.Add(value);
    }
}
