using System.Collections.Generic;
using LogParser.Models;

namespace LogParser.Filters
{
    public interface ILogFilter
    {
        string Name { get; }
        IEnumerable<LogEntry> Apply(IEnumerable<LogEntry> entries);
    }
}
