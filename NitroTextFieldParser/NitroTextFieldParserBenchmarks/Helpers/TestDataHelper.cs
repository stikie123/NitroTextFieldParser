using NitroTextFieldParserTests.BenchMarks.Models;

namespace NitroTextFieldParserTests.BenchMarks.Helpers;

public static class TestDataHelper
{
  public static MemoryStream GetSampleDataAsStream(int count)
  {
    var data = GetSampleData(count);
    var stream = new MemoryStream();
    using var writer = new StreamWriter(stream, leaveOpen:true);
    writer.WriteLine(SimpleCsvRowPopulated.GetCsvHeader());
    foreach (var row in data)
    {
      writer.WriteLine(row.ToString());
    }
    writer.Flush();
    stream.Position = 0;
    return stream;
  }

  public static List<SimpleCsvRowPopulated> GetSampleData(int count)
  {
    var faker = new Bogus.Faker();
    var result = new List<SimpleCsvRowPopulated>(count);
    for (int i = 0; i < count; i++)
    {
      var dateOfBirth = faker.Date.Past();
      result.Add(new SimpleCsvRowPopulated()
      {
        FirstName = faker.Name.FirstName(),
        LastName = faker.Name.LastName(),
        DateOfBirth = dateOfBirth,
        Age = GetAge(dateOfBirth),
        AddressLine1 = faker.Address.StreetAddress(),
        AddressLine2 = faker.Address.SecondaryAddress(),
        AddressLine3 = faker.Address.City(),
        AddressLine4 = faker.Address.State(),
        PostCode = faker.Address.ZipCode()
      });
    }

    return result;
  }

  public static int GetAge(DateTime dob)
  {
    var today = DateTime.Today;
    var age = today.Year - dob.Year;
    if (dob.Date > today.AddYears(-age))
      age--;
    return age;
  }
}