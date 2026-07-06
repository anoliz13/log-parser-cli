using System;
using System.Collections.Generic;

namespace LogParser.Models
{
    public class AggregationResult
    {
        public Dictionary<LogLevel, int> CountByLevel { get; set; } = new();
        public Dictionary<string, int> CountBySource { get; set; } = new();
        public Dictionary<string, int> CountByHour { get; set; } = new();
        public List<(DateTimeOffset Start, DateTimeOffset End, int Count)> Timeline { get; set; } = new();
        public List<LogEntry> Errors { get; set; } = new();
        public int TotalEntries { get; set; }
        public TimeSpan TimeRange { get; set; }
        public int UniqueSources { get; set; }
    }
}
