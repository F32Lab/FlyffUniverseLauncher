namespace FlyffUniverseLauncher.Classes.Csv;

/// <summary>
/// A very small csv table used to read and write the profiles file of the launcher.
/// The format is the same one the previous versions of the launcher used:
/// a header line with the column names, followed by one line per profile,
/// all separated by a semicolon.
/// </summary>
public class CsvTable
{
    private readonly string _separator;

    /// <summary>
    /// Creates an empty table out of the given columns.
    /// </summary>
    /// <param name="columns">The columns of the table.</param>
    public CsvTable(params CsvColumn[] columns)
    {
        _separator = ";";
        ColumnList.AddRange(columns);
    }

    /// <summary>
    /// Creates a table out of the lines of a csv file.
    /// The first line holds the column names, every following line holds one row.
    /// </summary>
    /// <param name="lines">The lines of the file.</param>
    /// <param name="separator">The separator between the values of a line.</param>
    public CsvTable(string[] lines, string separator)
    {
        _separator = separator;

        if (lines.Length == 0)
        {
            return;
        }

        foreach (var columnName in lines[0].Split(separator))
        {
            ColumnList.Add(new CsvColumn(columnName));
        }

        foreach (var line in lines.Skip(1).Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var values = line.Split(separator);

            // Every column gets a value of the row, so the table always stays aligned
            // even when a line has fewer values than there are columns.
            for (int i = 0; i < ColumnList.Count; i++)
            {
                ColumnList[i].AddRow(i < values.Length ? values[i] : string.Empty);
            }
        }
    }

    /// <summary>
    /// Gets the columns of the table.
    /// </summary>
    public List<CsvColumn> ColumnList { get; } = [];

    /// <summary>
    /// Gets the column with the given name.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    public CsvColumn this[string columnName] => ColumnList.First(x => x.Name == columnName);

    /// <summary>
    /// Gets the column at the given index.
    /// </summary>
    /// <param name="columnIndex">The index of the column.</param>
    public CsvColumn this[int columnIndex] => ColumnList[columnIndex];

    /// <summary>
    /// Adds a new, empty column at the end of the table.
    /// </summary>
    /// <param name="columnName">The name of the new column.</param>
    public void AddColumn(string columnName)
    {
        ColumnList.Add(new CsvColumn(columnName));
    }

    /// <summary>
    /// Turns the table back into the lines of a csv file (header line included),
    /// ready to be written to disk with <see cref="File.WriteAllLines(string, IEnumerable{string})"/>.
    /// </summary>
    public List<string> ToList()
    {
        var lines = new List<string>
        {
            string.Join(_separator, ColumnList.Select(x => x.Name)),
        };

        var rowCount = ColumnList.Count == 0 ? 0 : ColumnList.Max(x => x.RowCount);

        for (int i = 0; i < rowCount; i++)
        {
            lines.Add(string.Join(_separator, ColumnList.Select(x => i < x.RowCount ? x.RowList[i] : string.Empty)));
        }

        return lines;
    }
}
