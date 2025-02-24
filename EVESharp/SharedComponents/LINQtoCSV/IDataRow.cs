namespace SharedComponents.LINQtoCSV
{
    /// <summary>
    ///     Describes a single row. This is made up of a collection of data row items.
    ///     Each data row item is of type DataRowItem, containing the actual value (a string) and its line number (an int)
    ///     within the file.
    /// </summary>
    public interface IDataRow
    {
        #region Properties

        // Number of data row items in the row.
        int Count { get; }

        #endregion Properties

        #region Indexers

        // Allows you to access each data row item with an array index, such as
        // row[i]
        DataRowItem this[int index] { get; set; }

        #endregion Indexers

        #region Methods

        // Add a data row item to the collection.
        void Add(DataRowItem item);

        // Clear the collection of data row items.
        void Clear();

        #endregion Methods
    }
}