using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LogParser.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LogParser.Parsers
{
    public class JsonLogParser : ILogParser
    {
        public LogFormat Format => LogFormat.Json;

        public bool CanParse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;
            line = line.Trim();
            return line.StartsWith('{') && line.EndsWith('}');
        }

        public LogEntry ParseLine(string line, int lineNumber)
        {
            var entry = new LogEntry { RawLine = line, LineNumber = lineNumber };

            try
            {
                var obj = JObject.Parse(line);

                entry.Timestamp = TryGetDateTime(obj, "@timestamp", "timestamp", "time", "date", "@t");
                entry.Level = ParseLevel(obj);
                entry.Message = TryGetString(obj, "message", "msg", "Message", "@m", "event");
                entry.Source = TryGetString(obj, "source", "Source", "application", "service");
                entry.Logger = TryGetString(obj, "logger", "Logger", "@l", "log_level");
                entry.Exception = TryGetString(obj, "exception", "Exception", "@x", "stack_trace", "error");

                foreach (var prop in obj.Properties())
                {
                    if (!IsKnownField(prop.Name))
                    {
                        entry.Properties[prop.Name] = prop.Value.Type == JTokenType.String
                            ? prop.Value.ToString()
                            : prop.Value.ToObject<object>();
                    }
                }
            }
            catch
            {
                entry.Message = line;
            }

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

        private static LogLevel ParseLevel(JObject obj)
        {
            var raw = TryGetString(obj, "level", "Level", "@l", "log_level", "severity", "Severity");
            return raw?.ToLowerInvariant() switch
            {
                "trace" => LogLevel.Trace,
                "debug" or "verbose" => LogLevel.Debug,
                "info" or "information" or "informational" => LogLevel.Information,
                "warn" or "warning" => LogLevel.Warning,
                "error" => LogLevel.Error,
                "fatal" or "critical" => LogLevel.Critical,
                _ => LogLevel.Unknown
            };
        }

        private static DateTimeOffset TryGetDateTime(JObject obj, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (obj.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var token))
                {
                    if (DateTimeOffset.TryParse(token.ToString(), out var dto))
                        return dto;
                    if (DateTime.TryParse(token.ToString(), out var dt))
                        return dt;
                }
            }
            return DateTimeOffset.MinValue;
        }

        private static string? TryGetString(JObject obj, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (obj.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var token))
                    return token.ToString();
            }
            return null;
        }

        private static bool IsKnownField(string name)
        {
            return name switch
            {
                "@timestamp" or "timestamp" or "time" or "date" or "@t" => true,
                "level" or "Level" or "@l" or "log_level" or "severity" or "Severity" => true,
                "message" or "msg" or "Message" or "@m" or "event" => true,
                "source" or "Source" or "application" or "service" => true,
                "logger" or "Logger" => true,
                "exception" or "Exception" or "@x" or "stack_trace" or "error" => true,
                _ => false
            };
        }
    }
}
