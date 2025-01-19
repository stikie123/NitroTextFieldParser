using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NitroTextFieldParser.Helpers
{
  public class FieldSchema
  {
    public int Length { get; set; } = -1; // -1 means no length constraint
    public Regex Pattern { get; set; }
    public Func<string, bool> CustomValidator { get; set; }

    public FieldSchema(int length = -1, string pattern = null, Func<string, bool> customValidator = null)
    {
      Length = length;
      Pattern = string.IsNullOrEmpty(pattern) ? null : new Regex(pattern, RegexOptions.Compiled);
      CustomValidator = customValidator;
    }

    public bool Validate(string field)
    {
      if (Length > 0 && field.Length != Length)
        return false;

      if (Pattern != null && !Pattern.IsMatch(field))
        return false;

      if (CustomValidator != null && !CustomValidator(field))
        return false;

      return true;
    }
  }

}
