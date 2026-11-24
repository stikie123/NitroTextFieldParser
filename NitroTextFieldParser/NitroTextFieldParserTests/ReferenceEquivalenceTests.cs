using System.Text;
using NUnit.Framework;

namespace NitroTextFieldParserTests
{
  /// <summary>
  /// Compares the parser against a copy of the original (pre-1.3.0) character-by-character
  /// algorithm on randomly generated lines, so optimisations cannot change results.
  /// </summary>
  [TestFixture]
  public class ReferenceEquivalenceTests
  {
    private static readonly string[][] DelimiterSets =
    {
      new[] { "," },
      new[] { ",", "|" },
      new[] { "||" },
      new[] { "|", "||" },
      new[] { "||", "|" },
      new[] { "::", ";" },
      new[] { "\",", "," },
    };

    private const string Alphabet = "ab ,|;:\"\"";

    [Test]
    public void RandomLines_MatchOriginalAlgorithm([Values(false, true)] bool quoted)
    {
      var random = new Random(12345);

      foreach (var delimiters in DelimiterSets)
      {
        for (int n = 0; n < 3000; n++)
        {
          var line = RandomLine(random);
          AssertSameResult(line, quoted, delimiters);
        }
      }
    }

    private static string RandomLine(Random random)
    {
      int length = random.Next(0, random.Next(2) == 0 ? 12 : 120);
      var sb = new StringBuilder(length);
      for (int i = 0; i < length; i++)
        sb.Append(Alphabet[random.Next(Alphabet.Length)]);
      return sb.ToString();
    }

    private static void AssertSameResult(string line, bool quoted, string[] delimiters)
    {
      string[]? expected = null;
      Type? expectedException = null;
      try { expected = Reference.Parse(line, quoted, delimiters); }
      catch (Exception e) { expectedException = e.GetType(); }

      string[]? actual = null;
      Type? actualException = null;
      try
      {
        using var parser = new NitroTextFieldParser.TextFieldParser(new MemoryStream(Encoding.UTF8.GetBytes(line)));
        parser.SetDelimiters(delimiters);
        parser.HasFieldsEnclosedInQuotes = quoted;
        var fields = parser.ReadFields();
        actual = fields?.Select(f => f.ToString()).ToArray();
      }
      catch (Exception e) { actualException = e.GetType(); }

      var context = $"line=[{line}] quoted={quoted} delimiters=[{string.Join(" ", delimiters)}]";
      Assert.That(actualException, Is.EqualTo(expectedException), context);
      if (expectedException == null)
      {
        // An empty stream yields no line at all (null); compare as empty.
        Assert.That(actual ?? Array.Empty<string>(), Is.EqualTo(line.Length == 0 && !quoted ? Array.Empty<string>() : expected), context);
      }
    }

    /// <summary>Verbatim logic of the original implementation.</summary>
    private static class Reference
    {
      public static string[] Parse(string line, bool quoted, string[] delimiters) =>
        (quoted ? ParseQuotedFields(line, delimiters) : SplitLineToFields(line, delimiters))
          .Select(f => f.ToString()).ToArray();

      private static ReadOnlyMemory<char>[] SplitLineToFields(string line, string[] delimiters)
      {
        var fields = new List<ReadOnlyMemory<char>>();
        int start = 0;
        for (int i = 0; i < line.Length; i++)
        {
          if (IsDelimiterAt(line, i, delimiters, out int delimiterLength))
          {
            fields.Add(line.AsMemory(start, i - start));
            start = i + delimiterLength;
            i += delimiterLength - 1;
          }
        }
        fields.Add(line.AsMemory(start));
        return fields.ToArray();
      }

      private static ReadOnlyMemory<char>[] ParseQuotedFields(string line, string[] delimiters)
      {
        if (string.IsNullOrEmpty(line))
          return Array.Empty<ReadOnlyMemory<char>>();

        var fields = new List<ReadOnlyMemory<char>>();
        int start = 0;
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
          if (line[i] == '"')
          {
            if (inQuotes)
            {
              if (i + 1 < line.Length && line[i + 1] == '"')
                i++;
              else
                inQuotes = false;
            }
            else
            {
              inQuotes = true;
            }
          }
          else if (!inQuotes && IsDelimiterAt(line, i, delimiters, out int delimiterLength))
          {
            fields.Add(line.AsMemory(start, i - start).Trim('"'));
            start = i + delimiterLength;
            i += delimiterLength - 1;
          }
        }
        if (start <= line.Length)
          fields.Add(line.AsMemory(start, line.Length - start).Trim('"'));
        if (inQuotes)
          throw new FormatException($"Unbalanced quotes in line: {line}");
        return fields.ToArray();
      }

      private static bool IsDelimiterAt(string line, int index, string[] delimiters, out int delimiterLength)
      {
        foreach (string delimiter in delimiters)
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
    }
  }
}
