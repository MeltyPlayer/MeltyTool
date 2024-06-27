using System.Collections.Generic;
using System.Linq;

using fin.util.asserts;

using NUnit.Framework;

namespace fin.util.strings;

public class StringNumberSuffixComparerTests {
  [Test]
  public void TestOrder() {
    var inputStrings = new[] {
        "bar10", "bar1", "foo3", "bar5", "foo13", "bar2", "foo1"
    };

    var expectedOutputStrings = new[] {
        "bar1", "bar2", "bar5", "bar10", "foo1", "foo3", "foo13",
    };

    Asserts.SequenceEqual<IEnumerable<string>>(
        expectedOutputStrings,
        inputStrings.OrderBy(value => value,
                             new StringNumberSuffixComparer()));
  }
}