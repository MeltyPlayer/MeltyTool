using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;

using System.Collections.Generic;
using System.Text;

using asserts;

using schema.binary.util;


namespace schema.binary.io {
  public class NestedListsTests {
    [Test]
    public void TestEmptyList() {
      var impl = new NestedList<string>();
      AssertSequence_(new string[] { }, impl);
    }

    [Test]
    public void TestEmptyNestedLists() {
      var impl = new NestedList<string>();
      impl.Enter().Enter().Enter();
      AssertSequence_(new string[] { }, impl);
    }

    [Test]
    public async Task TestValues() {
      var impl = new NestedList<string>();

      impl.Add("first");
      impl.Add("second");
      impl.Add("third");

      AssertSequence_(new[] {"first", "second", "third"}, impl);
    }

    [Test]
    public async Task TestNestedValues() {
      var impl = new NestedList<string>();

      impl.Add("before children");
      var child1 = impl.Enter();
      var grandchild1A = impl.Enter();
      impl.Add("between children");
      {
        var twin2i = impl.Enter();
        var twin2ii = impl.Enter();
        twin2i.Add("in twin 2i");
        twin2ii.Add("in twin 2ii");
      }
      impl.Add("after children");

      child1.Add("in child 1");
      grandchild1A.Add("in grandchild 1A");

      AssertSequence_(
          new[] {
              "before children", "in child 1", "in grandchild 1A",
              "between children", "in twin 2i", "in twin 2ii", "after children"
          },
          impl);
    }

    private void AssertSequence_<T>(
        IEnumerable<T> enumerableA,
        IEnumerable<T> enumerableB) {
      var enumeratorA = enumerableA.GetEnumerator();
      var enumeratorB = enumerableB.GetEnumerator();

      var hasA = enumeratorA.MoveNext();
      var hasB = enumeratorB.MoveNext();

      var index = 0;
      while (hasA && hasB) {
        var currentA = enumeratorA.Current;
        var currentB = enumeratorB.Current;

        if (!object.Equals(currentA, currentB)) {
          Asserts.Fail(
              $"Expected {currentA} to equal {currentB} at index ${index}.");
        }
        index++;

        hasA = enumeratorA.MoveNext();
        hasB = enumeratorB.MoveNext();
      }

      Asserts.True(!hasA && !hasB,
                   "Expected enumerables to be equal:\n" +
                   $"  A: {ConvertSequenceToString_(enumerableA)}\n" +
                   $"  B: {ConvertSequenceToString_(enumerableB)}");
    }

    private string ConvertSequenceToString_(IEnumerable enumerable) {
      var str = new StringBuilder();
      foreach (var value in enumerable) {
        if (str.Length > 0) {
          str.Append(", ");
        }
        str.Append(value);
      }
      return str.ToString();
    }
  }
}