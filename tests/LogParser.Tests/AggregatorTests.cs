using System;
using System.Linq;
using FluentAssertions;
using LogParser.Models;
using LogParser.Output;
using Xunit;

namespace LogParser.Tests
{
    public class AggregatorTests
    {
        [Fact]
        public void Aggregate_ShouldCountByLevel()
        {
            var entries = new[]
            {
                new LogEntry { Level = LogLevel.Information, Message = "a" },
                new LogEntry { Level = LogLevel.Information, Message = "b" },
                new LogEntry { Level = LogLevel.Error, Message = "c" },
                new LogEntry { Level = LogLevel.Warning, Message = "d" },
            };

            var agg = new Aggregator().Aggregate(entries);

            agg.TotalEntries.Should().Be(4);
            agg.CountByLevel[LogLevel.Information].Should().Be(2);
            agg.CountByLevel[LogLevel.Error].Should().Be(1);
            agg.CountByLevel[LogLevel.Warning].Should().Be(1);
        }

        [Fact]
        public void Aggregate_ShouldCollectErrors()
        {
            var entries = new[]
            {
                new LogEntry { Level = LogLevel.Error, Message = "err1" },
                new LogEntry { Level = LogLevel.Critical, Message = "crit1" },
                new LogEntry { Level = LogLevel.Information, Message = "info1" },
            };

            var agg = new Aggregator().Aggregate(entries);

            agg.Errors.Should().HaveCount(2);
            agg.Errors.Should().Contain(e => e.Level == LogLevel.Error);
            agg.Errors.Should().Contain(e => e.Level == LogLevel.Critical);
        }

        [Fact]
        public void Aggregate_ShouldCountBySource()
        {
            var entries = new[]
            {
                new LogEntry { Source = "api", Level = LogLevel.Information },
                new LogEntry { Source = "api", Level = LogLevel.Error },
                new LogEntry { Source = "web", Level = LogLevel.Information },
            };

            var agg = new Aggregator().Aggregate(entries);

            agg.CountBySource["api"].Should().Be(2);
            agg.CountBySource["web"].Should().Be(1);
            agg.UniqueSources.Should().Be(2);
        }

        [Fact]
        public void Aggregate_ShouldBuildTimeline()
        {
            var entries = new[]
            {
                new LogEntry { Timestamp = DateTimeOffset.Parse("2024-01-01T10:15:00Z"), Level = LogLevel.Information },
                new LogEntry { Timestamp = DateTimeOffset.Parse("2024-01-01T10:45:00Z"), Level = LogLevel.Information },
                new LogEntry { Timestamp = DateTimeOffset.Parse("2024-01-01T11:00:00Z"), Level = LogLevel.Error },
            };

            var agg = new Aggregator().Aggregate(entries);

            agg.Timeline.Should().HaveCount(2);
            agg.Timeline[0].Count.Should().Be(2);
            agg.Timeline[1].Count.Should().Be(1);
        }
    }
}
