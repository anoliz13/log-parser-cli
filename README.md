# LogParser CLI

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![NuGet](https://img.shields.io/nuget/v/LogParser.Cli)
![Build](https://img.shields.io/github/actions/workflow/status/anoliz13/log-parser-cli/ci.yml?branch=main)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Multi-format log parser CLI** — Parse, filter, aggregate, dan export file log langsung dari terminal. Mendukung JSON, plain text, CSV, Log4Net, Serilog, dan NLog. Bisa diinstall sebagai .NET global tool.

---

## Daftar Isi

- [Fitur](#fitur)
- [Arsitektur](#arsitektur)
- [Teknologi](#teknologi)
- [Cara Install](#cara-install)
- [Usage](#usage)
- [Filter](#filter)
- [Output Format](#output-format)
- [Aggregation](#aggregation)
- [Testing](#testing)
- [Struktur Folder](#struktur-folder)
- [Screenshot](#screenshot)
- [Lisensi](#lisensi)

---

## Fitur

| Fitur | Keterangan |
|-------|-----------|
| **Auto-detect Format** | Deteksi format log dari sample lines (JSON, plain text, CSV, Log4Net, Serilog, NLog) |
| **Parse JSON Lines** | Parsing JSON log dengan deteksi otomatis field timestamp, level, message, exception |
| **Parse Plain Text** | Ekstrak timestamp & level dari teks biasa |
| **Parse CSV** | Parsing CSV dengan header detection |
| **Log4Net Support** | Pattern: `2024-01-15 10:30:00,123 INFO [App] - msg` |
| **Serilog Support** | Pattern: `[2024-01-15 10:30:00] INFO msg` |
| **NLog Support** | Pattern: `2024-01-15 10:30:00 \| Logger \| INFO \| msg` |
| **Filter by Level** | `--level Error Critical` |
| **Filter by Date Range** | `--from 2024-01-01 --to 2024-06-01` |
| **Text Search** | `--search "timeout"` — case insensitive |
| **Regex Search** | `--search "^ERROR.*database" --regex` |
| **Aggregation** | Count by level, by source, timeline, error summary |
| **Colored Console** | Output warna berdasarkan level (red=error, yellow=warn, dll) |
| **JSON Output** | Export hasil ke JSON |
| **CSV Output** | Export hasil ke CSV |
| **Large File Support** | Stream line-by-line tanpa loading semua file ke memory |
| **Custom Limit** | `--top 50` batasi jumlah output |
| **Errors Only** | `--errors-only` shortcut untuk filter Error + Critical |

---

## Arsitektur

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLI (System.CommandLine)                  │
│  Program.cs  │  ParseCommand  │  RootCommand                    │
└──────────────────────────────┬──────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Pipeline Engine                            │
│                                                                  │
│  ┌──────────────┐   ┌──────────────┐   ┌──────────────────┐    │
│  │  Parser       │   │  Filter      │   │  Output           │    │
│  │  Factory      │──▶│  Pipeline    │──▶│  Formatter        │    │
│  └──────────────┘   └──────────────┘   └──────────────────┘    │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Parsers                  │  Filters      │  Outputs      │   │
│  │  ┌────────┐ ┌──────────┐  │  ┌───────┐    │  ┌───────┐   │   │
│  │  │ JSON   │ │ Plain    │  │  │Level  │    │  │Console│   │   │
│  │  └────────┘ └──────────┘  │  └───────┘    │  └───────┘   │   │
│  │  ┌────────┐ ┌──────────┐  │  ┌───────┐    │  ┌───────┐   │   │
│  │  │ CSV    │ │ Log4Net  │  │  │Date   │    │  │ JSON  │   │   │
│  │  └────────┘ └──────────┘  │  └───────┘    │  └───────┘   │   │
│  │  ┌────────┐ ┌──────────┐  │  ┌───────┐    │  ┌───────┐   │   │
│  │  │Serilog │ │ NLog     │  │  │Text   │    │  │ CSV   │   │   │
│  │  └────────┘ └──────────┘  │  └───────┘    │  └───────┘   │   │
│  └──────────────────────────┘  └───────┘    └──────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Teknologi

| Teknologi | Versi | Fungsi |
|-----------|-------|--------|
| .NET | 8.0 | Runtime |
| C# | 12 | Language |
| System.CommandLine | 2.0-beta4 | CLI parsing & routing |
| Newtonsoft.Json | 13.0 | JSON serialization |
| Microsoft.Extensions.DependencyInjection | 8.0 | DI Container |

---

## Cara Install

### Prasyarat

| Tools | Versi |
|-------|-------|
| .NET SDK | 8.0+ |

### Install sebagai Global Tool

```bash
dotnet tool install --global LogParser.Cli
```

### Atau Run Lokal

```bash
git clone https://github.com/anoliz13/log-parser-cli.git
cd log-parser-cli

# Build
dotnet build --configuration Release

# Run langsung
dotnet run --project src/LogParser -- parse --help

# Atau pack sebagai tool
dotnet pack src/LogParser/LogParser.csproj --configuration Release
```

---

## Usage

### Basic Parsing

```bash
# Parse JSON log file
logparser parse app.log

# Parse dengan format explicit
logparser parse app.log --format json
logparser parse app.log --format csv
logparser parse app.log --format plaintext
```

### Filtering

```bash
# Filter by level
logparser parse app.log --level Error Critical

# Filter by date range
logparser parse app.log --from 2024-01-01 --to 2024-06-01

# Search text
logparser parse app.log --search "timeout"

# Search with regex
logparser parse app.log --search "^ERROR.*database" --regex

# Errors only (shortcut)
logparser parse app.log --errors-only
```

### Output

```bash
# Colored console (default)
logparser parse app.log

# JSON output
logparser parse app.log --output json

# CSV output
logparser parse app.log --output csv

# Export ke file
logparser parse app.log --outfile results.json --output json
logparser parse app.log --outfile errors.csv --output csv --errors-only
```

### Aggregation

```bash
# Tampilkan aggregation summary
logparser parse app.log --aggregate

# Aggregation + filter
logparser parse app.log --aggregate --level Error Critical

# Aggregation + export JSON
logparser parse app.log --aggregate --output json
```

### Advanced

```bash
# Limit output
logparser parse app.log --top 50

# Kombinasi semua filter
logparser parse app.log \
  --level Error Warning \
  --search "database" \
  --from "2024-06-01T00:00:00Z" \
  --to "2024-07-01T00:00:00Z" \
  --aggregate \
  --output csv \
  --outfile database-errors.csv
```

---

## Filter

| Filter | Flag | Contoh | Deskripsi |
|--------|------|--------|-----------|
| **Level** | `--level` | `--level Error Critical` | Filter berdasarkan log level |
| **Date From** | `--from` | `--from 2024-01-01T00:00:00Z` | Ambil log setelah tanggal ini |
| **Date To** | `--to` | `--to 2024-06-01` | Ambil log sebelum tanggal ini |
| **Text Search** | `--search` | `--search "timeout"` | Cari teks dalam message/exception |
| **Regex** | `--search --regex` | `--search "error.*\d{3}" --regex` | Cari dengan regular expression |
| **Errors Only** | `--errors-only` | `--errors-only` | Shortcut untuk level Error + Critical |
| **Limit** | `--top` | `--top 100` | Batasi jumlah entry yang ditampilkan |

---

## Output Format

| Format | Flag | Deskripsi |
|--------|------|-----------|
| **Console** | `--output console` | Colored, human-readable (default) |
| **JSON** | `--output json` | Machine-readable JSON array |
| **CSV** | `--output csv` | Comma-separated values |

### Console Output Colors

| Level | Color |
|-------|-------|
| TRACE | Gray |
| DEBUG | Dark Gray |
| INFO | White |
| WARNING | Yellow |
| ERROR | Red |
| CRITICAL | Dark Red |

---

## Aggregation

Saat menggunakan flag `--aggregate`, CLI menampilkan:

| Metric | Deskripsi |
|--------|-----------|
| **Total entries** | Jumlah total entry setelah filter |
| **Time range** | Rentang waktu log (menit) |
| **Unique sources** | Jumlah unique source |
| **Count by Level** | Distribusi per level (dengan warna) |
| **Count by Source** | Top 10 source terbanyak |
| **Errors List** | Daftar error & critical (max 20) |

---

## Testing

### Test Coverage

| Test File | Jumlah Test | Cakupan |
|-----------|-------------|---------|
| `JsonLogParserTests.cs` | 6 | CanParse, extract fields, different field names, unknown properties, invalid JSON, file stats |
| `PlainTextLogParserTests.cs` | 7 | Timestamp + level, Log4Net, Serilog, NLog, unknown format, file stats, level mapping |
| `CsvLogParserTests.cs` | 3 | Simple fields, quoted fields, file parsing |
| `FilterTests.cs` | 8 | Level filter, empty filter, date range, text search, exception search, regex, empty search |
| `AggregatorTests.cs` | 4 | Count by level, collect errors, count by source, build timeline |
| `ParserFactoryTests.cs` | 4 | Auto return JSON, specific parser, detect JSON, fallback to plain text |

### Run Tests

```bash
cd tests/LogParser.Tests
dotnet test

# atau dari root
.\build.ps1
```

---

## Struktur Folder

```
log-parser-cli/
├── src/
│   └── LogParser/                       # CLI Application
│       ├── Commands/
│       │   └── ParseCommand.cs          # Command utama: parse, filter, aggregate, output
│       ├── Filters/
│       │   ├── ILogFilter.cs            # Interface filter
│       │   ├── LevelFilter.cs           # Filter by log level
│       │   ├── DateRangeFilter.cs       # Filter by date range
│       │   └── TextFilter.cs            # Filter by text / regex
│       ├── Models/
│       │   ├── LogEntry.cs              # Model log entry
│       │   ├── LogLevel.cs              # Enum (Trace, Debug, Info, Warning, Error, Critical)
│       │   ├── LogFormat.cs             # Enum (Auto, Json, PlainText, Csv, NLog, Serilog, Apache)
│       │   ├── ParseResult.cs           # Hasil parsing dengan stats
│       │   └── AggregationResult.cs     # Hasil agregasi
│       ├── Output/
│       │   ├── IOutputFormatter.cs      # Interface output formatter
│       │   ├── ConsoleFormatter.cs      # Colored console output
│       │   ├── JsonOutputFormatter.cs   # JSON output
│       │   ├── CsvOutputFormatter.cs    # CSV output
│       │   └── Aggregator.cs            # Aggregation engine
│       ├── Parsers/
│       │   ├── ILogParser.cs            # Interface parser
│       │   ├── JsonLogParser.cs         # JSON lines parser
│       │   ├── PlainTextLogParser.cs    # Plain text + Log4Net/Serilog/NLog
│       │   ├── CsvLogParser.cs          # CSV parser
│       │   └── ParserFactory.cs         # Auto-detect format
│       ├── Program.cs                   # Entry point CLI
│       └── LogParser.csproj
│
├── tests/
│   └── LogParser.Tests/
│       ├── JsonLogParserTests.cs
│       ├── PlainTextLogParserTests.cs
│       ├── CsvLogParserTests.cs
│       ├── FilterTests.cs
│       ├── AggregatorTests.cs
│       ├── ParserFactoryTests.cs
│       └── LogParser.Tests.csproj
│
├── Directory.Build.props               # Common metadata
├── LogParser.sln
├── build.ps1                           # Build script (Windows)
├── build.sh                            # Build script (Linux/macOS)
├── LICENSE                             # MIT License
├── .gitignore
└── README.md
```

---

## Screenshot

| CLI Help | Parse Output |
|----------|-------------|
| `logparser --help` | `logparser parse app.log` |
| *(Screenshot akan ditambahkan)* | *(Screenshot akan ditambahkan)* |

### Contoh Output Console

```
2024-01-15 10:30:00.000  [Information]  Server started successfully
2024-01-15 10:30:01.234  [Warning    ]  High memory usage: 85%
2024-01-15 10:30:02.456  [Error      ]  Connection refused to database
2024-01-15 10:30:03.789  [Critical   ]  Out of disk space
```

---

## Environment Variables

| Variable | Default | Deskripsi |
|----------|---------|-----------|
| `DOTNET_ENVIRONMENT` | `Production` | Environment |

*(CLI ini tidak memerlukan environment variables — semua konfigurasi via CLI flags)*

---

## Contoh Real-world

### Monitoring Production Errors

```bash
# Cari semua error hari ini
logparser parse /var/log/app/production.log \
  --level Error Critical \
  --from "$(date -Iseconds -d 'today 00:00:00')" \
  --aggregate
```

### Export ke Excel/CSV

```bash
# Export error ke CSV untuk analisis
logparser parse app.log \
  --errors-only \
  --output csv \
  --outfile errors-$(date +%Y%m%d).csv
```

### Watch Log Real-time

```bash
# Pipe dari tail
tail -f /var/log/app.log | while read line; do
  echo "$line"
done | logparser parse --format plaintext
```

---

## Kontributor

- **anoliz13** — Developer

---

## Lisensi

[MIT](LICENSE)

Hak Cipta © 2026 LogParser Contributors. Semua hak dilindungi.
