using System;

namespace SharedComponents.LINQtoCSV
{
    /// <summary>
    ///     Summary description for FieldFormat
    /// </summary>
    [AttributeUsage(AttributeTargets.Field |
                    AttributeTargets.Property)
    ]
    public class CsvOutputFormatAttribute : Attribute
    {
        #region Constructors

        public CsvOutputFormatAttribute(string format)
        {
            Format = format;
        }

        #endregion Constructors

        #region Properties

        public string Format { get; set; } = "";

        #endregion Properties
    }
}