using NitroTextFieldParser.Helpers;
using System;
using System.Buffers;
using System.Text;

namespace NitroTextFieldParser
{
  /// <summary>
  /// A reimplementation of a TextFieldParser for parsing delimited or fixed-width fields.
  /// </summary>
  public class TextFieldParser : IDisposable
  {
    private StreamReader _reader;
    private readonly bool _leaveOpen;
    private bool _isDisposed;
    private string[]? _delimiters;
    private List<Func<string, bool>> _fieldValidators;
    private List<FieldSchema> _fieldSchemas;

    // First character of every delimiter, for vectorised scanning.
    private SearchValues<char>? _delimiterStarts;
    // Same set plus '"', used when HasFieldsEnclosedInQuotes is true.
    private SearchValues<char>? _delimiterStartsOrQuote;
    private bool _allDelimitersSingleChar;

    // Reused between rows so only the returned array is allocated per row.
    private readonly List<ReadOnlyMemory<char>> _fieldBuffer = new();

    public bool HasFieldsEnclosedInQuotes { get; set; } = false;

    public bool EndOfData => _reader?.EndOfStream ?? true;

    public TextFieldParser(Stream stream) : this(stream, Encoding.UTF8, true, false) { }

    public TextFieldParser(Stream stream, Encoding defaultEncoding) : this(stream, defaultEncoding, true, false) { }

    public TextFieldParser(Stream stream, Encoding defaultEncoding, bool detectEncoding) : this(stream, defaultEncoding, detectEncoding, false) { }

    public TextFieldParser(Stream stream, Encoding defaultEncoding, bool detectEncoding, bool leaveOpen)
    {
      _leaveOpen = leaveOpen;
      InitializeFromStream(stream, defaultEncoding, detectEncoding);
    }

    private void InitializeFromStream(Stream stream, Encoding defaultEncoding, bool detectEncoding)
    {
      _reader = stream != null
          ? new StreamReader(stream, defaultEncoding, detectEncoding, leaveOpen: _leaveOpen)
          : throw new ArgumentNullException(nameof(stream), "Stream cannot be null.");
    }

    public void SetFieldSchemas(params FieldSchema[] fieldSchemas)
    {
      if (fieldSchemas == null || fieldSchemas.Length == 0)
        throw new ArgumentException("At least one field schema must be provided.", nameof(fieldSchemas));

      _fieldSchemas = new List<FieldSchema>(fieldSchemas);
    }

    public void ValidateFieldsWithSchema(ReadOnlyMemory<char>[] fields)
    {
      if (_fieldSchemas == null)
        throw new InvalidOperationException("Field schemas are not set. Use SetFieldSchemas() first.");

      if (fields.Length != _fieldSchemas.Count)
        throw new FormatException($"Field count mismatch. Expected {_fieldSchemas.Count}, but got {fields.Length}.");

      for (int i = 0; i < fields.Length; i++)
      {
        string field = fields[i].ToString();
        if (!_fieldSchemas[i].Validate(field))
          throw new FormatException($"Field validation failed for field {i + 1}: {field}");
      }
    }

    public ReadOnlyMemory<char>[] ReadFieldsWithSchemaValidation()
    {
      var fields = ReadFields();
      if (fields != null)
      {
        ValidateFieldsWithSchema(fields);
      }
      return fields;
    }

    public void SetDelimiters(params string[] delimiters)
    {
      if (delimiters == null || delimiters.Length == 0)
        throw new ArgumentException("At least one delimiter must be provided.", nameof(delimiters));

      foreach (var delimiter in delimiters)
      {
        if (string.IsNullOrEmpty(delimiter))
          throw new ArgumentException("Delimiters cannot be null or empty.", nameof(delimiters));
      }

      _delimiters = delimiters;

      var startChars = new HashSet<char>();
      bool allSingleChar = true;
      foreach (var delimiter in delimiters)
      {
        startChars.Add(delimiter[0]);
        if (delimiter.Length != 1)
          allSingleChar = false;
      }

      var startArray = new char[startChars.Count];
      startChars.CopyTo(startArray);
      _delimiterStarts = SearchValues.Create(startArray);

      startChars.Add('"');
      var startOrQuoteArray = new char[startChars.Count];
      startChars.CopyTo(startOrQuoteArray);
      _delimiterStartsOrQuote = SearchValues.Create(startOrQuoteArray);

      _allDelimitersSingleChar = allSingleChar;
    }
    public void SetValidatorSchema(params Func<string, bool>[] fieldValidators)
    {
      if (fieldValidators == null || fieldValidators.Length == 0)
        throw new ArgumentException("At least one validator must be provided.", nameof(fieldValidators));

      _fieldValidators = new List<Func<string, bool>>(fieldValidators);
    }

