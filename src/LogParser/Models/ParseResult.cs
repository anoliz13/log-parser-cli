using System;
using System.Collections.Generic;

namespace LogParser.Models
{
    public class ParseResult
    {
        public List<LogEntry> Entries { get; set; } = new();
        public string FilePath { get; set; } = string.Empty;
        public int TotalLines { get; set; }
        public int ParsedCount { get; set; }
        public int SkippedCount { get; set; }
        public TimeSpan Elapsed { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
