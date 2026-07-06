using FluentAssertions;
using LogParser.Models;
using LogParser.Parsers;
using Xunit;

namespace LogParser.Tests
{
    public class ParserFactoryTests
    {
        [Fact]
        public void GetParser_ShouldReturnJsonParser_WhenAuto()
        {
            var factory = new ParserFactory(new ILogParser[]
            {
                new JsonLogParser(),
                new PlainTextLogParser()
            });

            var parser = factory.GetParser(LogFormat.Auto);
            parser.Should().BeOfType<JsonLogParser>();
        }

        [Fact]
        public void GetParser_ShouldReturnSpecificParser()
        {
            var factory = new ParserFactory(new ILogParser[]
            {
                new JsonLogParser(),
                new PlainTextLogParser()
            });

            var parser = factory.GetParser(LogFormat.PlainText);
            parser.Should().BeOfType<PlainTextLogParser>();
        }

        [Fact]
        public void DetectParser_ShouldDetectJson()
        {
            var factory = new ParserFactory(new ILogParser[]
            {
                new JsonLogParser(),
                new PlainTextLogParser(),
                new CsvLogParser()
            });

            var samples = new[]
            {
                "{\"message\":\"test\",\"level\":\"INFO\"}",
                "{\"message\":\"test2\",\"level\":\"ERROR\"}"
            };

            var parser = factory.DetectParser(samples);
            parser.Should().BeOfType<JsonLogParser>();
        }

        [Fact]
        public void DetectParser_ShouldFallbackToPlainText_WhenNoMatch()
        {
            var factory = new ParserFactory(new ILogParser[]
            {
                new JsonLogParser(),
                new PlainTextLogParser()
            });

            var samples = new[] { "just a plain line", "another line" };

            var parser = factory.DetectParser(samples);
            parser.Should().BeOfType<PlainTextLogParser>();
        }
    }
}
