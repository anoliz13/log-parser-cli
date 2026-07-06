using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LogParser.Models;

namespace LogParser.Parsers
{
    public class CsvLogParser : ILogParser
    {
        private string[]? _headers;

        public LogFormat Format => LogFormat.Csv;

        public bool CanParse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;
            var parts = SplitCsv(line);
            return parts.Length >= 3;
        }

        public LogEntry ParseLine(string line, int lineNumber)
        {
            if (_headers == null)
            {
                _headers = SplitCsv(line);
                return new LogEntry
                {
                    RawLine = line,
                    LineNumber = lineNumber,
                    Message = "[header row]",
                    Timestamp = DateTimeOffset.MinValue,
                    Level = LogLevel.Unknown
                };
            }

            var values = SplitCsv(line);
            var entry = new LogEntry { RawLine = line, LineNumber = lineNumber };

            for (int i = 0; i < _headers.Length && i < values.Length; i++)
            {
                var key = _headers[i].ToLowerInvariant().Trim();
                var val = values[i].Trim();

                switch (key)
                {
                    case "timestamp":
                    case "time":
                    case "date":
                    case "@timestamp":
                        entry.Timestamp = TryParseDate(val);
                        break;
                    case "level":
                    case "log_level":
                    case "severity":
                    case "@l":
                        entry.Level = PlainTextLogParser.ParseLevelString(val);
                        break;
                    case "message":
                    case "msg":
                    case "@m":
                        entry.Message = val;
                        break;
                    case "source":
                    case "application":
                    case "service":
                        entry.Source = val;
                        break;
                    case "logger":
                    case "logger_name":
                        entry.Logger = val;
                        break;
                    case "exception":
                    case "error":
                    case "@x":
                        entry.Exception = val;
                        break;
                    default:
                        entry.Properties[key] = val;
                        break;
                }
            }

            if (string.IsNullOrEmpty(entry.Message))
                entry.Message = line;

            return entry;
        }

        public async Task<ParseResult> ParseFileAsync(string filePath)
        {
            _headers = null;
            var sw = Stopwatch.StartNew();
            var result = new ParseResult { FilePath = filePath };

            using var reader = new StreamReader(filePath);
            bool isFirst = true;

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

                    if (isFirst && _headers != null)
                    {
                        isFirst = false;
                        result.SkippedCount++;
                        continue;
                    }

                    if (isFirst)
                        isFirst = false;

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

        internal static string[] SplitCsv(string line)
        {
            var parts = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            parts.Add(current.ToString());
            return parts.ToArray();
        }

        private static DateTimeOffset TryParseDate(string val)
        {
            if (DateTimeOffset.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
                return dto;
            if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            return DateTimeOffset.MinValue;
        }
    }
}
