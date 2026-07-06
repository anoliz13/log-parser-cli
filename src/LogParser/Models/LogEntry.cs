using System;
using System.Collections.Generic;

namespace LogParser.Models
{
    public class LogEntry
    {
        public DateTimeOffset Timestamp { get; set; }
        public LogLevel Level { get; set; } = LogLevel.Unknown;
        public string Message { get; set; } = string.Empty;
        public string? Source { get; set; }
        public string? Logger { get; set; }
        public string? Exception { get; set; }
        public int LineNumber { get; set; }
        public Dictionary<string, object?> Properties { get; set; } = new();
        public string RawLine { get; set; } = string.Empty;
    }
}
