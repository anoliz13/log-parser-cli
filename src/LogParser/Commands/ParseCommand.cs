using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LogParser.Filters;
using LogParser.Models;
using LogParser.Output;
using LogParser.Parsers;

namespace LogParser.Commands
{
    public class ParseCommand : Command
    {
        public ParseCommand() : base("parse", "Parse and display log files")
        {
            AddArgument(new Argument<FileInfo>("file", "Path to the log file"));
            AddOption(new Option<LogFormat>("--format", () => LogFormat.Auto, "Log file format (auto, json, plaintext, csv)"));
            AddOption(new Option<string[]>("--level", "Filter by log level(s): Trace, Debug, Info, Warn, Error, Critical"));
            AddOption(new Option<string>("--search", "Search text within log messages"));
            AddOption(new Option<bool>("--regex", "Treat --search as a regular expression"));
            AddOption(new Option<DateTimeOffset?>("--from", "Start date (ISO 8601)"));
            AddOption(new Option<DateTimeOffset?>("--to", "End date (ISO 8601)"));
            AddOption(new Option<string>("--output", () => "console", "Output format: console, json, csv"));
            AddOption(new Option<string>("--outfile", "Write output to file instead of stdout"));
            AddOption(new Option<bool>("--aggregate", "Show aggregation summary"));
            AddOption(new Option<int>("--top", () => 1000, "Maximum number of entries to display"));
            AddOption(new Option<bool>("--errors-only", "Show only errors and criticals"));
            AddOption(new Option<bool>("--no-color", "Disable colored output"));

            Handler = CommandHandler.Create<ParseCommandArgs>(ExecuteAsync);
        }

        private async Task ExecuteAsync(ParseCommandArgs args)
        {
            if (!args.File.Exists)
            {
                Console.Error.WriteLine($"File not found: {args.File.FullName}");
                return;
            }

            // Resolve parser
            var jsonParser = new JsonLogParser();
            var plainParser = new PlainTextLogParser();
            var csvParser = new CsvLogParser();
            var factory = new ParserFactory(new ILogParser[] { jsonParser, plainParser, csvParser });

            ILogParser parser;
            if (args.Format != LogFormat.Auto)
            {
                parser = factory.GetParser(args.Format);
            }
            else
            {
                // Sample first 20 lines for auto-detection
                var samples = await File.ReadAllLinesAsync(args.File.FullName);
                parser = factory.DetectParser(samples.Take(20));
            }

            // Parse
            Console.Error.WriteLine($"Parsing {args.File.Name} as {parser.Format}...");
            var result = await parser.ParseFileAsync(args.File.FullName);
            Console.Error.WriteLine($"Parsed {result.ParsedCount} entries ({result.SkippedCount} skipped) in {result.Elapsed.TotalMilliseconds:F0}ms");

            // Apply filters
            var entries = result.Entries.AsEnumerable();

            var filters = new List<ILogFilter>();

            if (args.ErrorsOnly)
            {
                filters.Add(new LevelFilter(LogLevel.Error, LogLevel.Critical));
            }
            else if (args.Level is { Length: > 0 })
            {
                var levels = args.Level.Select(l => PlainTextLogParser.ParseLevelString(l));
                filters.Add(new LevelFilter(levels));
            }

            if (args.From.HasValue || args.To.HasValue)
                filters.Add(new DateRangeFilter(args.From, args.To));

            if (!string.IsNullOrEmpty(args.Search))
                filters.Add(new TextFilter(args.Search, args.Regex));

            foreach (var filter in filters)
                entries = filter.Apply(entries);

            // Take top N
            entries = entries.Take(args.Top);

            // Resolve output
            IOutputFormatter formatter = args.Output?.ToLowerInvariant() switch
            {
                "json" => new JsonOutputFormatter(),
                "csv" => new CsvOutputFormatter(),
                _ => new ConsoleFormatter()
            };

            // Write
            TextWriter writer = Console.Out;
            bool ownsWriter = false;

            if (!string.IsNullOrEmpty(args.Outfile))
            {
                writer = new StreamWriter(args.Outfile);
                ownsWriter = true;
            }

            if (args.Aggregate)
            {
                var aggregator = new Aggregator();
                var aggResult = aggregator.Aggregate(entries);
                await formatter.WriteAggregationAsync(aggResult, writer);
            }
            else
            {
                await formatter.WriteAsync(entries, writer);
            }

            if (ownsWriter)
            {
                writer.Dispose();
                Console.Error.WriteLine($"Output written to {args.Outfile}");
            }
        }
    }

    public class ParseCommandArgs
    {
        public FileInfo File { get; set; } = null!;
        public LogFormat Format { get; set; } = LogFormat.Auto;
        public string[]? Level { get; set; }
        public string? Search { get; set; }
        public bool Regex { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public string? Output { get; set; }
        public string? Outfile { get; set; }
        public bool Aggregate { get; set; }
        public int Top { get; set; } = 1000;
        public bool ErrorsOnly { get; set; }
        public bool NoColor { get; set; }
    }
}
