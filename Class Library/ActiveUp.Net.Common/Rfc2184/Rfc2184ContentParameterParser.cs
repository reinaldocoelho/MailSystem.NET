using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace ActiveUp.Net.Common
{
    internal class Rfc2184ContentParameterParser
    {
        private IList<string> _originalParameterList;
        private IList<Rfc2184ItemParser> _multiLineParameterList;
        public NameValueCollection Parameters { get; set; }

        public Rfc2184ContentParameterParser()
        {
            Parameters = new NameValueCollection();
            _originalParameterList = new List<string>();
            _multiLineParameterList = new List<Rfc2184ItemParser>();
        }

        public Rfc2184ContentParameterParser(IList<string> parameters) : this()
        {
            _originalParameterList = parameters;
        }

        /// <summary>
        /// Add new string parameter to process list.
        /// </summary>
        /// <param name="simpleParameter"></param>
        public void Add(string simpleParameter)
        {
            _originalParameterList.Add(simpleParameter);
        }

        /// <summary>
        /// Parse all parameter line strings received.
        /// </summary>
        public void Parse()
        {
            foreach (var param in _originalParameterList)
            {
                var actualItemParser = new Rfc2184ItemParser(param);
                if (actualItemParser.IsSingleLine)
                {
                    AddSingleLineParameter(actualItemParser);
                }
                else
                {
                    AddMultiLineParameter(actualItemParser);
                }
            }
            RecoverMultiLineParameters();
        }

        /// <summary>
        /// Process all identified Multi-line itens to parameter collection.
        /// </summary>
        private void RecoverMultiLineParameters()
        {
            var multilineGroupList = _multiLineParameterList.OrderBy(i => i.Name).ThenBy(i => i.Index).GroupBy(i => i.Name);
            foreach (var item in multilineGroupList)
            {
                var itemName = item.Key;
                var itemValue = new StringBuilder();
                foreach (var paramValue in item)
                {
                    itemValue.Append(paramValue.Value);
                }
                Parameters.Add(itemName, itemValue.ToString());
            }
        }

        /// <summary>
        /// Add identified multiline item to future process.
        /// </summary>
        /// <param name="actualItemParser">Parsed item.</param>
        private void AddMultiLineParameter(Rfc2184ItemParser actualItemParser)
        {
            _multiLineParameterList.Add(actualItemParser);
        }

        /// <summary>
        /// Add singleline item to final parameter list.
        /// </summary>
        /// <param name="actualItemParser"></param>
        private void AddSingleLineParameter(Rfc2184ItemParser actualItemParser)
        {
            if (!Parameters.AllKeys.ToList().Exists(p => p == actualItemParser.Name))
            {
                Parameters.Add(actualItemParser.Name, actualItemParser.Value);
            }
        }
    }
}