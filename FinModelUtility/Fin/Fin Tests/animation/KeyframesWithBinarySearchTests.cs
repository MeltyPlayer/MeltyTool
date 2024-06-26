using fin.animation.keyframes;
using fin.util.asserts;

using NUnit.Framework;

namespace fin.animation {
  public class KeyframeDefinitionsWithBinarySearchTests {
    [Test]
    public void TestAddToEnd() {
      var impl = new KeyframeDefinitionsWithBinarySearch<string>();

      impl.SetKeyframe(0, "0", out var performedBinarySearch0);
      Assert.False(performedBinarySearch0);

      impl.SetKeyframe(1, "1", out var performedBinarySearch1);
      Assert.False(performedBinarySearch1);

      impl.SetKeyframe(2, "2", out var performedBinarySearch2);
      Assert.False(performedBinarySearch2);

      impl.SetKeyframe(3, "3", out var performedBinarySearch3);
      Assert.False(performedBinarySearch3);

      impl.SetKeyframe(4, "4", out var performedBinarySearch4);
      Assert.False(performedBinarySearch4);

      AssertKeyframes_(impl,
                       new Keyframe<string>(0, "0"),
                       new Keyframe<string>(1, "1"),
                       new Keyframe<string>(2, "2"),
                       new Keyframe<string>(3, "3"),
                       new Keyframe<string>(4, "4")
      );
    }

    [Test]
    public void TestReplace() {
      var impl = new KeyframeDefinitionsWithBinarySearch<string>();

      impl.SetKeyframe(1, "first", out var performedBinarySearch1);
      Assert.False(performedBinarySearch1);

      impl.SetKeyframe(1, "second", out var performedBinarySearch2);
      Assert.False(performedBinarySearch2);

      impl.SetKeyframe(1, "third", out var performedBinarySearch3);
      Assert.False(performedBinarySearch3);

      AssertKeyframes_(impl, new Keyframe<string>(1, "third"));
    }

    [Test]
    public void TestInsertAtFront() {
      var impl = new KeyframeDefinitionsWithBinarySearch<string>();

      impl.SetKeyframe(4, "4", out var performedBinarySearch4);
      Assert.False(performedBinarySearch4);

      impl.SetKeyframe(5, "5", out var performedBinarySearch5);
      Assert.False(performedBinarySearch5);
      
      impl.SetKeyframe(2, "2", out var performedBinarySearch2);
      Assert.True(performedBinarySearch2);
      
      impl.SetKeyframe(1, "1", out var performedBinarySearch1);
      Assert.True(performedBinarySearch1);

      impl.SetKeyframe(0, "0", out var performedBinarySearch0);
      Assert.True(performedBinarySearch0);

      AssertKeyframes_(impl,
                       new Keyframe<string>(0, "0"),
                       new Keyframe<string>(1, "1"),
                       new Keyframe<string>(2, "2"),
                       new Keyframe<string>(4, "4"),
                       new Keyframe<string>(5, "5")
      );
    }

    [Test]
    public void TestInsertInMiddle() {
      var impl = new KeyframeDefinitionsWithBinarySearch<string>();

      impl.SetKeyframe(0, "0");
      impl.SetKeyframe(9, "9");
      impl.SetKeyframe(5, "5");
      impl.SetKeyframe(2, "2");
      impl.SetKeyframe(7, "7");

      AssertKeyframes_(impl,
                       new Keyframe<string>(0, "0"),
                       new Keyframe<string>(2, "2"),
                       new Keyframe<string>(5, "5"),
                       new Keyframe<string>(7, "7"),
                       new Keyframe<string>(9, "9")
      );
    }

    [Test]
    public void TestHugeRange() {
      var impl = new KeyframeDefinitionsWithBinarySearch<string>();

      impl.SetKeyframe(1000, "1000");
      impl.SetKeyframe(2, "2");
      impl.SetKeyframe(123, "123");

      AssertKeyframes_(impl,
                       new Keyframe<string>(2, "2"),
                       new Keyframe<string>(123, "123"),
                       new Keyframe<string>(1000, "1000")
      );
    }

    [Test]
    public void TestGetIndices() {
      var impl = new KeyframeDefinitionsWithBinarySearch<string>();

      impl.SetKeyframe(0, "first");
      impl.SetKeyframe(2, "second");
      impl.SetKeyframe(4, "third");

      Assert.AreEqual(new Keyframe<string>(0, "first"),
                      impl.GetKeyframeAtIndex(0));
      Assert.AreEqual(new Keyframe<string>(2, "second"),
                      impl.GetKeyframeAtIndex(1));
      Assert.AreEqual(new Keyframe<string>(4, "third"),
                      impl.GetKeyframeAtIndex(2));
    }

