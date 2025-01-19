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
      Assert.IsNotNull(result);
      Assert.AreEqual(3, result.Length);
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
      Assert.IsNotNull(result);
      Assert.AreEqual(3, result.Length);
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
      Assert.IsNotNull(result);
      Assert.AreEqual(3, result.Length);
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
      Assert.IsNotNull(result);
      Assert.AreEqual(3, result.Length);
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
      Assert.IsNull(result);
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
      Assert.Throws<InvalidOperationException>(() => _parser.ReadFields());
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
      Assert.IsNotNull(result);
      Assert.AreEqual(3, result.Length);
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
      Assert.IsNotNull(result);
      Assert.AreEqual(3, result.Length);
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
      Assert.IsNotNull(result);
      Assert.AreEqual(3, result.Length);
      Assert.AreEqual("Field7, with \"escaped\" quotes", result[0].ToString());
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
      Assert.IsNotNull(result);
      Assert.AreEqual(4, result.Length); // Expect 4 fields including an empty field for the trailing delimiter
      Assert.AreEqual("Field2", result[0].ToString());
      Assert.AreEqual("Field4", result[1].ToString());
      Assert.AreEqual("Field5", result[2].ToString());
      Assert.AreEqual(string.Empty, result[3].ToString()); // Empty field for the trailing delimiter

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
      Assert.IsNotNull(result);
      Assert.AreEqual(4, result.Length); // Expect 4 fields including an empty field for the trailing delimiter
      Assert.AreEqual("Field1", result[0].ToString());
      Assert.AreEqual("Field2", result[1].ToString());
      Assert.AreEqual("Field3", result[2].ToString());
      Assert.AreEqual(string.Empty, result[3].ToString()); // Explicitly check for an empty field

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
      Assert.IsNull(result);
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
      Assert.Throws<InvalidOperationException>(() => _parser.ReadFields());
    }

    #endregion
  }
}