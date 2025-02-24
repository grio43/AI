namespace SharedComponents.LINQtoCSV
{
    public class DataRowItem
    {
        #region Constructors

        public DataRowItem(string value, int lineNbr)
        {
            Value = value;
            LineNbr = lineNbr;
        }

        #endregion Constructors

        #region Properties

        public int LineNbr { get; }

        public string Value { get; }

        #endregion Properties
    }
}