using System;
using System.Globalization;

namespace SharedComponents.LINQtoCSV
{
    /// <summary>
    ///     Summary description for CsvColumnAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)
    ]
    public class CsvColumnAttribute : Attribute
    {
        #region Fields

        internal const int mc_DefaultFieldIndex = int.MaxValue;

        #endregion Fields

        #region Constructors

        public CsvColumnAttribute()
        {
            Name = "";
            FieldIndex = mc_DefaultFieldIndex;
            CanBeNull = true;
            NumberStyle = NumberStyles.Any;
            OutputFormat = "G";
        }

        public CsvColumnAttribute(
            string name,
            int fieldIndex,
            bool canBeNull,
            string outputFormat,
            NumberStyles numberStyle,
            int charLength)
        {
            Name = name;
            FieldIndex = fieldIndex;
            CanBeNull = canBeNull;
            NumberStyle = numberStyle;
            OutputFormat = outputFormat;

            CharLength = charLength;
        }

        #endregion Constructors

        #region Properties

        public bool CanBeNull { get; set; }
        public int CharLength { get; set; }
        public int FieldIndex { get; set; }
        public string Name { get; set; }
        public NumberStyles NumberStyle { get; set; }
        public string OutputFormat { get; set; }

        #endregion Properties
    }
}