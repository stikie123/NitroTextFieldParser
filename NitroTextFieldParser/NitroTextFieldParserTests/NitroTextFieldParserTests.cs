using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroTextFieldParser.Helpers;
using NUnit.Framework;

namespace NitroTextFieldParserTests
{
  [TestFixture]
  public class NitroTextFieldParserTests
  {
    private NitroTextFieldParser.TextFieldParser _parser;

    #region BasicFields Tests

    [Test]
    public async Task ParseQuotedFields_ShouldHandleValidatedFields()
    {
      // Setup
      var sampleData = new MemoryStream(Encoding.UTF8.GetBytes("Field1,2,\"2025-01-09,18:45:00\""));
      _parser = new NitroTextFieldParser.TextFieldParser(sampleData);

      // Arrange
      _parser.SetDelimiters(",");
      _parser.HasFieldsEnclosedInQuotes = true;

      _parser.SetValidatorSchema(
        field => !string.IsNullOrWhiteSpace(field),         // Non-empty string
        field => int.TryParse(field, out _),               // Integer validation
        field => DateTime.TryParse(field, out _)           // Date validation
      );

      // Act
      var result = _parser.ReadFieldsWithValidation();

      // Assert
      Assert.That(result, Is.Not.Null);
      Assert.That(result.Length, Is.EqualTo(3));
      for (int i = 0; i < result.Length; i++)
      {
        TestContext.WriteLine($"Field {i + 1}: {result[i]}");
      }
    }
    [Test]
    public async Task ParseQuotedFields_ShouldHandleFieldSchema()
    {
      // Setup
      var sampleData = new MemoryStream(Encoding.UTF8.GetBytes("XA123,2,\"2025-01-09\""));
      _parser = new NitroTextFieldParser.TextFieldParser(sampleData);

      // Arrange
      _parser.SetDelimiters(",");
      _parser.HasFieldsEnclosedInQuotes = true;

      _parser.SetFieldSchemas(
        new FieldSchema(5, "^XA\\d{3}$"),               // Field 1: Exactly 5 characters, starts with XA, ends with 3 digits
        new FieldSchema(customValidator: field => int.TryParse(field, out _)), // Field 2: Integer validation
        new FieldSchema(pattern: @"^\d{4}-\d{2}-\d{2}$") // Field 3: Date in YYYY-MM-DD format
      );

      // Act
      var result = _parser.ReadFieldsWithSchemaValidation();

      // Assert
      Assert.That(result, Is.Not.Null);
      Assert.That(result.Length, Is.EqualTo(3));
      for (int i = 0; i < result.Length; i++)
      {
        TestContext.WriteLine($"Field {i + 1}: {result[i]}");
      }
    }

    [Test]
    public async Task ParseBasicFields_ShouldHandleBasicDelimitedFields()
    {
      // Setup
      var sampleData = new MemoryStream(Encoding.UTF8.GetBytes("Field1,Field2,Field3"));
      _parser = new NitroTextFieldParser.TextFieldParser(sampleData);

      // Arrange
      _parser.SetDelimiters(",");
      _parser.HasFieldsEnclosedInQuotes = false;

      // Act
      var result = _parser.ReadFields();

      // Assert
      Assert.That(result, Is.Not.Null);
      Assert.That(result.Length, Is.EqualTo(3));
      for (int i = 0; i < result.Length; i++)
      {
        TestContext.WriteLine($"Field {i + 1}: {result[i]}");
      }
    }

    [Test]
    public async Task ParseBasicFields_ShouldHandleBasicMultiDelimitedFields()
    {
      // Setup
      var sampleData = new MemoryStream(Encoding.UTF8.GetBytes("Field1|Field2,Field3"));
      _parser = new NitroTextFieldParser.TextFieldParser(sampleData);

      // Arrange
      _parser.SetDelimiters(",","|");
      _parser.HasFieldsEnclosedInQuotes = false;

      // Act
      var result = _parser.ReadFields();

      // Assert
      Assert.That(result, Is.Not.Null);
      Assert.That(result.Length, Is.EqualTo(3));
      for (int i = 0; i < result.Length; i++)
      {
        TestContext.WriteLine($"Field {i + 1}: {result[i]}");
      }
    }

