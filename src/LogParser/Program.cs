using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using LogParser.Commands;

namespace LogParser
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var root = new RootCommand("Multi-format log parser CLI with filtering, aggregation, and rich output.")
            {
                new ParseCommand()
            };

            root.AddGlobalOption(new Option<bool>("--version", "Show version information"));

            var parser = new CommandLineBuilder(root)
                .UseDefaults()
                .UseExceptionHandler((ex, ctx) =>
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                    ctx.ExitCode = 1;
                })
                .Build();

            return await parser.InvokeAsync(args);
        }
    }
}
