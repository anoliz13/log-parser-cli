using System;
using System.Collections.Generic;
using System.Linq;
using LogParser.Models;

namespace LogParser.Filters
{
    public class LevelFilter : ILogFilter
    {
        private readonly HashSet<LogLevel> _allowed;

        public string Name => "LevelFilter";

        public LevelFilter(IEnumerable<LogLevel> allowed)
        {
            _allowed = new HashSet<LogLevel>(allowed);
        }

        public LevelFilter(params LogLevel[] allowed)
        {
            _allowed = new HashSet<LogLevel>(allowed);
        }

        public IEnumerable<LogEntry> Apply(IEnumerable<LogEntry> entries)
        {
            if (_allowed.Count == 0)
                return entries;

            return entries.Where(e => _allowed.Contains(e.Level));
        }
    }
}
