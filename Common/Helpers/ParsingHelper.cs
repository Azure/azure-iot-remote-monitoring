using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    // Methods, related to parsing.
    public static class ParsingHelper
    {
        #region Public Methods

        #region Static Method: ParseCsv

        /// <summary>
        /// Parses a TextReader's contents as a CSV.
        /// </summary>
        /// <param name="textReader">
        /// The TextReader with contents to parse as a CSV.
        /// </param>
        /// <returns>
        /// The rows of the CSV, parsed from textReader's contents.
        /// </returns>
        public static IEnumerable<string[]> ParseCsv(TextReader textReader)
        {
            Action addItem;
            char currentChar;
            int currentRead;
            bool inQuote;
            Func<string[]> produceRow;
            List<string> rowBuilder;
            StringBuilder stringBuffer;

            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            inQuote = false;
            stringBuffer = new StringBuilder();
            rowBuilder = new List<string>();

            addItem = () =>
            {
                rowBuilder.Add(stringBuffer.ToString());
                stringBuffer.Clear();
            };

            produceRow = () =>
            {
                string[] row;

                addItem();

                row = rowBuilder.ToArray();
                rowBuilder.Clear();

                return row;
            };

            while ((currentRead = textReader.Read()) >= 0)
            {
                currentChar = (char)currentRead;

                if (currentChar == '"')
                {
                    if (textReader.Peek() == (int)'"')
                    {
                        textReader.Read();
                        stringBuffer.Append('"');
                    }
                    else
                    {
                        inQuote = !inQuote;
                    }
                }
                else if (inQuote)
                {
                    stringBuffer.Append(currentChar);
                }
                else
                {
                    switch (currentChar)
                    {
                        case '\r':
                            if (textReader.Peek() == (int)'\n')
                            {
                                textReader.Read();
                            }

                            yield return produceRow();
                            break;

                        case '\n':
                            if (textReader.Peek() == (int)'\r')
                            {
                                textReader.Read();
                            }

                            yield return produceRow();
                            break;

                        case ',':
                            addItem();
                            break;

                        default:
                            stringBuffer.Append(currentChar);
                            break;
                    }
                }
            }

            if (inQuote)
            {
                throw new ArgumentException(
                    "textReader's contents have an unmatched double-quote.",
                    "textReader");
            }

            if ((stringBuffer.Length != 0) ||
                (rowBuilder.Count != 0))
            {
                yield return produceRow();
            }
        }

        #endregion

        #region Static Method: ToDictionaries

        /// <summary>
        /// Expresses a parsed CSV's items as string dictionaries.
        /// </summary>
        /// <param name="parsedCsv">
        /// The parsed CSV's items.
        /// </param>
        /// <returns>
        /// A parsed CSV's items as string dictionaries.
        /// </returns>
        /// <remarks>
        /// The first parsed item's contents will be used as keys for 
        /// subsequent items.
        /// </remarks>
        public static IEnumerable<IDictionary<string, string>> ToDictionaries(
            this IEnumerable<string[]> parsedCsv)
        {
            Dictionary<string, string> currentItem;
            string[] firstRow;
            int i;

            if (parsedCsv == null)
            {
                throw new ArgumentNullException("parsedCsv");
            }

            parsedCsv = parsedCsv.Where(t => t != null);

            firstRow = null;
            foreach (string[] row in parsedCsv)
            {
                if (firstRow == null)
                {
                    if (row.Any(t => object.ReferenceEquals(t, null)))
                    {
                        throw new ArgumentException(
                            "parsedCsv's first non-null item has an index that is a null reference.",
                            "parsedCsv");
                    }

                    firstRow = row;
                }
                else
                {
                    currentItem = new Dictionary<string, string>();

                    for (i = 0;
                        (i < row.Length) &&
                        (i < firstRow.Length);
                        ++i)
                    {
                        currentItem[firstRow[i]] = row[i];
                    }

                    yield return currentItem;
                }
            }

            if (firstRow == null)
            {
                throw new ArgumentException(
                    "parsedCsv has no header row item.",
                    "parsedCsv");
            }
        }

        #endregion

        #region Static Method: ToExpandoObjects

        /// <summary>
        /// Expresses a parsed CSV's items as ExpandoObjects.
        /// </summary>
        /// <param name="parsedCsv">
        /// The parsed CSV's items.
        /// </param>
        /// <returns>
        /// A parsed CSV's items as ExpandoObject.
        /// </returns>
        /// <remarks>
        /// The first parsed item's contents will be used as property names for 
        /// subsequent items.
        /// </remarks>
        public static IEnumerable<ExpandoObject> ToExpandoObjects(
            this IEnumerable<string[]> parsedCsv)
        {
            ExpandoObject currentItem;
            IDictionary<string, object> currentDictionary;
            string[] firstRow;
            int i;

            if (parsedCsv == null)
            {
                throw new ArgumentNullException("parsedCsv");
            }

            parsedCsv = parsedCsv.Where(t => t != null);

            firstRow = null;
            foreach (string[] row in parsedCsv)
            {
                if (firstRow == null)
                {
                    if (row.Any(t => object.ReferenceEquals(t, null)))
                    {
                        throw new ArgumentException(
                            "parsedCsv's first non-null item has an index that is a null reference.",
                            "parsedCsv");
                    }

                    firstRow = row;
                }
                else
                {
                    currentItem = new ExpandoObject();
                    currentDictionary = (IDictionary<string, object>)currentItem;

                    for (i = 0;
                        (i < row.Length) &&
                        (i < firstRow.Length);
                        ++i)
                    {
                        currentDictionary[firstRow[i]] = row[i];
                    }

                    yield return currentItem;
                }
            }

            if (firstRow == null)
            {
                throw new ArgumentException(
                    "parsedCsv has no header row item.",
                    "parsedCsv");
            }
        }

        #endregion

        #endregion
    }
}