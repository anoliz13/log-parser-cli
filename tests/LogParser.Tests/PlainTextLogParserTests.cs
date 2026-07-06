using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using LogParser.Models;
using LogParser.Parsers;
using Xunit;

namespace LogParser.Tests
{
    public class PlainTextLogParserTests
    {
        private readonly PlainTextLogParser _parser = new();

        [Fact]
        public void ParseLine_ShouldExtractTimestampAndLevel_FromPlainLine()
        {
            var line = "2024-01-15 10:30:00 ERROR Connection failed";
            var entry = _parser.ParseLine(line, 1);

            entry.Timestamp.Should().Be(DateTimeOffset.Parse("2024-01-15T10:30:00"));
            entry.Level.Should().Be(LogLevel.Error);
            entry.Message.Should().Be(line);
        }

        [Fact]
        public void ParseLine_ShouldParseLog4NetFormat()
        {
            var line = "2024-01-15 10:30:00,123 INFO  12345 [MyApp] - User logged in";
            var entry = _parser.ParseLine(line, 1);

            entry.Timestamp.Should().Be(DateTimeOffset.Parse("2024-01-15T10:30:00.123"));
            entry.Level.Should().Be(LogLevel.Information);
            entry.Source.Should().Be("12345");
            entry.Logger.Should().Be("MyApp");
            entry.Message.Should().Be("User logged in");
        }

        [Fact]
        public void ParseLine_ShouldParseSerilogFormat()
        {
            var line = "[2024-06-01 12:00:00.000] WARN Disk space low";
            var entry = _parser.ParseLine(line, 1);

            entry.Timestamp.Should().Be(DateTimeOffset.Parse("2024-06-01T12:00:00.000"));
            entry.Level.Should().Be(LogLevel.Warning);
            entry.Message.Should().Be("Disk space low");
        }

        [Fact]
        public void ParseLine_ShouldParseNLogFormat()
        {
            var line = "2024-01-15 10:30:00.000 | MyLogger | ERROR | Something broke";
            var entry = _parser.ParseLine(line, 1);

            entry.Timestamp.Should().Be(DateTimeOffset.Parse("2024-01-15T10:30:00.000"));
            entry.Logger.Should().Be("MyLogger");
            entry.Level.Should().Be(LogLevel.Error);
            entry.Message.Should().Be("Something broke");
        }

        [Fact]
        public void ParseLine_ShouldHandleUnknownFormat()
        {
            var line = "just a plain log line without timestamp";
            var entry = _parser.ParseLine(line, 1);

            entry.Level.Should().Be(LogLevel.Unknown);
            entry.Message.Should().Be(line);
        }

        [Fact]
        public async Task ParseFileAsync_ShouldReturnCorrectStats()
        {
            var path = Path.GetTempFileName();
            await File.WriteAllLinesAsync(path, new[]
            {
                "2024-01-15 10:30:00 INFO Starting app",
                "2024-01-15 10:30:01 ERROR Failed to connect",
                "",
                "2024-01-15 10:30:02 WARN Retrying"
            });

            var result = await _parser.ParseFileAsync(path);

            result.TotalLines.Should().Be(4);
            result.ParsedCount.Should().Be(3);
            result.SkippedCount.Should().Be(1);

            File.Delete(path);
        }

        [Fact]
        public void ParseLevelString_ShouldMapCorrectly()
        {
            PlainTextLogParser.ParseLevelString("TRACE").Should().Be(LogLevel.Trace);
            PlainTextLogParser.ParseLevelString("DEBUG").Should().Be(LogLevel.Debug);
            PlainTextLogParser.ParseLevelString("INFO").Should().Be(LogLevel.Information);
            PlainTextLogParser.ParseLevelString("WARN").Should().Be(LogLevel.Warning);
            PlainTextLogParser.ParseLevelString("ERROR").Should().Be(LogLevel.Error);
            PlainTextLogParser.ParseLevelString("FATAL").Should().Be(LogLevel.Critical);
            PlainTextLogParser.ParseLevelString("UNKNOWN").Should().Be(LogLevel.Unknown);
        }
    }
}
