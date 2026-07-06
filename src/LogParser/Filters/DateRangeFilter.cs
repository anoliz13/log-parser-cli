using System;
using System.Collections.Generic;
using System.Linq;
using LogParser.Models;

namespace LogParser.Filters
{
    public class DateRangeFilter : ILogFilter
    {
        private readonly DateTimeOffset? _from;
        private readonly DateTimeOffset? _to;

        public string Name => "DateRangeFilter";

        public DateRangeFilter(DateTimeOffset? from, DateTimeOffset? to)
        {
            _from = from;
            _to = to;
        }

        public IEnumerable<LogEntry> Apply(IEnumerable<LogEntry> entries)
        {
            if (_from == null && _to == null)
                return entries;

            return entries.Where(e =>
            {
                if (_from.HasValue && e.Timestamp < _from.Value)
                    return false;
                if (_to.HasValue && e.Timestamp > _to.Value)
                    return false;
                return true;
            });
        }
    }
}
