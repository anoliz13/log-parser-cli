using System.Collections.Generic;
using System.Threading.Tasks;
using LogParser.Models;

namespace LogParser.Output
{
    public interface IOutputFormatter
    {
        string FormatName { get; }
        Task WriteAsync(IEnumerable<LogEntry> entries, TextWriter writer);
        Task WriteAggregationAsync(AggregationResult aggregation, TextWriter writer);
    }
}