    [Test]
    public void TestGetKeyframes() {
      var impl = new KeyframeDefinitionsWithBinarySearch<string>();

      impl.SetKeyframe(0, "first");
      impl.SetKeyframe(2, "second");
      impl.SetKeyframe(4, "third");

      AssertKeyframe_(new Keyframe<string>(0, default),
                      impl.GetKeyframeAtFrame(-1));
      AssertKeyframe_(new Keyframe<string>(0, "first"),
                      impl.GetKeyframeAtFrame(0));
      AssertKeyframe_(new Keyframe<string>(0, "first"),
                      impl.GetKeyframeAtFrame(1));
      AssertKeyframe_(new Keyframe<string>(2, "second"),
                      impl.GetKeyframeAtFrame(2));
      AssertKeyframe_(new Keyframe<string>(2, "second"),
                      impl.GetKeyframeAtFrame(3));
      AssertKeyframe_(new Keyframe<string>(4, "third"),
                      impl.GetKeyframeAtFrame(4));
      AssertKeyframe_(new Keyframe<string>(4, "third"),
                      impl.GetKeyframeAtFrame(5));
    }

    [Test]
    public void TestFindIndexOfKeyframe() {
      var impl = new KeyframeDefinitionsWithBinarySearch<string>();

      impl.SetKeyframe(0, "0");
      impl.SetKeyframe(1, "1");

      impl.FindIndexOfKeyframe(-1,
                               out var keyframeIndexMinus1,
                               out _,
                               out var isLastKeyframeMinus1);
      Assert.AreEqual(0, keyframeIndexMinus1);
      Assert.AreEqual(false, isLastKeyframeMinus1);

      impl.FindIndexOfKeyframe(0,
                               out var keyframeIndex0,
                               out _,
                               out var isLastKeyframe0);
      Assert.AreEqual(0, keyframeIndex0);
      Assert.AreEqual(false, isLastKeyframe0);

      impl.FindIndexOfKeyframe(1,
                               out var keyframeIndex1,
                               out _,
                               out var isLastKeyframe1);
      Assert.AreEqual(1, keyframeIndex1);
      Assert.AreEqual(true, isLastKeyframe1);

      impl.FindIndexOfKeyframe(2,
                               out var keyframeIndex2,
                               out _,
                               out var isLastKeyframe2);
      Assert.AreEqual(1, keyframeIndex2);
      Assert.AreEqual(true, isLastKeyframe2);
    }

    [Test]
    public void TestFindIndexOfKeyframeWhenEmpty() {
      var impl = new KeyframeDefinitionsWithBinarySearch<string>();

      impl.FindIndexOfKeyframe(-1,
                               out var keyframeIndexMinus1,
                               out _,
                               out var isLastKeyframeMinus1);
      Assert.AreEqual(0, keyframeIndexMinus1);
      Assert.AreEqual(false, isLastKeyframeMinus1);

      impl.FindIndexOfKeyframe(0,
                               out var keyframeIndex0,
                               out _,
                               out var isLastKeyframe0);
      Assert.AreEqual(0, keyframeIndex0);
      Assert.AreEqual(false, isLastKeyframe0);
    }

    [Test]
    public void TestFindManyIndices() {
      var impl = new KeyframeDefinitionsWithBinarySearch<string>();

      var s = 2;
      var n = 25;
      for (var sI = 0; sI < n; sI += s) {
        impl.SetKeyframe(sI, $"{sI}");
      }

      for (var i = 0; i < n; ++i) {
        var sI = i - (i % s);

        var isKeyframeDefined = impl.FindIndexOfKeyframe(i,
          out var keyframeIndex,
          out var keyframe,
          out var isLastKeyframe);

        Assert.True(isKeyframeDefined);
        Assert.AreEqual(sI / s, keyframeIndex);
        Assert.AreEqual(sI, keyframe.Frame);
        Assert.AreEqual(i == n - 1, isLastKeyframe);
      }
    }

    private void AssertKeyframe_(Keyframe<string>? expected,
                                 Keyframe<string>? actual)
      => Assert.AreEqual(expected, actual);

    private void AssertKeyframes_(KeyframeDefinitionsWithBinarySearch<string> actual,
                                  params Keyframe<string>[] expected)
      => Asserts.SequenceEqual(expected, actual.Definitions);
  }
}