    [Test]
    public void ParseBasicFields_ShouldHandleEmptyLines()
    {
      // Arrange
      var emptyLineParser = new NitroTextFieldParser.TextFieldParser(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")));
      emptyLineParser.SetDelimiters(",");
      emptyLineParser.HasFieldsEnclosedInQuotes = false;
      // Act
      var result = emptyLineParser.ReadFields();

      // Assert
      Assert.That(result, Is.Null);
    }

    [Test]
    public void ParseBasicFields_ShouldThrowException_WhenDelimitersNotSet()
    {
      // Setup
      var noQuotesData = new MemoryStream(Encoding.UTF8.GetBytes("\"Field1\",\"Field, with, commas\",\"Field3\""));
      _parser = new NitroTextFieldParser.TextFieldParser(noQuotesData);

      // Arrange
      _parser.HasFieldsEnclosedInQuotes = false;

      // Act & Assert
      Assert.Throws<InvalidOperationException>(new Action(() => _parser.ReadFields()));
    }

    #endregion
    #region QuotedFields Tests

    [Test]
    public async Task ParseQuotedFields_ShouldHandleBasicQuotedFields()
    {
      // Setup
      var noQuotesData = new MemoryStream(Encoding.UTF8.GetBytes("\"Field1\",\"Field, with, commas\",\"Field3\""));
      _parser = new NitroTextFieldParser.TextFieldParser(noQuotesData);

      // Arrange
      _parser.SetDelimiters(",");
      _parser.HasFieldsEnclosedInQuotes = true;

      // Act
      var result = _parser.ReadFields();

      // Assert
      Assert.That(result, Is.Not.Null);
      Assert.That(result.Length, Is.EqualTo(3));
      for (int i = 0; i < result.Length; i++)
      {
        TestContext.WriteLine($"Field {i + 1}: {result[i]}");
      }
    }

    [Test]
    public async Task ParseQuotedFields_ShouldHandleBasicMultipleDelimitersQuotedFields()
    {
      // Setup
      var noQuotesData = new MemoryStream(Encoding.UTF8.GetBytes("\"Field1\"|\"Field, with, commas\",\"Field3\""));
      _parser = new NitroTextFieldParser.TextFieldParser(noQuotesData);

      // Arrange
      _parser.SetDelimiters(",", "|");
      _parser.HasFieldsEnclosedInQuotes = true;

      // Act
      var result = _parser.ReadFields();

      // Assert
      Assert.That(result, Is.Not.Null);
      Assert.That(result.Length, Is.EqualTo(3));
      for (int i = 0; i < result.Length; i++)
      {
        TestContext.WriteLine($"Field {i + 1}: {result[i]}");
      }
    }

    [Test]
    public async Task ParseQuotedFields_ShouldHandleEscapedQuotes()
    {
      // Setup
      var noQuotesData = new MemoryStream(Encoding.UTF8.GetBytes("\"Field7, with \"escaped\" quotes\",\"Field8\",\"Field9\""));
      _parser = new NitroTextFieldParser.TextFieldParser(noQuotesData);

      // Arrange
      _parser.SetDelimiters(",");
      _parser.HasFieldsEnclosedInQuotes = true;

      // Act
      var result = _parser.ReadFields();

      // Assert
      Assert.That(result, Is.Not.Null);
      Assert.That(result.Length, Is.EqualTo(3));
      Assert.That(result[0].ToString(), Is.EqualTo("Field7, with \"escaped\" quotes"));
      for (int i = 0; i < result.Length; i++)
      {
        TestContext.WriteLine($"Field {i + 1}: {result[i]}");
      }
    }

    [Test]
    public async Task ParseQuotedFields_ShouldHandleTrailingDelimiters()
    {
      var noQuotesData = new MemoryStream(Encoding.UTF8.GetBytes("\"Field2\",\"Field4\",\"Field5\","));
      _parser = new NitroTextFieldParser.TextFieldParser(noQuotesData);

      // Arrange
      _parser.SetDelimiters(",");
      _parser.HasFieldsEnclosedInQuotes = true;

      // Act
      var result = _parser.ReadFields();

      // Assert
      Assert.That(result, Is.Not.Null);
      Assert.That(result.Length, Is.EqualTo(4)); // Expect 4 fields including an empty field for the trailing delimiter
      Assert.That(result[0].ToString(), Is.EqualTo("Field2"));
      Assert.That(result[1].ToString(), Is.EqualTo("Field4"));
      Assert.That(result[2].ToString(), Is.EqualTo("Field5"));
      Assert.That(result[3].ToString(), Is.EqualTo(string.Empty)); // Empty field for the trailing delimiter

      foreach (var field in result)
      {
        TestContext.WriteLine(field.ToString() == string.Empty ? "<Empty Field>" : field);
      }
    }

    [Test]
    public async Task ParseQuotedFields_ShouldProcessWithoutEnclosedQuotes_WhenQuotedFieldIsTrue()
    {
      // Setup
      var noQuotesData = new MemoryStream(Encoding.UTF8.GetBytes("Field1,Field2,Field3,"));
      _parser = new NitroTextFieldParser.TextFieldParser(noQuotesData);

      // Arrange
      _parser.SetDelimiters(",");
      _parser.HasFieldsEnclosedInQuotes = true; // Quoted fields enabled, but input doesn't have quotes

      // Act
      var result = _parser.ReadFields();

      // Assert
      Assert.That(result, Is.Not.Null);
      Assert.That(result.Length, Is.EqualTo(4)); // Expect 4 fields including an empty field for the trailing delimiter
      Assert.That(result[0].ToString(), Is.EqualTo("Field1"));
      Assert.That(result[1].ToString(), Is.EqualTo("Field2"));
      Assert.That(result[2].ToString(), Is.EqualTo("Field3"));
      Assert.That(result[3].ToString(), Is.EqualTo(string.Empty)); // Explicitly check for an empty field

      // Output results
      foreach (var field in result)
      {
        TestContext.WriteLine(field.ToString());
      }
    }

    [Test]
    public void ParseQuotedFields_ShouldHandleEmptyLines()
    {
      // Arrange
      var emptyLineParser = new NitroTextFieldParser.TextFieldParser(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")));
      emptyLineParser.SetDelimiters(",");
      emptyLineParser.HasFieldsEnclosedInQuotes = true;

      // Act
      var result = emptyLineParser.ReadFields();

      // Assert
      Assert.That(result, Is.Null);
    }

    [Test]
    public void ParseQuotedFields_ShouldThrowException_WhenDelimitersNotSet()
    {
      // Setup
      var noQuotesData = new MemoryStream(Encoding.UTF8.GetBytes("\"Field1\",\"Field, with, commas\",\"Field3\""));
      _parser = new NitroTextFieldParser.TextFieldParser(noQuotesData);

      // Arrange
      _parser.HasFieldsEnclosedInQuotes = true;

      // Act & Assert
      Assert.Throws<InvalidOperationException>(new Action(() => _parser.ReadFields()));
    }

    #endregion

    #region Behaviour lock-in Tests
    // These pin down the parser's current output so that performance work
    // cannot silently change results.

    private static NitroTextFieldParser.TextFieldParser CreateParser(string data, bool quoted, params string[] delimiters)
    {
      var parser = new NitroTextFieldParser.TextFieldParser(new MemoryStream(Encoding.UTF8.GetBytes(data)));
      parser.SetDelimiters(delimiters);
      parser.HasFieldsEnclosedInQuotes = quoted;
      return parser;
    }

    private static string[] AsStrings(ReadOnlyMemory<char>[] fields) => fields.Select(f => f.ToString()).ToArray();

    [TestCase(false)]
    [TestCase(true)]
    public void MultiCharDelimiter_SplitsOnFullDelimiterOnly(bool quoted)
    {
      using var parser = CreateParser("a|b||c||", quoted, "||");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "a|b", "c", "" }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void OverlappingDelimiters_FirstDeclaredDelimiterWins(bool quoted)
    {
      using var parser = CreateParser("a||b", quoted, "|", "||");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "a", "", "b" }));
    }

