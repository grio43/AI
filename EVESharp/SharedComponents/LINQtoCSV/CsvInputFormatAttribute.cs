using System;
using System.Globalization;

namespace SharedComponents.LINQtoCSV
{
    /// <summary>
    ///     Summary description for CsvInputFormat
    /// </summary>
    [AttributeUsage(AttributeTargets.Field |
                    AttributeTargets.Property)
    ]
    public class CsvInputFormatAttribute : Attribute
    {
        #region Constructors

        public CsvInputFormatAttribute(NumberStyles numberStyle)
        {
            NumberStyle = numberStyle;
        }

        #endregion Constructors

        #region Properties

        public NumberStyles NumberStyle { get; set; } = NumberStyles.Any;

        #endregion Properties
    }
}