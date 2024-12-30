using System.Text;
using System.Text.RegularExpressions;

namespace NitroTextFieldParser
{
  /// <summary>
  /// A reimplementation of a TextFieldParser for parsing delimited or fixed-width fields.
  /// </summary>
  public class NitroTextFieldParser : IDisposable
  {
    private StreamReader _reader;
    private readonly bool _leaveOpen;
    private bool _isDisposed;
    private string[] _delimiters;
    private Regex _delimiterRegex;

    public bool HasFieldsEnclosedInQuotes { get; set; } = false;

    public bool EndOfData => _reader?.EndOfStream ?? true;

    public NitroTextFieldParser(Stream stream) : this(stream, Encoding.UTF8, true, false) { }

    public NitroTextFieldParser(Stream stream, Encoding defaultEncoding) : this(stream, defaultEncoding, true, false) { }

    public NitroTextFieldParser(Stream stream, Encoding defaultEncoding, bool detectEncoding) : this(stream, defaultEncoding, detectEncoding, false) { }

    public NitroTextFieldParser(Stream stream, Encoding defaultEncoding, bool detectEncoding, bool leaveOpen)
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
      string pattern = string.Join("|", Array.ConvertAll(delimiters, Regex.Escape));
      _delimiterRegex = new Regex(pattern, RegexOptions.Compiled);
    }

    public ReadOnlyMemory<char>[] ReadFields()
    {
      if (_delimiters == null)
        throw new InvalidOperationException("Delimiters are not set. Use SetDelimiters() first.");

      string line = ReadLine();
      if (line == null) return null;

      return HasFieldsEnclosedInQuotes ? ParseQuotedFields(line) : SplitLineToFields(line);
    }

    private ReadOnlyMemory<char>[] SplitLineToFields(string line)
    {
      var fields = new List<ReadOnlyMemory<char>>();
      int start = 0;

      for (int i = 0; i < line.Length; i++)
      {
        if (IsDelimiterAt(line, i, out int delimiterLength))
        {
          fields.Add(line.AsMemory(start, i - start));
          start = i + delimiterLength;
          i += delimiterLength - 1; // Move past the delimiter
        }
      }

      fields.Add(line.AsMemory(start)); // Add the final field
      return fields.ToArray();
    }

    private ReadOnlyMemory<char>[] ParseQuotedFields(string line)
    {
      var fields = new List<ReadOnlyMemory<char>>();
      int start = 0, fieldStart = 0;
      bool inQuotes = false;

      for (int i = 0; i < line.Length; i++)
      {
        if (line[i] == '"')
        {
          if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
          {
            i++; // Skip escaped quote
          }
          else
          {
            inQuotes = !inQuotes;
            if (!inQuotes) fieldStart = i + 1;
          }
        }
        else if (!inQuotes && IsDelimiterAt(line, i, out int delimiterLength))
        {
          fields.Add(line.AsMemory(start, fieldStart - start));
          start = i + delimiterLength;
          i += delimiterLength - 1; // Move past the delimiter
        }
      }

      fields.Add(line.AsMemory(start));
      return fields.ToArray();
    }

    private bool IsDelimiterAt(string line, int index, out int delimiterLength)
    {
      foreach (string delimiter in _delimiters)
      {
        if (line.AsSpan(index).StartsWith(delimiter))
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
