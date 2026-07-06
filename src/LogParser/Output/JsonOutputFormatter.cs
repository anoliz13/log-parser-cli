using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LogParser.Models;
using Newtonsoft.Json;

namespace LogParser.Output
{
    public class JsonOutputFormatter : IOutputFormatter
    {
        public string FormatName => "json";

        private static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        public Task WriteAsync(IEnumerable<LogEntry> entries, TextWriter writer)
        {
            var json = JsonConvert.SerializeObject(entries, Settings);
            writer.WriteLine(json);
            return Task.CompletedTask;
        }

        public Task WriteAggregationAsync(AggregationResult aggregation, TextWriter writer)
        {
            var json = JsonConvert.SerializeObject(aggregation, Settings);
            writer.WriteLine(json);
            return Task.CompletedTask;
        }
    }
}
