namespace NitroTextFieldParserTests.BenchMarks.Models;

public sealed class SimpleCsvRowPopulatedTest
{
  public SimpleCsvRowPopulatedTest()
  {
    
  }

  public SimpleCsvRowPopulatedTest(IList<string> fieldValues)
  {
    if (fieldValues.Count != 9)
      throw new ArgumentException("CSV line contains too many fields.");
    FirstName = fieldValues[0];
    LastName = fieldValues[1];
    DateOfBirth = DateTime.TryParse(fieldValues[2], out var dob) ? dob : null;
    Age = int.TryParse(fieldValues[3], out var age) ? age : null;
    AddressLine1 = fieldValues[4];
    AddressLine2 = fieldValues[5];
    AddressLine3 = fieldValues[6];
    AddressLine4 = fieldValues[7];
    PostCode = fieldValues[8];
  }

  public SimpleCsvRowPopulatedTest(ReadOnlyMemory<char>[] lineAsMemory)
  {
    FromCsv(lineAsMemory);  
  }
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public DateTime? DateOfBirth { get; set; }
  public int? Age { get; set; }
  public string? AddressLine1 { get; set; }
  public string? AddressLine2 { get; set; }
  public string? AddressLine3 { get; set; }
  public string? AddressLine4 { get; set; }
  public string? PostCode { get; set; }

  public override string ToString()
  {
    return $"{FirstName},{LastName},{DateOfBirth?.ToString("yyyy-MM-dd")},{Age},{AddressLine1},{AddressLine2},{AddressLine3},{AddressLine4},{PostCode}";
  }

  public void FromCsv(ReadOnlyMemory<char>[] fields)
  {
    ;
    // var fields = csvLine.Split(',');
    var fieldIndex = 0;
    foreach (var field in fields)
    {
      switch (fieldIndex)
      {
        case 0:
          FirstName = field.ToString();
          break;
        case 1:
          LastName = field.ToString();
          break;
        case 2:
          DateOfBirth = DateTime.TryParse(field.ToString(), out var dob) ? dob : null;
          break;
        case 3:
          Age = int.TryParse(field.ToString(), out var age) ? age : null;
          break;
        case 4:
          AddressLine1 = field.ToString();
          break;
        case 5:
          AddressLine2 = field.ToString();
          break;
        case 6:
          AddressLine3 = field.ToString();
          break;
        case 7:
          AddressLine4 = field.ToString();
          break;
        case 8:
          PostCode = field.ToString();
          break;
        case 9:
          throw new ArgumentException("CSV line contains too many fields.");
      }

      fieldIndex++;
    }
    if(fieldIndex<8)
      throw new ArgumentException("CSV line contains too few fields.");
  }

  public static string GetCsvHeader()
  {
    return "FirstName,LastName,DateOfBirth,Age,AddressLine1,AddressLine2,AddressLine3,AddressLine4,PostCode";
  }
}

