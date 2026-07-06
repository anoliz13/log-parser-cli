using System;
using System.Linq;
using FluentAssertions;
using LogParser.Filters;
using LogParser.Models;
using Xunit;

namespace LogParser.Tests
{
    public class FilterTests
    {
        private readonly LogEntry[] _entries =
        {
            new() { Timestamp = DateTimeOffset.Parse("2024-01-01T10:00:00Z"), Level = LogLevel.Information, Message = "Server started" },
            new() { Timestamp = DateTimeOffset.Parse("2024-01-01T10:05:00Z"), Level = LogLevel.Warning, Message = "High memory usage" },
            new() { Timestamp = DateTimeOffset.Parse("2024-01-01T11:00:00Z"), Level = LogLevel.Error, Message = "Connection refused" },
            new() { Timestamp = DateTimeOffset.Parse("2024-01-02T10:00:00Z"), Level = LogLevel.Critical, Message = "Out of disk space" },
        };

        [Fact]
        public void LevelFilter_ShouldFilterByLevel()
        {
            var filter = new LevelFilter(LogLevel.Error, LogLevel.Critical);
            var result = filter.Apply(_entries);

            result.Should().HaveCount(2);
            result.All(e => e.Level is LogLevel.Error or LogLevel.Critical).Should().BeTrue();
        }

        [Fact]
        public void LevelFilter_ShouldReturnAll_WhenEmpty()
        {
            var filter = new LevelFilter(Array.Empty<LogLevel>());
            var result = filter.Apply(_entries);

            result.Should().HaveCount(4);
        }

        [Fact]
        public void DateRangeFilter_ShouldFilterByDate()
        {
            var from = DateTimeOffset.Parse("2024-01-01T11:00:00Z");
            var filter = new DateRangeFilter(from, null);
            var result = filter.Apply(_entries);

            result.Should().HaveCount(2);
            result.All(e => e.Timestamp >= from).Should().BeTrue();
        }

        [Fact]
        public void DateRangeFilter_ShouldReturnAll_WhenNoRange()
        {
            var filter = new DateRangeFilter(null, null);
            var result = filter.Apply(_entries);

            result.Should().HaveCount(4);
        }

        [Fact]
        public void TextFilter_ShouldSearchInMessage()
        {
            var filter = new TextFilter("memory");
            var result = filter.Apply(_entries);

            result.Should().ContainSingle(e => e.Message == "High memory usage");
        }

        [Fact]
        public void TextFilter_ShouldSearchInException()
        {
            var entries = new[]
            {
                new LogEntry { Message = "test", Exception = "NullReferenceException" }
            };

            var filter = new TextFilter("NullReference");
            var result = filter.Apply(entries);

            result.Should().ContainSingle();
        }

        [Fact]
        public void TextFilter_ShouldSupportRegex()
        {
            var filter = new TextFilter("^Server", isRegex: true);
            var result = filter.Apply(_entries);

            result.Should().ContainSingle(e => e.Message == "Server started");
        }

        [Fact]
        public void TextFilter_ShouldReturnAll_WhenSearchEmpty()
        {
            var filter = new TextFilter("");
            var result = filter.Apply(_entries);

            result.Should().HaveCount(4);
        }
    }
}
