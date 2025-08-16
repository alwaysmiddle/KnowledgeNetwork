# Knowledge Network Analysis Tester

A console application for testing and validating Knowledge Network analysis services across different programming languages.

## Features

- **Interactive Mode**: Rich console interface with menus and real-time feedback
- **File Analysis**: Analyze single source code files
- **Directory Analysis**: Batch analyze all files in a directory
- **Multiple Output Formats**: Console tables, JSON, and Markdown export
- **Performance Benchmarking**: Built-in performance testing suite
- **CFG Visualization**: ASCII art visualization of control flow graphs
- **Language Detection**: Automatic language detection from file extensions
- **Progress Tracking**: Real-time progress bars for batch operations

## Installation

```bash
cd src/tools/KnowledgeNetwork.AnalysisTester
dotnet restore
dotnet build
```

## Usage

### Interactive Mode (Default)
```bash
dotnet run
```

This opens an interactive menu where you can:
- Test single files
- Test entire directories
- Run benchmarks
- Visualize control flow graphs
- Export results

### Command Line Mode

#### Analyze a Single File
```bash
dotnet run --file "path/to/MyClass.cs"
dotnet run --file "path/to/MyClass.cs" --export json
dotnet run --file "path/to/MyClass.cs" --export markdown --output results.md
```

#### Analyze a Directory
```bash
dotnet run --dir "src/MyProject" --pattern "*.cs"
dotnet run --dir "src" --pattern "*.cs" --export json --output analysis.json
```

#### Run Benchmarks
```bash
dotnet run --benchmark
```

### Options

- `--file, -f`: Analyze a single file
- `--dir, -d`: Analyze all files in a directory
- `--pattern, -p`: File pattern to match (default: *.cs)
- `--export, -e`: Export format: json, markdown, or console (default)
- `--output, -o`: Output file path for exported results
- `--benchmark, -b`: Run performance benchmarks
- `--interactive, -i`: Run in interactive mode (default when no args)

## Supported Languages

Currently supported:
- **C#** (.cs files) - Full analysis with CFG support

Planned support:
- TypeScript (.ts, .tsx)
- JavaScript (.js, .jsx)
- Documents (.md, .txt)

## Test Samples

The `test-samples` directory contains organized sample files for testing:

```
test-samples/
├── csharp/
│   ├── simple/          # Basic C# constructs
│   ├── complex/         # Advanced patterns (async, generics, etc.)
│   └── edge-cases/      # Error conditions and edge cases
└── (future languages)
```

### Sample Files

#### Simple
- `HelloWorld.cs` - Basic class with main method and simple conditions
- `BasicClass.cs` - Properties, constructors, and basic methods

#### Complex  
- `AsyncMethods.cs` - Async/await patterns, generics, exception handling

#### Edge Cases
- `MalformedCode.cs` - Intentionally broken syntax for error testing
- `EmptyFile.cs` - Empty file to test edge case handling

## Output Examples

### Console Output
Rich formatted tables with color coding:
- ✓ Success indicators
- Performance metrics
- Detailed breakdowns by classes, methods, CFG blocks
- ASCII CFG visualizations

### JSON Export
```json
{
  "FilePath": "path/to/file.cs",
  "Success": true,
  "Duration": "00:00:00.0234567",
  "ClassCount": 2,
  "MethodCount": 5,
  "ControlFlowGraphs": [...],
  "AnalysisResult": {...}
}
```

### Markdown Export
Formatted tables and summaries suitable for documentation:
- Summary statistics
- Detailed class/method breakdowns  
- CFG analysis results
- Error reports

## Performance Benchmarking

The built-in benchmark suite tests:
- Simple class analysis
- Complex method patterns
- Large class files
- Nested conditions
- Multiple loop constructs

Metrics tracked:
- Analysis duration
- Memory usage
- CFG complexity
- Block/edge counts

## CFG Visualization

ASCII art representation of control flow graphs:
```
┌─ Block 0 (Entry)
│  var x = 10
│  if (x > 0)
│  → Block 1 (true)
│  → Block 2 (false)
└─

┌─ Block 1 (Block)
│  Console.WriteLine("Positive")
│  → Block 3
└─
```

## Development

### Adding New Languages

1. Create analysis service for the language
2. Add language detection in `TestFileManager.DetectLanguageFromExtension()`
3. Update `AnalysisTestRunner` to handle the new service
4. Add test samples in `test-samples/{language}/`

### Adding New Output Formats

1. Extend `AnalysisResultFormatter` with new format methods
2. Update command line options to include new format
3. Add format handling in `DisplayOrExportResult()`

## Contributing

When adding new features:
1. Update this README
2. Add appropriate test samples
3. Ensure console output is well-formatted
4. Test both interactive and command-line modes
5. Verify export formats work correctly

## Dependencies

- **Spectre.Console**: Rich console formatting and interaction
- **System.CommandLine**: Command-line argument parsing
- **KnowledgeNetwork.Domains.Code**: Core analysis services