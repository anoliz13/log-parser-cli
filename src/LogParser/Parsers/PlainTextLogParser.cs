using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LogParser.Models;

namespace LogParser.Parsers
{
    public class PlainTextLogParser : ILogParser
    {
        private static readonly Regex TimestampPattern = new(
            @"\d{4}[-/]\d{2}[-/]\d{2}[T ]\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:?\d{2})?",
            RegexOptions.Compiled);

        private static readonly Regex LevelPattern = new(
            @"\b(TRACE|DEBUG|INFO|WARN(?:ING)?|ERROR|FATAL|CRITICAL)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex Log4NetPattern = new(
            @"^(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2},\d{3})\s+(\w+)\s+(\d+)\s+\[(.+?)\]\s+-\s+(.+)$",
            RegexOptions.Compiled);

        private static readonly Regex SerilogPattern = new(
            @"^\[(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}(?:\.\d+)?)\]\s+(\w+)\s+(.+)$",
            RegexOptions.Compiled);

        private static readonly Regex NLogPattern = new(
            @"^(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}(?:\.\d+)?)\s+\|(.+?)\|\s+(\w+)\s+\|(.+)$",
            RegexOptions.Compiled);

        public LogFormat Format => LogFormat.PlainText;

        public bool CanParse(string line) => !string.IsNullOrWhiteSpace(line);

        public LogEntry ParseLine(string line, int lineNumber)
        {
            var entry = new LogEntry { RawLine = line, LineNumber = lineNumber };

            // Try structured patterns first
            var log4Match = Log4NetPattern.Match(line);
            if (log4Match.Success)
            {
                entry.Timestamp = TryParseDate(log4Match.Groups[1].Value);
                entry.Level = ParseLevelString(log4Match.Groups[2].Value);
                entry.Source = log4Match.Groups[3].Value;
                entry.Logger = log4Match.Groups[4].Value;
                entry.Message = log4Match.Groups[5].Value;
                return entry;
            }

            var serilogMatch = SerilogPattern.Match(line);
            if (serilogMatch.Success)
            {
                entry.Timestamp = TryParseDate(serilogMatch.Groups[1].Value);
                entry.Level = ParseLevelString(serilogMatch.Groups[2].Value);
                entry.Message = serilogMatch.Groups[3].Value;
                return entry;
            }

            var nlogMatch = NLogPattern.Match(line);
            if (nlogMatch.Success)
            {
                entry.Timestamp = TryParseDate(nlogMatch.Groups[1].Value);
                entry.Logger = nlogMatch.Groups[2].Value.Trim();
                entry.Level = ParseLevelString(nlogMatch.Groups[3].Value);
                entry.Message = nlogMatch.Groups[4].Value.Trim();
                return entry;
            }

            // Fallback: extract timestamp and level from the line
            var tsMatch = TimestampPattern.Match(line);
            if (tsMatch.Success)
                entry.Timestamp = TryParseDate(tsMatch.Value);

            var levelMatch = LevelPattern.Match(line);
            if (levelMatch.Success)
                entry.Level = ParseLevelString(levelMatch.Value);

            entry.Message = line;

            return entry;
        }

        public async Task<ParseResult> ParseFileAsync(string filePath)
        {
            var sw = Stopwatch.StartNew();
            var result = new ParseResult { FilePath = filePath };

            using var reader = new StreamReader(filePath);

            while (await reader.ReadLineAsync() is { } line)
            {
                result.TotalLines++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    result.SkippedCount++;
                    continue;
                }

                try
                {
                    var entry = ParseLine(line, result.TotalLines);
                    result.Entries.Add(entry);
                    result.ParsedCount++;
                }
                catch (Exception ex)
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Line {result.TotalLines}: {ex.Message}");
                }
            }

            sw.Stop();
            result.Elapsed = sw.Elapsed;
            return result;
        }

        internal static LogLevel ParseLevelString(string level) => level.ToUpperInvariant() switch
        {
            "TRACE" => LogLevel.Trace,
            "DEBUG" or "VERBOSE" => LogLevel.Debug,
            "INFO" or "INFORMATION" or "INF" => LogLevel.Information,
            "WARN" or "WARNING" or "WRN" => LogLevel.Warning,
            "ERR" or "ERROR" => LogLevel.Error,
            "FATAL" or "CRITICAL" or "CRIT" => LogLevel.Critical,
            _ => LogLevel.Unknown
        };

        internal static DateTimeOffset TryParseDate(string value)
        {
            value = value.Replace(",", ".");
            if (DateTimeOffset.TryParse(value, out var dto))
                return dto;
            if (DateTime.TryParse(value, out var dt))
                return dt;
            return DateTimeOffset.MinValue;
        }
    }
}
