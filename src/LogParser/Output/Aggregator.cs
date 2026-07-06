using System;
using System.Collections.Generic;
using System.Linq;
using LogParser.Models;

namespace LogParser.Output
{
    public class Aggregator
    {
        public AggregationResult Aggregate(IEnumerable<LogEntry> entries)
        {
            var list = entries.ToList();
            var result = new AggregationResult
            {
                TotalEntries = list.Count
            };

            // Count by level
            foreach (var level in Enum.GetValues<LogLevel>())
                result.CountByLevel[level] = 0;

            foreach (var entry in list)
            {
                if (result.CountByLevel.ContainsKey(entry.Level))
                    result.CountByLevel[entry.Level]++;
                else
                    result.CountByLevel[LogLevel.Unknown]++;
            }

            // Count by source
            foreach (var entry in list)
            {
                var src = entry.Source ?? "(unknown)";
                if (!result.CountBySource.ContainsKey(src))
                    result.CountBySource[src] = 0;
                result.CountBySource[src]++;
            }
            result.UniqueSources = result.CountBySource.Count;

            // Timeline by hour
            var hourly = list
                .Where(e => e.Timestamp != DateTimeOffset.MinValue)
                .GroupBy(e => new DateTimeOffset(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day, e.Timestamp.Hour, 0, 0, e.Timestamp.Offset))
                .OrderBy(g => g.Key)
                .Select(g => (g.Key, g.Key.AddHours(1), g.Count()))
                .ToList();

            result.Timeline = hourly.Select(t => (t.Item1, t.Item2, t.Item3)).ToList();

            // Count by hour bucket
            foreach (var (start, _, count) in hourly)
                result.CountByHour[start.ToString("yyyy-MM-dd HH:mm")] = count;

            // Error list
            result.Errors = list.Where(e => e.Level is LogLevel.Error or LogLevel.Critical).ToList();

            // Time range
            if (list.Any(e => e.Timestamp != DateTimeOffset.MinValue))
            {
                var min = list.Where(e => e.Timestamp != DateTimeOffset.MinValue).Min(e => e.Timestamp);
                var max = list.Where(e => e.Timestamp != DateTimeOffset.MinValue).Max(e => e.Timestamp);
                result.TimeRange = max - min;
            }

            return result;
        }
    }
}
