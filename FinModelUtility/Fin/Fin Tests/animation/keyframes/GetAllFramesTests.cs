using System;
using System.Linq;

using fin.animation.interpolation;
using fin.animation.types.single;
using fin.util.asserts;
using fin.util.optional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.animation.keyframes;

public class GetAllFramesTests {
  [Test]
  public void TestStairstepNonlooping() {
    var impl = new StairStepKeyframes<float>(
        new SharedInterpolationConfig { AnimationLength = 10, },
        new IndividualInterpolationConfig<float>
            { DefaultValue = Optional.Of(-1f) });

    impl.SetKeyframe(2, 0);
    impl.SetKeyframe(4, 1);
    impl.SetKeyframe(6, 2);
    impl.SetKeyframe(8, 3);

    AssertGetAllFramesMatchesInterpolated_(impl);
  }

  [Test]
  public void TestStairstepLooping() {
    var impl = new StairStepKeyframes<float>(
        new SharedInterpolationConfig { AnimationLength = 10, Looping = true },
        new IndividualInterpolationConfig<float>
            { DefaultValue = Optional.Of(-1f) });

    impl.SetKeyframe(2, 0);
    impl.SetKeyframe(4, 1);
    impl.SetKeyframe(6, 2);
    impl.SetKeyframe(8, 3);

    AssertGetAllFramesMatchesInterpolated_(impl);
  }

  [Test]
  public void TestInterpolatedNonlooping() {
    var impl = new InterpolatedKeyframes<Keyframe<float>, float>(
        new SharedInterpolationConfig { AnimationLength = 10, },
        FloatKeyframeInterpolator.Instance,
        new IndividualInterpolationConfig<float>
            { DefaultValue = Optional.Of(-1f) });

    impl.SetKeyframe(2, 0);
    impl.SetKeyframe(4, 1);
    impl.SetKeyframe(6, 2);
    impl.SetKeyframe(8, 3);

    AssertGetAllFramesMatchesInterpolated_(impl);
  }

  [Test]
  public void TestInterpolatedLooping() {
    var impl = new InterpolatedKeyframes<Keyframe<float>, float>(
        new SharedInterpolationConfig { AnimationLength = 10, Looping = true },
        FloatKeyframeInterpolator.Instance,
        new IndividualInterpolationConfig<float>
            { DefaultValue = Optional.Of(-1f) });

    impl.SetKeyframe(2, 0);
    impl.SetKeyframe(4, 1);
    impl.SetKeyframe(6, 2);
    impl.SetKeyframe(8, 3);

    AssertGetAllFramesMatchesInterpolated_(impl);
  }

  private static void AssertGetAllFramesMatchesInterpolated_<T>(
      IConfiguredInterpolatable<T> impl) where T : unmanaged {
    var length = impl.SharedConfig.AnimationLength;

    Span<T> interpolatedFrames = stackalloc T[length];
    for (var i = 0; i < length; i++) {
      Asserts.True(impl.TryGetAtFrameOrDefault(i, out var value));
      interpolatedFrames[i] = value;
    }

    Span<T> getAllFramesFrames = stackalloc T[length];
    impl.GetAllFrames(getAllFramesFrames);

    Asserts.SpansEqual<T>(interpolatedFrames, getAllFramesFrames);
  }
}