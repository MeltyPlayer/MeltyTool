using fin.animation.interpolation;
using fin.util.asserts;

using NUnit.Framework;

namespace fin.animation.keyframes;

public class StairStepKeyframesTests {
  [Test]
  public void TestAddToEnd() {
    var impl = new StairStepKeyframes<string>(new SharedInterpolationConfig());

    impl.SetKeyframe(0, "0");
    impl.SetKeyframe(1, "1");
    impl.SetKeyframe(2, "2");
    impl.SetKeyframe(3, "3");
    impl.SetKeyframe(4, "4");

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
    var impl = new StairStepKeyframes<string>(new SharedInterpolationConfig());

    impl.SetKeyframe(1, "first");
    impl.SetKeyframe(1, "second");
    impl.SetKeyframe(1, "third");

    AssertKeyframes_(impl, new Keyframe<string>(1, "third"));
  }

  [Test]
  public void TestInsertAtFront() {
    var impl = new StairStepKeyframes<string>(new SharedInterpolationConfig());

    impl.SetKeyframe(4, "4");
    impl.SetKeyframe(5, "5");
    impl.SetKeyframe(2, "2");
    impl.SetKeyframe(1, "1");
    impl.SetKeyframe(0, "0");

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
    var impl = new StairStepKeyframes<string>(new SharedInterpolationConfig());

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
    var impl = new StairStepKeyframes<string>(new SharedInterpolationConfig());

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
    var impl = new StairStepKeyframes<string>(new SharedInterpolationConfig());

    impl.SetKeyframe(0, "first");
    impl.SetKeyframe(2, "second");
    impl.SetKeyframe(4, "third");

    Assert.AreEqual(new Keyframe<string>(0, "first"),
                    impl.Definitions[0]);
    Assert.AreEqual(new Keyframe<string>(2, "second"),
                    impl.Definitions[1]);
    Assert.AreEqual(new Keyframe<string>(4, "third"),
                    impl.Definitions[2]);
  }

  [Test]
  public void TestInterpolateValesNonLooping() {
    var impl = new StairStepKeyframes<string>(new SharedInterpolationConfig());

    impl.SetKeyframe(0, "first");
    impl.SetKeyframe(2, "second");
    impl.SetKeyframe(4, "third");

    Assert.False(impl.TryGetAtFrame(-1, out var value0));
    this.AssertFrame_(impl, 0, "first");
    this.AssertFrame_(impl, 1, "first");
    this.AssertFrame_(impl, 2, "second");
    this.AssertFrame_(impl, 3, "second");
    this.AssertFrame_(impl, 4, "third");
    this.AssertFrame_(impl, 5, "third");
  }

  [Test]
  public void TestInterpolateValesLooping() {
    var impl = new StairStepKeyframes<string>(new SharedInterpolationConfig {
        AnimationLength = 6, Looping = true
    });

    impl.SetKeyframe(0, "first");
    impl.SetKeyframe(2, "second");
    impl.SetKeyframe(4, "third");

    this.AssertFrame_(impl, -1, "third");
    this.AssertFrame_(impl, 0, "first");
    this.AssertFrame_(impl, 1, "first");
    this.AssertFrame_(impl, 2, "second");
    this.AssertFrame_(impl, 3, "second");
    this.AssertFrame_(impl, 4, "third");
    this.AssertFrame_(impl, 5, "third");
    this.AssertFrame_(impl, 6, "first");
  }

  private void AssertFrame_<T>(IInterpolatable<T> impl,
                               float frame,
                               T expected) {
    Assert.True(impl.TryGetAtFrame(frame, out var actual));
    Assert.AreEqual(expected, actual);
  }

  private void AssertKeyframes_(IReadOnlyKeyframes<Keyframe<string>> actual,
                                params Keyframe<string>[] expected)
    => Asserts.SequenceEqual(expected, actual.Definitions);
}