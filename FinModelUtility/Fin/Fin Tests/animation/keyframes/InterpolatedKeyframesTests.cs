using fin.animation.interpolation;
using fin.animation.types.single;
using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.animation.keyframes;

public class InterpolatedKeyframesTests {
  [Test]
  public void TestAddToEnd() {
    var impl = new InterpolatedKeyframes<Keyframe<float>, float>(
        new SharedInterpolationConfig(),
        FloatKeyframeInterpolator.Instance);

    impl.SetKeyframe(0, 0);
    impl.SetKeyframe(1, 1);
    impl.SetKeyframe(2, 2);
    impl.SetKeyframe(3, 3);
    impl.SetKeyframe(4, 4);

    AssertKeyframes_(impl,
                     new Keyframe<float>(0, 0),
                     new Keyframe<float>(1, 1),
                     new Keyframe<float>(2, 2),
                     new Keyframe<float>(3, 3),
                     new Keyframe<float>(4, 4)
    );
  }

  [Test]
  public void TestReplace() {
    var impl = new InterpolatedKeyframes<Keyframe<float>, float>(
        new SharedInterpolationConfig(),
        FloatKeyframeInterpolator.Instance);

    impl.SetKeyframe(1, 1);
    impl.SetKeyframe(1, 2);
    impl.SetKeyframe(1, 3);

    AssertKeyframes_(impl, new Keyframe<float>(1, 3));
  }

  [Test]
  public void TestInsertAtFront() {
    var impl = new InterpolatedKeyframes<Keyframe<float>, float>(
        new SharedInterpolationConfig(),
        FloatKeyframeInterpolator.Instance);

    impl.SetKeyframe(4, 4);
    impl.SetKeyframe(5, 5);
    impl.SetKeyframe(2, 2);
    impl.SetKeyframe(1, 1);
    impl.SetKeyframe(0, 0);

    AssertKeyframes_(impl,
                     new Keyframe<float>(0, 0),
                     new Keyframe<float>(1, 1),
                     new Keyframe<float>(2, 2),
                     new Keyframe<float>(4, 4),
                     new Keyframe<float>(5, 5)
    );
  }

  [Test]
  public void TestInsertInMiddle() {
    var impl = new InterpolatedKeyframes<Keyframe<float>, float>(
        new SharedInterpolationConfig(),
        FloatKeyframeInterpolator.Instance);

    impl.SetKeyframe(0, 0);
    impl.SetKeyframe(9, 9);
    impl.SetKeyframe(5, 5);
    impl.SetKeyframe(2, 2);
    impl.SetKeyframe(7, 7);

    AssertKeyframes_(impl,
                     new Keyframe<float>(0, 0),
                     new Keyframe<float>(2, 2),
                     new Keyframe<float>(5, 5),
                     new Keyframe<float>(7, 7),
                     new Keyframe<float>(9, 9)
    );
  }

  [Test]
  public void TestHugeRange() {
    var impl = new InterpolatedKeyframes<Keyframe<float>, float>(
        new SharedInterpolationConfig(),
        FloatKeyframeInterpolator.Instance);

    impl.SetKeyframe(1000, 1000);
    impl.SetKeyframe(2, 2);
    impl.SetKeyframe(123, 123);

    AssertKeyframes_(impl,
                     new Keyframe<float>(2, 2),
                     new Keyframe<float>(123, 123),
                     new Keyframe<float>(1000, 1000)
    );
  }

  [Test]
  public void TestGetIndices() {
    var impl = new InterpolatedKeyframes<Keyframe<float>, float>(
        new SharedInterpolationConfig(),
        FloatKeyframeInterpolator.Instance);

    impl.SetKeyframe(0, 1);
    impl.SetKeyframe(2, 2);
    impl.SetKeyframe(4, 3);

    Assert.AreEqual(new Keyframe<float>(0, 1),
                    impl.Definitions[0]);
    Assert.AreEqual(new Keyframe<float>(2, 2),
                    impl.Definitions[1]);
    Assert.AreEqual(new Keyframe<float>(4, 3),
                    impl.Definitions[2]);
  }

  [Test]
  public void TestInterpolateValesNonLooping() {
    var impl = new InterpolatedKeyframes<Keyframe<float>, float>(
        new SharedInterpolationConfig(),
        FloatKeyframeInterpolator.Instance);

    impl.SetKeyframe(0, 1);
    impl.SetKeyframe(2, 2);
    impl.SetKeyframe(4, 3);

    Assert.IsFalse(impl.TryGetAtFrame(-1, out _));
    
    AssertFrame_(impl, 0, 1);
    AssertFrame_(impl, 1, 1.5f);
    AssertFrame_(impl, 2, 2);
    AssertFrame_(impl, 3, 2.5f);
    AssertFrame_(impl, 4, 3);

    AssertFrame_(impl, 5, 3);
  }

  [Test]
  public void TestInterpolateValesLooping() {
    var impl = new InterpolatedKeyframes<Keyframe<float>, float>(
        new SharedInterpolationConfig { AnimationLength = 6, Looping = true },
        FloatKeyframeInterpolator.Instance);

    impl.SetKeyframe(0, 1);
    impl.SetKeyframe(2, 2);
    impl.SetKeyframe(4, 3);

    /*AssertFrame_(impl, -1, 2);
    
    AssertFrame_(impl, 0, 1);
    AssertFrame_(impl, 1, 1.5f);
    AssertFrame_(impl, 2, 2);
    AssertFrame_(impl, 3, 2.5f);
    AssertFrame_(impl, 4, 3);
    AssertFrame_(impl, 5, 2);*/
    
    AssertFrame_(impl, 6, 1);
  }

  private static void AssertFrame_<T>(IInterpolatable<T> impl,
                               float frame,
                               T expected) {
    Assert.IsTrue(impl.TryGetAtFrame(frame, out var actual));
    Assert.AreEqual(expected, actual);
  }

  private static void AssertKeyframes_(IReadOnlyKeyframes<Keyframe<float>> actual,
                                params Keyframe<float>[] expected)
    => Asserts.SequenceEqual(expected, actual.Definitions);
}