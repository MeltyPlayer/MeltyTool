using System;

using fin.animation.interpolation;
using fin.util.asserts;
using fin.util.optional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

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
  public void TestInterpolateValuesNonLoopingWithoutDefault() {
    var impl = new StairStepKeyframes<string>(
        new SharedInterpolationConfig { AnimationLength = 8 });

    impl.SetKeyframe(2, "first");
    impl.SetKeyframe(4, "second");
    impl.SetKeyframe(6, "third");

    Assert.IsFalse(impl.TryGetAtFrame(-1, out _));

    Assert.IsFalse(impl.TryGetAtFrame(0, out _));
    Assert.IsFalse(impl.TryGetAtFrame(1, out _));
    AssertFrame_(impl, 2, "first");
    AssertFrame_(impl, 3, "first");
    AssertFrame_(impl, 4, "second");
    AssertFrame_(impl, 5, "second");
    AssertFrame_(impl, 6, "third");
    AssertFrame_(impl, 7, "third");

    AssertFrame_(impl, 8, "third");
  }

  [Test]
  public void TestInterpolateValuesNonLoopingWithDefault() {
    var impl = new StairStepKeyframes<string>(
        new SharedInterpolationConfig { AnimationLength = 8 },
        new IndividualInterpolationConfig<
            string> {
            DefaultValue = Optional.Of("default"),
        });

    impl.SetKeyframe(2, "first");
    impl.SetKeyframe(4, "second");
    impl.SetKeyframe(6, "third");

    AssertFrame_(impl, -1, "default");

    AssertFrame_(impl, 0, "default");
    AssertFrame_(impl, 1, "default");
    AssertFrame_(impl, 2, "first");
    AssertFrame_(impl, 3, "first");
    AssertFrame_(impl, 4, "second");
    AssertFrame_(impl, 5, "second");
    AssertFrame_(impl, 6, "third");
    AssertFrame_(impl, 7, "third");

    AssertFrame_(impl, 8, "third");
  }

  [Test]
  public void TestInterpolateValuesLooping() {
    var impl = new StairStepKeyframes<string>(
        new SharedInterpolationConfig {
            AnimationLength = 8,
            Looping = true
        },
        new IndividualInterpolationConfig<
            string> {
            DefaultValue = Optional.Of("default"),
        });

    impl.SetKeyframe(2, "first");
    impl.SetKeyframe(4, "second");
    impl.SetKeyframe(6, "third");

    AssertFrame_(impl, -1, "third");
    AssertFrame_(impl, 0, "third");
    AssertFrame_(impl, 1, "third");

    AssertFrame_(impl, 2, "first");
    AssertFrame_(impl, 3, "first");
    AssertFrame_(impl, 4, "second");
    AssertFrame_(impl, 5, "second");
    AssertFrame_(impl, 6, "third");
    AssertFrame_(impl, 7, "third");

    AssertFrame_(impl, 8, "third");
  }

  [Test]
  public void TestGetAllFramesNonLooping() {
    var impl = new StairStepKeyframes<char>(
        new SharedInterpolationConfig(),
        new IndividualInterpolationConfig<char> {
            DefaultValue = Optional.Of('d')
        });
    impl.SetKeyframe(2, 'a');
    impl.SetKeyframe(4, 'b');
    impl.SetKeyframe(6, 'c');

    Span<char> frames = stackalloc char[8];
    impl.GetAllFrames(frames);

    Asserts.SpansEqual(['d', 'd', 'a', 'a', 'b', 'b', 'c', 'c'], frames);
  }

  [Test]
  public void TestGetAllFramesLooping() {
    var impl = new StairStepKeyframes<char>(
        new SharedInterpolationConfig { Looping = true },
        new IndividualInterpolationConfig<char> {
            DefaultValue = Optional.Of('d')
        });
    impl.SetKeyframe(2, 'a');
    impl.SetKeyframe(4, 'b');
    impl.SetKeyframe(6, 'c');

    Span<char> frames = stackalloc char[8];
    impl.GetAllFrames(frames);

    Asserts.SpansEqual(['c', 'c', 'a', 'a', 'b', 'b', 'c', 'c'], frames);
  }

  private static void AssertFrame_<T>(IInterpolatable<T> impl,
                                      float frame,
                                      T expected) {
    Assert.IsTrue(impl.TryGetAtFrame(frame, out var actual));
    Assert.AreEqual(expected, actual);
  }

  private static void AssertKeyframes_(
      IReadOnlyKeyframes<Keyframe<string>> actual,
      params Keyframe<string>[] expected)
    => Asserts.SequenceEqual(expected, actual.Definitions);
}