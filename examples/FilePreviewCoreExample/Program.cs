using Lux.Display;

var parsed = FilePreviewService.ParseCsv("Item;Price\nDesk;299.90\nChair;149.00", "catalog.csv");
Console.WriteLine($"Detected delimiter '{parsed.Delimiter}', {parsed.Rows.Count} rows, Price numeric={parsed.NumericColumns[1]}");
