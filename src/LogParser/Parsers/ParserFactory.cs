using System;
using System.Collections.Generic;
using System.Linq;
using LogParser.Models;

namespace LogParser.Parsers
{
    public class ParserFactory
    {
        private readonly IEnumerable<ILogParser> _parsers;

        public ParserFactory(IEnumerable<ILogParser> parsers)
        {
            _parsers = parsers;
        }

        public ILogParser GetParser(LogFormat format)
        {
            if (format != LogFormat.Auto)
                return _parsers.First(p => p.Format == format);

            return _parsers.First(p => p.Format == LogFormat.Json);
        }

        public ILogParser DetectParser(IEnumerable<string> sampleLines)
        {
            var counts = new Dictionary<LogFormat, int>();

            foreach (var parser in _parsers)
            {
                counts[parser.Format] = sampleLines.Count(l => parser.CanParse(l));
            }

            var best = counts.OrderByDescending(c => c.Value).First();
            return best.Value > 0
                ? _parsers.First(p => p.Format == best.Key)
                : _parsers.First(p => p.Format == LogFormat.PlainText);
        }
    }
}
