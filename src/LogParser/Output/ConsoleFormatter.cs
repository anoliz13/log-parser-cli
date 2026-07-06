using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LogParser.Models;

namespace LogParser.Output
{
    public class ConsoleFormatter : IOutputFormatter
    {
        public string FormatName => "console";

        public Task WriteAsync(IEnumerable<LogEntry> entries, TextWriter writer)
        {
            foreach (var entry in entries)
            {
                var color = entry.Level switch
                {
                    LogLevel.Trace => ConsoleColor.Gray,
                    LogLevel.Debug => ConsoleColor.DarkGray,
                    LogLevel.Information => ConsoleColor.White,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    LogLevel.Error => ConsoleColor.Red,
                    LogLevel.Critical => ConsoleColor.DarkRed,
                    _ => ConsoleColor.Gray
                };

                var original = Console.ForegroundColor;
                Console.ForegroundColor = color;

                var ts = entry.Timestamp == DateTimeOffset.MinValue
                    ? "".PadRight(20)
                    : entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff").PadRight(24);

                var level = $"[{entry.Level,-11}]";

                writer.WriteLine($"{ts} {level} {entry.Message}");

                if (entry.Exception != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    writer.WriteLine($"  {entry.Exception}");
                }

                Console.ForegroundColor = original;
            }

            return Task.CompletedTask;
        }

        public Task WriteAggregationAsync(AggregationResult aggregation, TextWriter writer)
        {
            writer.WriteLine("═══════════════════════════════════════");
            writer.WriteLine("  AGGREGATION RESULTS");
            writer.WriteLine("═══════════════════════════════════════");
            writer.WriteLine($"  Total entries:  {aggregation.TotalEntries}");
            writer.WriteLine($"  Time range:     {aggregation.TimeRange.TotalMinutes:F1} min");
            writer.WriteLine($"  Unique sources: {aggregation.UniqueSources}");
            writer.WriteLine();

            writer.WriteLine("  ── Count by Level ──");
            foreach (var kv in aggregation.CountByLevel.OrderBy(kv => kv.Key))
            {
                var color = kv.Key switch
                {
                    LogLevel.Error or LogLevel.Critical => ConsoleColor.Red,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    _ => ConsoleColor.Gray
                };
                var original = Console.ForegroundColor;
                Console.ForegroundColor = color;
                writer.WriteLine($"    {kv.Key,-12}: {kv.Value,8}");
                Console.ForegroundColor = original;
            }

            writer.WriteLine();
            writer.WriteLine("  ── Count by Source ──");
            foreach (var kv in aggregation.CountBySource.OrderByDescending(kv => kv.Value).Take(10))
                writer.WriteLine($"    {kv.Key,-20}: {kv.Value,8}");

            if (aggregation.Errors.Count > 0)
            {
                writer.WriteLine();
                writer.WriteLine($"  ── Errors ({aggregation.Errors.Count}) ──");
                foreach (var err in aggregation.Errors.Take(20))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    writer.WriteLine($"    {err.Message}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                if (aggregation.Errors.Count > 20)
                    writer.WriteLine($"    ... and {aggregation.Errors.Count - 20} more");
            }

            return Task.CompletedTask;
        }
    }
}