    public ReadOnlyMemory<char>[] ReadFields()
    {
      if (_delimiters == null)
        throw new InvalidOperationException("Delimiters are not set. Use SetDelimiters() first.");

      string line = ReadLine();
      if (line == null) return null;

      return HasFieldsEnclosedInQuotes ? ParseQuotedFields(line) : SplitLineToFields(line);
    }

    public ReadOnlyMemory<char>[] ReadFieldsWithValidation()
    {
      var fields = ReadFields();
      if (fields != null)
      {
        ValidateFields(fields);
      }
      return fields;
    }

    public void ValidateFields(ReadOnlyMemory<char>[] fields)
    {
      if (_fieldValidators == null)
        throw new InvalidOperationException("Schema is not set. Use SetValidatorSchema() first.");

      if (fields.Length != _fieldValidators.Count)
        throw new FormatException($"Field count mismatch. Expected {_fieldValidators.Count}, but got {fields.Length}.");

      for (int i = 0; i < fields.Length; i++)
      {
        string field = fields[i].ToString();
        if (!_fieldValidators[i](field))
          throw new FormatException($"Field validation failed for field {i + 1}: {field}");
      }
    }

    private ReadOnlyMemory<char>[] SplitLineToFields(string line)
    {
      var fields = _fieldBuffer;
      fields.Clear();

      ReadOnlySpan<char> span = line;
      int start = 0;
      int pos = 0;

      while (true)
      {
        // Jump straight to the next character that could begin a delimiter.
        int offset = span.Slice(pos).IndexOfAny(_delimiterStarts!);
        if (offset < 0)
          break;

        int i = pos + offset;
        if (IsDelimiterAt(span, i, out int delimiterLength))
        {
          fields.Add(line.AsMemory(start, i - start));
          start = pos = i + delimiterLength;
        }
        else
        {
          pos = i + 1;
        }
      }

      fields.Add(line.AsMemory(start)); // Add the final field
      return fields.ToArray();
    }

    private ReadOnlyMemory<char>[] ParseQuotedFields(string line)
    {
      if (string.IsNullOrEmpty(line))
        return Array.Empty<ReadOnlyMemory<char>>();

      var fields = _fieldBuffer;
      fields.Clear();

      ReadOnlySpan<char> span = line;
      int start = 0;
      int pos = 0;
      bool inQuotes = false;

      while (true)
      {
        // Inside quotes only a quote can change state; outside, a quote or a delimiter can.
        int offset = inQuotes
          ? span.Slice(pos).IndexOf('"')
          : span.Slice(pos).IndexOfAny(_delimiterStartsOrQuote!);
        if (offset < 0)
          break;

        int i = pos + offset;
        if (span[i] == '"')
        {
          if (inQuotes && i + 1 < span.Length && span[i + 1] == '"')
          {
            pos = i + 2; // Skip escaped quote
            continue;
          }

          inQuotes = !inQuotes;
          pos = i + 1;
        }
        else if (IsDelimiterAt(span, i, out int delimiterLength))
        {
          fields.Add(line.AsMemory(start, i - start).Trim('"'));
          start = pos = i + delimiterLength;
        }
        else
        {
          pos = i + 1;
        }
      }

      fields.Add(line.AsMemory(start).Trim('"'));

      if (inQuotes)
        throw new FormatException($"Unbalanced quotes in line: {line}");

      return fields.ToArray();
    }

    // Called only at positions holding the first character of some delimiter.
    private bool IsDelimiterAt(ReadOnlySpan<char> line, int index, out int delimiterLength)
    {
      if (_allDelimitersSingleChar)
      {
        delimiterLength = 1;
        return true;
      }

      var remaining = line.Slice(index);
      foreach (string delimiter in _delimiters!)
      {
        if (remaining.StartsWith(delimiter))
        {
          delimiterLength = delimiter.Length;
          return true;
        }
      }

      delimiterLength = 0;
      return false;
    }

    public string ReadLine() => _reader?.ReadLine();

    public string ReadToEnd() => _reader?.ReadToEnd();

    public void Dispose()
    {
      if (_isDisposed) return;

      if (_reader != null && !_leaveOpen)
        _reader.Dispose();

      _reader = null;
      _isDisposed = true;
    }
  }
}
