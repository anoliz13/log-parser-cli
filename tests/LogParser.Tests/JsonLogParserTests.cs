using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using LogParser.Models;
using LogParser.Parsers;
using Xunit;

namespace LogParser.Tests
{
    public class JsonLogParserTests
    {
        private readonly JsonLogParser _parser = new();

        [Fact]
        public void CanParse_ShouldReturnTrue_ForJsonObject()
        {
            var result = _parser.CanParse("{\"key\":\"value\"}");
            result.Should().BeTrue();
        }

        [Fact]
        public void CanParse_ShouldReturnFalse_ForPlainText()
        {
            var result = _parser.CanParse("2024-01-01 INFO this is a log");
            result.Should().BeFalse();
        }

        [Fact]
        public void ParseLine_ShouldExtractFields()
        {
            var line = "{\"@timestamp\":\"2024-01-15T10:30:00Z\",\"level\":\"ERROR\",\"message\":\"Connection failed\",\"source\":\"api\",\"exception\":\"TimeoutException\"}";
            var entry = _parser.ParseLine(line, 1);

            entry.Timestamp.Should().Be(DateTimeOffset.Parse("2024-01-15T10:30:00Z"));
            entry.Level.Should().Be(LogLevel.Error);
            entry.Message.Should().Be("Connection failed");
            entry.Source.Should().Be("api");
            entry.Exception.Should().Be("TimeoutException");
            entry.LineNumber.Should().Be(1);
        }

        [Fact]
        public void ParseLine_ShouldHandleDifferentFieldNames()
        {
            var line = "{\"time\":\"2024-06-01T12:00:00Z\",\"Severity\":\"WARNING\",\"msg\":\"Disk space low\"}";
            var entry = _parser.ParseLine(line, 1);

            entry.Timestamp.Should().Be(DateTimeOffset.Parse("2024-06-01T12:00:00Z"));
            entry.Level.Should().Be(LogLevel.Warning);
            entry.Message.Should().Be("Disk space low");
        }

        [Fact]
        public void ParseLine_ShouldStoreUnknownProperties()
        {
            var line = "{\"message\":\"test\",\"level\":\"INFO\",\"user_id\":123,\"env\":\"production\"}";
            var entry = _parser.ParseLine(line, 1);

            entry.Properties.Should().ContainKey("user_id");
            entry.Properties.Should().ContainKey("env");
        }

        [Fact]
        public void ParseLine_ShouldHandleInvalidJson()
        {
            var line = "not json at all";
            var entry = _parser.ParseLine(line, 1);

            entry.Message.Should().Be(line);
            entry.Level.Should().Be(LogLevel.Unknown);
        }

        [Fact]
        public async Task ParseFileAsync_ShouldReturnStats()
        {
            var path = Path.GetTempFileName();
            await File.WriteAllLinesAsync(path, new[]
            {
                "{\"message\":\"line1\",\"level\":\"INFO\"}",
                "{\"message\":\"line2\",\"level\":\"ERROR\"}",
                "invalid",
                "{\"message\":\"line3\",\"level\":\"DEBUG\"}"
            });

            var result = await _parser.ParseFileAsync(path);

            result.TotalLines.Should().Be(4);
            result.ParsedCount.Should().Be(3);
            result.SkippedCount.Should().Be(1);
            result.Errors.Should().HaveCount(1);
            result.FilePath.Should().Be(path);

            File.Delete(path);
        }
    }
}
