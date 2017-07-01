using System;

namespace ActiveUp.Net.Common
{
    /// <summary>
    /// Parse line with RFC2184 rules (https://tools.ietf.org/html/rfc2184)
    /// </summary>
    internal class Rfc2184ItemParser
    {
        private string _OriginalItem;
        public bool IsSingleLine { get; private set; }
        public bool TextContainsSpecialChar { get; private set; }
        public int Index { get; private set; }
        public string Name { get; private set; }
        public string Language { get; private set; }
        public string CharSet { get; private set; }
        public string Value { get; private set; }

        public Rfc2184ItemParser(string itemLine)
        {
            _OriginalItem = itemLine;
            ParseLine();
        }

        private void ParseLine()
        {
            ExtractNameData(_OriginalItem.Substring(0, _OriginalItem.IndexOf('=')));
            ExtractValueData(_OriginalItem.Substring(_OriginalItem.IndexOf('=') + 1).Replace("\"", "").Trim('\r', '\n'));
        }

        /// <summary>
        /// Extract all information about FieldName.
        /// </summary>
        /// <param name="fieldName">String of fieldName.</param>
        private void ExtractNameData(string fieldName)
        {
            IsSingleLine = true;
            TextContainsSpecialChar = false;
            Index = 0;

            var nameStructure = (fieldName ?? "").ToLower().Trim();

            // If last char is *, then line contain CharSet or Lang.
            if (nameStructure.Length > 0 && nameStructure.Substring(nameStructure.Length-1) == "*")
            {
                TextContainsSpecialChar = true;
                nameStructure = nameStructure.Substring(0, nameStructure.Length - 1);
            }

            var firstAsteriskIndex = nameStructure.IndexOf('*');
            if (firstAsteriskIndex >= 0)
            {
                Name = nameStructure.Substring(0, firstAsteriskIndex).Trim();
                var itemCount = nameStructure.Substring(firstAsteriskIndex);
                if (itemCount.Contains("*"))
                {
                    var newIndex = 0;
                    var actualIndex = itemCount.Replace("*", "");
                    if (Int32.TryParse(actualIndex, out newIndex))
                    {
                        Index = newIndex;
                    }
                }
                IsSingleLine = false;
            }
            else
            {
                Name = nameStructure;
            }
        }

        /// <summary>
        /// Extract all information about FieldValue.
        /// </summary>
        /// <param name="fieldValue">Value string to extract information.</param>
        private void ExtractValueData(string fieldValue)
        {
            if (string.IsNullOrWhiteSpace(fieldValue)) return;
            var valueToProcess = (fieldValue ?? "").Trim();
            if (valueToProcess.Length > 1 && valueToProcess.Substring(valueToProcess.Length - 1, 1) == ";") valueToProcess = valueToProcess.Substring(0, valueToProcess.Length - 1);

            if (!TextContainsSpecialChar || !valueToProcess.Contains("'"))
            {
                Value = valueToProcess;
            }

            var splitedFieldValue = valueToProcess.Split('\'');
            if (splitedFieldValue.Length < 3)
            {
                CharSet = null;
                Language = null;
                Value = (valueToProcess ?? "").Trim();
            }
            else
            {
                CharSet = splitedFieldValue[0];
                Language = splitedFieldValue[1];
                Value = splitedFieldValue[2];
            }
        }
    }
}