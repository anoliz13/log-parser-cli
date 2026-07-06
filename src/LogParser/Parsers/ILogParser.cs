using System.Threading.Tasks;
using LogParser.Models;

namespace LogParser.Parsers
{
    public interface ILogParser
    {
        LogFormat Format { get; }
        bool CanParse(string line);
        LogEntry ParseLine(string line, int lineNumber);
        Task<ParseResult> ParseFileAsync(string filePath);
    }
}