    [Test]
    public void MixedLengthDelimiters_AreAllRecognised()
    {
      using var parser = CreateParser("a::b;c::d", false, "::", ";");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "a", "b", "c", "d" }));
    }

    [Test]
    public void QuotedField_WithMultiCharDelimiterInside_IsNotSplit()
    {
      using var parser = CreateParser("\"a||b\"||c", true, "||");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "a||b", "c" }));
    }

    [Test]
    public void QuotedField_DoubledQuotes_AreKeptAsIs()
    {
      // Current behaviour: "" inside a quoted field is not collapsed to a single quote.
      using var parser = CreateParser("\"a\"\"b\",c", true, ",");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "a\"\"b", "c" }));
    }

    [Test]
    public void QuotedMode_QuoteInMiddleOfField_ProtectsDelimiter()
    {
      using var parser = CreateParser("ab\"c,d\"e,f", true, ",");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "ab\"c,d\"e", "f" }));
    }

    [Test]
    public void QuotedMode_UnbalancedQuotes_Throws()
    {
      using var parser = CreateParser("\"abc,def", true, ",");
      Assert.Throws<FormatException>(new Action(() => parser.ReadFields()));
    }

    [Test]
    public void UnquotedMode_QuotesAreLeftInPlace()
    {
      using var parser = CreateParser("\"a\",b", false, ",");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "\"a\"", "b" }));
    }

    [Test]
    public void EmptyLine_UnquotedMode_ReturnsSingleEmptyField()
    {
      using var parser = CreateParser("\nx", false, ",");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "" }));
    }

    [Test]
    public void EmptyLine_QuotedMode_ReturnsNoFields()
    {
      using var parser = CreateParser("\nx", true, ",");
      Assert.That(parser.ReadFields(), Is.Empty);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void LineWithoutDelimiter_ReturnsWholeLine(bool quoted)
    {
      using var parser = CreateParser("justonefield", quoted, ",");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "justonefield" }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ConsecutiveDelimiters_ProduceEmptyFields(bool quoted)
    {
      using var parser = CreateParser(",,a,,", quoted, ",");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "", "", "a", "", "" }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MultipleLines_AreReadInOrder(bool quoted)
    {
      using var parser = CreateParser("a,b\r\nc,d\ne,f", quoted, ",");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "a", "b" }));
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "c", "d" }));
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "e", "f" }));
      Assert.That(parser.EndOfData, Is.True);
      Assert.That(parser.ReadFields(), Is.Null);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void LongLine_ManyFields_ParsedCorrectly(bool quoted)
    {
      // Long enough to exercise vectorised search paths.
      var expected = Enumerable.Range(0, 500).Select(i => $"value{i}_{new string('x', i % 37)}").ToArray();
      using var parser = CreateParser(string.Join(",", expected), quoted, ",");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(expected));
    }

    [Test]
    public void LongLine_QuotedFieldsWithDelimiters_ParsedCorrectly()
    {
      var values = Enumerable.Range(0, 200).Select(i => $"v{i}, with, commas {new string('y', i % 29)}").ToArray();
      var line = string.Join(",", values.Select(v => $"\"{v}\""));
      using var parser = CreateParser(line, true, ",");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(values));
    }

    [Test]
    public void ParserIsReusable_AfterChangingDelimiters()
    {
      using var parser = CreateParser("a,b\nc;d", false, ",");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "a", "b" }));
      parser.SetDelimiters(";");
      Assert.That(AsStrings(parser.ReadFields()), Is.EqualTo(new[] { "c", "d" }));
    }

    [Test]
    public void ReturnedFields_AreIndependentBetweenRows()
    {
      using var parser = CreateParser("a,b\nc,d,e", false, ",");
      var first = parser.ReadFields();
      var second = parser.ReadFields();
      Assert.That(AsStrings(first), Is.EqualTo(new[] { "a", "b" }));
      Assert.That(AsStrings(second), Is.EqualTo(new[] { "c", "d", "e" }));
    }

    #endregion
  }
}