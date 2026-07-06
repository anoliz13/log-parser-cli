using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LogParser.Models;

namespace LogParser.Output
{
    public class CsvOutputFormatter : IOutputFormatter
    {
        public string FormatName => "csv";

        public Task WriteAsync(IEnumerable<LogEntry> entries, TextWriter writer)
        {
            writer.WriteLine("timestamp,level,source,logger,message,exception");

            foreach (var entry in entries)
            {
                var ts = entry.Timestamp == DateTimeOffset.MinValue
                    ? ""
                    : entry.Timestamp.ToString("O");
                var msg = EscapeCsv(entry.Message);
                var exc = EscapeCsv(entry.Exception ?? "");
                writer.WriteLine($"{ts},{entry.Level},{EscapeCsv(entry.Source)},{EscapeCsv(entry.Logger)},{msg},{exc}");
            }

            return Task.CompletedTask;
        }

        public Task WriteAggregationAsync(AggregationResult aggregation, TextWriter writer)
        {
            writer.WriteLine("metric,value");
            writer.WriteLine($"total_entries,{aggregation.TotalEntries}");
            writer.WriteLine($"time_range_minutes,{aggregation.TimeRange.TotalMinutes:F1}");
            writer.WriteLine($"unique_sources,{aggregation.UniqueSources}");

            foreach (var kv in aggregation.CountByLevel)
                writer.WriteLine($"count_level_{kv.Key},{kv.Value}");

            foreach (var kv in aggregation.CountBySource.OrderByDescending(kv => kv.Value).Take(10))
                writer.WriteLine($"count_source_{EscapeCsv(kv.Key)},{kv.Value}");

            return Task.CompletedTask;
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
