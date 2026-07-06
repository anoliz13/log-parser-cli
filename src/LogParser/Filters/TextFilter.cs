using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LogParser.Models;

namespace LogParser.Filters
{
    public class TextFilter : ILogFilter
    {
        private readonly string _search;
        private readonly bool _isRegex;
        private readonly bool _caseSensitive;
        private Regex? _regex;

        public string Name => "TextFilter";

        public TextFilter(string search, bool isRegex = false, bool caseSensitive = false)
        {
            _search = search;
            _isRegex = isRegex;
            _caseSensitive = caseSensitive;

            if (_isRegex)
            {
                var opts = _caseSensitive ? RegexOptions.Compiled : RegexOptions.Compiled | RegexOptions.IgnoreCase;
                _regex = new Regex(_search, opts);
            }
        }

        public IEnumerable<LogEntry> Apply(IEnumerable<LogEntry> entries)
        {
            if (string.IsNullOrEmpty(_search))
                return entries;

            return entries.Where(e =>
            {
                var comparison = _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                if (_isRegex && _regex != null)
                    return _regex.IsMatch(e.Message) || _regex.IsMatch(e.RawLine);

                return e.Message.Contains(_search, comparison) ||
                       e.RawLine.Contains(_search, comparison) ||
                       (e.Exception?.Contains(_search, comparison) ?? false);
            });
        }
    }
}
