using System;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.types;
using fin.animation.types.quaternion;
using fin.animation.types.single;
using fin.animation.types.vector3;
using fin.util.asserts;
using fin.util.optional;

using NUnit.Framework;

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

    AssertGetAllFramesMatchesInterpolated_(impl, 10);
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

    AssertGetAllFramesMatchesInterpolated_(impl, 10);
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

    AssertGetAllFramesMatchesInterpolated_(impl, 10);
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

    AssertGetAllFramesMatchesInterpolated_(impl, 10);
  }


  [Test]
  public void TestSeparateVector3Nonlooping() {
    var impl = new SeparateVector3Keyframes<Keyframe<float>>(
        new SharedInterpolationConfig { AnimationLength = 10, },
        FloatKeyframeInterpolator.Instance,
        new IndividualInterpolationConfig<float>
            { DefaultValue = Optional.Of(-1f) });

    impl.SetKeyframe(0, 1, 0);
    impl.SetKeyframe(1, 2, 1);
    impl.SetKeyframe(2, 3, 2);

    impl.SetKeyframe(0, 5, 3);
    impl.SetKeyframe(1, 5, 1);
    impl.SetKeyframe(2, 5, 2);

    impl.SetKeyframe(0, 7, 4);
    impl.SetKeyframe(1, 7, 2);
    impl.SetKeyframe(2, 7, 3);

    AssertGetAllFramesMatchesInterpolated_(impl, 10);
  }

  [Test]
  public void TestSeparateVector3Looping() {
    var impl = new SeparateVector3Keyframes<Keyframe<float>>(
        new SharedInterpolationConfig { AnimationLength = 10, Looping = true},
        FloatKeyframeInterpolator.Instance,
        new IndividualInterpolationConfig<float>
            { DefaultValue = Optional.Of(-1f) });

    impl.SetKeyframe(0, 1, 0);
    impl.SetKeyframe(1, 2, 1);
    impl.SetKeyframe(2, 3, 2);

    impl.SetKeyframe(0, 5, 3);
    impl.SetKeyframe(1, 5, 1);
    impl.SetKeyframe(2, 5, 2);

    impl.SetKeyframe(0, 7, 4);
    impl.SetKeyframe(1, 7, 2);
    impl.SetKeyframe(2, 7, 3);

    AssertGetAllFramesMatchesInterpolated_(impl, 10);
  }


  [Test]
  public void TestCombinedVector3Nonlooping() {
    var impl = new CombinedVector3Keyframes<Keyframe<Vector3>>(
        new SharedInterpolationConfig { AnimationLength = 10 },
        Vector3KeyframeInterpolator.Instance,
        new IndividualInterpolationConfig<Vector3>
            { DefaultValue = Optional.Of(new Vector3(-1, -1, -1)) });
    
    impl.SetKeyframe(2, new Vector3(1, 2, 3));
    impl.SetKeyframe(4, new Vector3(4, 2, 3));
    impl.SetKeyframe(6, new Vector3(5, 5, 5));
    impl.SetKeyframe(8, new Vector3(6, 7, 8));

    AssertGetAllFramesMatchesInterpolated_(impl, 10);
  }

  [Test]
  public void TestCombinedVector3Looping() {
    var impl = new CombinedVector3Keyframes<Keyframe<Vector3>>(
        new SharedInterpolationConfig { AnimationLength = 10, Looping = true },
        Vector3KeyframeInterpolator.Instance,
        new IndividualInterpolationConfig<Vector3>
            { DefaultValue = Optional.Of(new Vector3(-1, -1, -1)) });
    
    impl.SetKeyframe(2, new Vector3(1, 2, 3));
    impl.SetKeyframe(4, new Vector3(4, 2, 3));
    impl.SetKeyframe(6, new Vector3(5, 5, 5));
    impl.SetKeyframe(8, new Vector3(6, 7, 8));

    AssertGetAllFramesMatchesInterpolated_(impl, 10);
  }


  [Test]
  public void TestCombinedQuaternionNonlooping() {
    var impl = new CombinedQuaternionKeyframes<Keyframe<Quaternion>>(
        new SharedInterpolationConfig { AnimationLength = 10 },
        QuaternionKeyframeInterpolator.Instance,
        new IndividualInterpolationConfig<Quaternion>
            { DefaultValue = Optional.Of(new Quaternion(-1, -1, -1, -1)) });
    
    impl.SetKeyframe(2, new Quaternion(1, 2, 3, 4));
    impl.SetKeyframe(4, new Quaternion(4, 2, 3, 4));
    impl.SetKeyframe(6, new Quaternion(5, 5, 5, 5));
    impl.SetKeyframe(8, new Quaternion(6, 7, 8, 9));

    AssertGetAllFramesMatchesInterpolated_(impl, 10);
  }

  [Test]
  public void TestCombinedQuaternionLooping() {
    var impl = new CombinedQuaternionKeyframes<Keyframe<Quaternion>>(
        new SharedInterpolationConfig { AnimationLength = 10, Looping = true },
        QuaternionKeyframeInterpolator.Instance,
        new IndividualInterpolationConfig<Quaternion>
            { DefaultValue = Optional.Of(new Quaternion(-1, -1, -1, -1)) });
    
    impl.SetKeyframe(2, new Quaternion(1, 2, 3, 4));
    impl.SetKeyframe(4, new Quaternion(4, 2, 3, 4));
    impl.SetKeyframe(6, new Quaternion(5, 5, 5, 5));
    impl.SetKeyframe(8, new Quaternion(6, 7, 8, 9));

    AssertGetAllFramesMatchesInterpolated_(impl, 10);
  }


  private static void AssertGetAllFramesMatchesInterpolated_<T>(
      IInterpolatable<T> impl,
      int length) where T : unmanaged {
    Span<T> interpolatedFrames = stackalloc T[length];
    for (var i = 0; i < length; i++) {
      Asserts.True(impl.TryGetAtFrame(i, out var value));
      interpolatedFrames[i] = value;
    }

    Span<T> getAllFramesFrames = stackalloc T[length];
    impl.GetAllFrames(getAllFramesFrames);

    Asserts.SpansEqual<T>(interpolatedFrames, getAllFramesFrames);
  }
}