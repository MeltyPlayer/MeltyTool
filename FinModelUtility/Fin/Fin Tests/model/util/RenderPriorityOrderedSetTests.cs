using System.Collections.Generic;

using fin.util.asserts;

using NUnit.Framework;

namespace fin.model.util;

public class RenderPriorityOrderedSetTests {
  [Test]
  public void TestSimplestOrder() {
    var impl = new RenderPriorityOrderedSet<string> {
        { "abc", 1, true },
        { "123", 1, false },
        { "bar", 0, true },
        { "foo", 0, false },
    };

    Asserts.SequenceEqual<IEnumerable<string>>(
        impl,
        [
            "foo",
            "bar",
            "123",
            "abc",
        ]);
  }

  [Test]
  public void TestSimplestOrderWithDuplicates() {
    var impl = new RenderPriorityOrderedSet<string> {
        { "bar-1", 3, true },
        { "bar-2", 3, true },
        { "foo", 2, false },
        { "abc", 1, true },
        { "123", 1, false },
        { "bar-2", 0, true },
        { "bar-1", 0, true },
        { "foo", 0, false },
    };

    Asserts.SequenceEqual<IEnumerable<string>>(
        impl,
        [
            "foo",
            "bar-1",
            "bar-2",
            "123",
            "abc",
        ]);
  }
}