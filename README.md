# LogParser CLI

[![NuGet](https://img.shields.io/nuget/v/LogParser.Cli)](https://www.nuget.org/packages/LogParser.Cli)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Multi-format log parser CLI** — parse, filter, aggregate, and export log files from your terminal. Supports JSON, plain text, CSV, Log4Net, Serilog, NLog, and Apache formats.

## Features

- **Auto-detect** log format from file content
- **Parse** JSON lines, plain text, CSV, structured logs (Log4Net, Serilog, NLog)
- **Filter** by level, date range, text search, regex
- **Aggregate** — count by level/source, timeline, error summary
- **Output** as colored console, JSON, or CSV
- **Large file support** — streams line-by-line
- **Installable as .NET global tool**

## Quick Start

### Installation

```bash
# As a .NET global tool
dotnet tool install --global LogParser.Cli

# Or run locally
dotnet run --project src/LogParser -- parse --help
```

### Usage

```bash
# Parse a JSON log file
logparser parse app.log

# Show only errors and criticals
logparser parse app.log --errors-only

# Filter by level
logparser parse app.log --level Error Critical

# Search for text
logparser parse app.log --search "timeout"

# Search with regex
logparser parse app.log --search "^ERROR.*database" --regex

# Date range filter
logparser parse app.log --from 2024-01-01 --to 2024-06-01

# Show aggregation summary
logparser parse app.log --aggregate

# Output as JSON
logparser parse app.log --output json

# Output to file
logparser parse app.log --outfile results.csv --output csv

# Limit results
logparser parse app.log --top 50

# Explicit format
logparser parse app.log --format csv
```

## Supported Log Formats

| Format | Auto-Detect | Examples |
|--------|-------------|----------|
| **JSON** | ✅ Lines starting with `{` | Serilog JSON, structured logs |
| **Plain Text** | ✅ Fallback | Generic timestamp + level |
| **CSV** | ✅ Has header row | Exported logs |
| **Log4Net** | ✅ Pattern match | `2024-01-15 10:30:00,123 INFO [App] - msg` |
| **Serilog** | ✅ Pattern match | `[2024-01-15 10:30:00] INFO msg` |
| **NLog** | ✅ Pattern match | `2024-01-15 10:30:00 \| Logger \| INFO \| msg` |

## Filters

| Filter | Flag | Example |
|--------|------|---------|
| Level | `--level` | `--level Error Critical` |
| Date From | `--from` | `--from 2024-01-01T00:00:00Z` |
| Date To | `--to` | `--to 2024-06-01` |
| Text Search | `--search` | `--search "timeout"` |
| Regex | `--search --regex` | `--search "error.*\d{3}" --regex` |
| Errors Only | `--errors-only` | `--errors-only` |
| Limit | `--top` | `--top 100` |

## Output Formats

| Format | Flag | Description |
|--------|------|-------------|
| Console | `--output console` | Colored, human-readable (default) |
| JSON | `--output json` | Machine-readable JSON array |
| CSV | `--output csv` | Comma-separated values |

## Examples

```bash
# Find all database errors in the last hour
logparser parse app.log --search "database" --level Error --from "$(date -u -Iseconds -d '1 hour ago')"

# Aggregate error counts by source
logparser parse app.log --aggregate

# Export warnings to CSV for analysis
logparser parse app.log --level Warning --output csv --outfile warnings.csv

# Watch a growing log file
tail -f app.log | while read line; do echo "$line"; done | logparser parse --format plaintext
```

## Contributing

1. Fork the repository.
2. Create a feature branch.
3. Run `./build.sh` or `.\build.ps1` to verify.
4. Submit a PR.

All contributions must include tests and follow the existing code style.

## License

[MIT](LICENSE)
