using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using LogParser.Models;
using LogParser.Parsers;
using Xunit;

namespace LogParser.Tests
{
    public class CsvLogParserTests
    {
        [Fact]
        public void SplitCsv_ShouldHandleSimpleFields()
        {
            var parts = CsvLogParser.SplitCsv("a,b,c");
            parts.Should().BeEquivalentTo(new[] { "a", "b", "c" });
        }

        [Fact]
        public void SplitCsv_ShouldHandleQuotedFields()
        {
            var parts = CsvLogParser.SplitCsv("1,\"hello, world\",3");
            parts.Should().BeEquivalentTo(new[] { "1", "hello, world", "3" });
        }

        [Fact]
        public async Task ParseFileAsync_ShouldParseCsvCorrectly()
        {
            var path = Path.GetTempFileName();
            await File.WriteAllLinesAsync(path, new[]
            {
                "timestamp,level,message,source",
                "2024-01-15T10:30:00Z,ERROR,Connection failed,api",
                "2024-01-15T10:31:00Z,INFO,Server started,web"
            });

            var parser = new CsvLogParser();
            var result = await parser.ParseFileAsync(path);

            result.TotalLines.Should().Be(3);
            result.ParsedCount.Should().Be(2);
            result.SkippedCount.Should().Be(1); // header

            result.Entries[0].Level.Should().Be(LogLevel.Error);
            result.Entries[0].Message.Should().Be("Connection failed");
            result.Entries[0].Source.Should().Be("api");

            result.Entries[1].Level.Should().Be(LogLevel.Information);
            result.Entries[1].Message.Should().Be("Server started");
            result.Entries[1].Source.Should().Be("web");

            File.Delete(path);
        }
    }
}
