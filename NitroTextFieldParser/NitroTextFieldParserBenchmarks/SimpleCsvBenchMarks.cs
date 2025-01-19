
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using NitroTextFieldParser;
using NitroTextFieldParserTests.BenchMarks.Helpers;
using NitroTextFieldParserTests.BenchMarks.Models;

namespace NitroTextFieldParserTests;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SimpleCsvBenchMarks
{
  private readonly byte[] _csvData;

  public SimpleCsvBenchMarks()
  {
    _csvData = TestDataHelper.GetSampleDataAsStream(1000).ToArray();
  }

  [Benchmark]
  public void OldTextFieldParser()
  {
    using var csvStream = new MemoryStream(_csvData);
    var processedRows = new List<SimpleCsvRowPopulated>();
    csvStream.Position = 0;
    var processLine = (IList<string> fields) => { processedRows.Add(new SimpleCsvRowPopulated(fields)); };
    var rowsProcessed =
      ProcessSimpleCsvActionMemoryOldTextFieldParser(csvStream, processLine, CancellationToken.None, false, false, true,
        ",");

    Console.WriteLine($"Processed {rowsProcessed} rows");
  }

  [Benchmark]
  public void NitroTextFieldParser()
  {
    using var csvStream = new MemoryStream(_csvData);
    var processedRows = new List<SimpleCsvRowPopulatedTest>();
    csvStream.Position = 0;
    var processLine = (ReadOnlyMemory<char>[] fields) => { processedRows.Add(new SimpleCsvRowPopulatedTest(fields)); };
    var rowsProcessed =
      ProcessSimpleCsvActionMemoryNewTextFieldParser(csvStream, processLine, CancellationToken.None, false, false, true,
        ",");

    Console.WriteLine($"Processed {rowsProcessed} rows");
  }
  

  public static int ProcessSimpleCsvActionMemoryOldTextFieldParser(
    Stream inputStream,
    Action<IList<string>> rowProcessed,
    CancellationToken cancellationToken = default,
    bool closeStream = true,
    bool hasTextQualifier = false,
    bool ignoreFirstLine = false,
    params string[] delimiters)
  {
    var rowNumber = 0;
    var recipientsProcessed = 0;
    try
    {
      // using var reader = new StreamReader(inputStream);
      using var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(inputStream);

      parser.SetDelimiters(delimiters); // Accepts multiple delimiters
      parser.HasFieldsEnclosedInQuotes = hasTextQualifier;

      while (!parser.EndOfData)
      {
        rowNumber++;
        var lineValues = parser.ReadFields();
        if (lineValues == null)
          continue;

        if (rowNumber == 1 && ignoreFirstLine)
          continue; // Skip the header row

        if (cancellationToken.IsCancellationRequested)
          break;

        rowProcessed(lineValues);
        recipientsProcessed++;
      }

      return recipientsProcessed;
    }
    catch (Exception e)
    {
      throw new InvalidOperationException($"Csv processing failed:[{rowNumber}].  {e.Message}", e);
    }
    finally
    {
      if (closeStream)
        inputStream?.Close();
    }
  }

  public static int ProcessSimpleCsvActionMemoryNewTextFieldParser(
    Stream inputStream,
    Action<ReadOnlyMemory<char>[]> rowProcessed,
    CancellationToken cancellationToken = default,
    bool closeStream = true,
    bool hasTextQualifier = false,
    bool ignoreFirstLine = false,
    params string[] delimiters)
  {
    var rowNumber = 0;
    var recipientsProcessed = 0;
    try
    {
      // using var reader = new StreamReader(inputStream);
      using var parser = new TextFieldParser(inputStream);

      parser.SetDelimiters(delimiters); // Accepts multiple delimiters
      parser.HasFieldsEnclosedInQuotes = hasTextQualifier;

      while (!parser.EndOfData)
      {
        rowNumber++;
        var lineValues = parser.ReadFields();
        if (lineValues == null)
          continue;

        if (rowNumber == 1 && ignoreFirstLine)
          continue; // Skip the header row

        if (cancellationToken.IsCancellationRequested)
          break;

        rowProcessed(lineValues);
        recipientsProcessed++;
      }

      return recipientsProcessed;
    }
    catch (Exception e)
    {
      throw new InvalidOperationException($"Csv processing failed:[{rowNumber}].  {e.Message}", e);
    }
    finally
    {
      if (closeStream)
        inputStream?.Close();
    }
  }

}

