using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharedComponents.LINQtoCSV
{
    /// <summary>
    ///     Based on code found at
    ///     http://knab.ws/blog/index.php?/archives/3-CSV-file-parser-and-writer-in-C-Part-1.html
    ///     and
    ///     http://knab.ws/blog/index.php?/archives/10-CSV-file-parser-and-writer-in-C-Part-2.html
    /// </summary>
    internal class CsvStream
    {
        #region Constructors

        /// ///////////////////////////////////////////////////////////////////////
        /// CsvStream
        public CsvStream(TextReader inStream, TextWriter outStream, char SeparatorChar, bool IgnoreTrailingSeparatorChar)
        {
            m_instream = inStream;
            m_outStream = outStream;
            m_SeparatorChar = SeparatorChar;
            m_IgnoreTrailingSeparatorChar = IgnoreTrailingSeparatorChar;
            m_SpecialChars = ("\"\x0A\x0D" + m_SeparatorChar).ToCharArray();
            m_lineNbr = 1;
        }

        #endregion Constructors

        #region Fields

        private readonly char[] buffer = new char[4096];
        private readonly bool m_IgnoreTrailingSeparatorChar;
        private readonly TextReader m_instream;
        private readonly TextWriter m_outStream;
        private readonly char m_SeparatorChar;
        private readonly char[] m_SpecialChars;
        private bool EOL;

        private bool EOS;
        private int length;

        // Current line number in the file. Only used when reading a file, not when writing a file.
        private int m_lineNbr;

        private int pos;
        private bool previousWasCr;

        #endregion Fields

        #region Methods

        /// ///////////////////////////////////////////////////////////////////////
        /// ReadRow
        /// <summary>
        /// </summary>
        /// <param name="row">
        ///     Contains the values in the current row, in the order in which they
        ///     appear in the file.
        /// </param>
        /// <returns>
        ///     True if a row was returned in parameter "row".
        ///     False if no row returned. In that case, you're at the end of the file.
        /// </returns>
        public bool ReadRow(IDataRow row, List<int> charactersLength = null)
        {
            row.Clear();

            int i = 0;

            while (true)
            {
                // Number of the line where the item starts. Note that an item
                // can span multiple lines.
                int startingLineNbr = m_lineNbr;

                string item = null;

                int? itemLength = charactersLength == null ? (int?)null : charactersLength.Skip(i).First();

                bool moreAvailable = GetNextItem(ref item, itemLength);
                if (charactersLength != null)
                    if (!(charactersLength.Count() > i + 1))
                    {
                        if (moreAvailable)
                            row.Add(new DataRowItem(item, startingLineNbr));

                        if (!EOL)
                        {
                            AdvanceToEndOfLine();
                            moreAvailable = false;
                        }
                    }

                if (!moreAvailable)
                    return row.Count > 0;

                row.Add(new DataRowItem(item, startingLineNbr));

                i++;
            }
        }

        /// ///////////////////////////////////////////////////////////////////////
        /// WriteRow
        public void WriteRow(List<string> row, bool quoteAllFields)
        {
            bool firstItem = true;
            foreach (string item in row)
            {
                if (!firstItem) m_outStream.Write(m_SeparatorChar);

                // If the item is null, don't write anything.
                if (item != null)
                    if (quoteAllFields ||
                        item.IndexOfAny(m_SpecialChars) > -1 ||
                        item.Trim() == "")
                        m_outStream.Write("\"" + item.Replace("\"", "\"\"") + "\"");
                    else
                        m_outStream.Write(item);

                firstItem = false;
            }

            m_outStream.WriteLine("");
        }

        private void AdvanceToEndOfLine()
        {
            while (true)
            {
                char c = GetNextChar(true);

                if (EOS)
                    break;

                if (c == '\x0D') //'\r'
                {
                    m_lineNbr++;
                    previousWasCr = true;

                    // we are at the end of the line, eat newline characters and exit
                    EOL = true;
                    if (c == '\x0D' && GetNextChar(false) == '\x0A')
                        GetNextChar(true);
                    EOL = false;

                    break;
                }
            }
        }

        private char GetNextChar(bool eat)
        {
            if (pos >= length)
            {
                length = m_instream.ReadBlock(buffer, 0, buffer.Length);
                if (length == 0)
                {
                    EOS = true;
                    return '\0';
                }
                pos = 0;
            }
            if (eat)
                return buffer[pos++];
            return buffer[pos];
        }

        private bool GetNextItem(ref string itemString, int? itemLength = null)
        {
            itemString = null;
            if (EOL)
            {
                // previous item was last in line, start new line
                EOL = false;
                return false;
            }

            bool itemFound = false;
            bool quoted = false;
            bool predata = true;
            bool postdata = false;
            StringBuilder item = new StringBuilder();

            int cnt = 0;
            while (true)
            {
                if (itemLength.HasValue && cnt >= itemLength.Value)
                {
                    itemString = item.ToString();
                    return true;
                }

                char c = GetNextChar(true);
                cnt++;

                if (EOS)
                {
                    if (itemFound) itemString = item.ToString();
                    return itemFound;
                }

                // ---------
                // Keep track of line number.
                // Note that line breaks can happen within a quoted field, not just at the
                // end of a record.

                // Don't count 0D0A as two line breaks.
                if (!previousWasCr && c == '\x0A')
                    m_lineNbr++;

                if (c == '\x0D') //'\r'
                {
                    m_lineNbr++;
                    previousWasCr = true;
                }
                else
                {
                    previousWasCr = false;
                }

                // ---------

                if ((postdata || !quoted) && itemLength == null && c == m_SeparatorChar)
                {
                    if (m_IgnoreTrailingSeparatorChar)
                    {
                        char nC = GetNextChar(false);
                        if (nC == '\x0A' || nC == '\x0D')
                            continue;
                    }
                    // end of item, return
                    if (itemFound) itemString = item.ToString();
                    return true;
                }

                if ((predata || postdata || !quoted) && (c == '\x0A' || c == '\x0D'))
                {
                    // we are at the end of the line, eat newline characters and exit
                    EOL = true;
                    if (c == '\x0D' && GetNextChar(false) == '\x0A')
                        GetNextChar(true);

                    if (itemFound) itemString = item.ToString();
                    return true;
                }

                if (predata && c == ' ')
                    // whitespace preceeding data, discard
                    continue;

                if (predata && c == '"')
                {
                    // quoted data is starting
                    quoted = true;
                    predata = false;
                    itemFound = true;
                    continue;
                }

                if (predata)
                {
                    // data is starting without quotes
                    predata = false;
                    item.Append(c);
                    itemFound = true;
                    continue;
                }

                if (c == '"' && quoted)
                {
                    if (GetNextChar(false) == '"')
                        item.Append(GetNextChar(true));
                    else
                        postdata = true;

                    continue;
                }

                // all cases covered, character must be data
                item.Append(c);
            }
        }

        #endregion Methods
    }
}