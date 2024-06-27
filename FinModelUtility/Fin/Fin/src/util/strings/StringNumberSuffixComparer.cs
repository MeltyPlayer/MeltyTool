using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace fin.util.strings;

public class StringNumberSuffixComparer : IComparer<string> {
  private readonly Regex regex_ = new(@"^(\D*)(\d*)$");

  public int Compare(string lhs, string rhs) {
    var lhsMatch = this.regex_.Match(lhs);
    var rhsMatch = this.regex_.Match(rhs);

    var lhsGroups = lhsMatch.Groups;
    var rhsGroups = rhsMatch.Groups;

    if (!lhsMatch.Success || lhsGroups.Count < 3 || !rhsMatch.Success ||
        rhsGroups.Count < 3) {
      return lhs.CompareTo(rhs);
    }

    var textComparison = lhsGroups[1].Value.CompareTo(rhsGroups[1].Value);
    if (textComparison != 0) {
      return textComparison;
    }

    var lhsNumber = int.Parse(lhsGroups[2].Value);
    var rhsNumber = int.Parse(rhsGroups[2].Value);
    return lhsNumber.CompareTo(rhsNumber);
  }
}