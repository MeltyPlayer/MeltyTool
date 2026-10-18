using System;
using System.Collections.Generic;

using fin.animation.keyframes;

using readOnly;

namespace fin.animation.interpolation;

public interface IInterpolatable<T> {
  bool HasAnyData { get; }

  int AnimationLength { get; }
  bool Looping { get; }

  [Const]
  bool TryGetAtFrameWithBinarySearch(float frame, out T value);

  // Returns the value at each frame in the animation.
  [Const]
  void GetAllFrames(Span<T> dst);

  // Returns just the keyframes of the animation.
  [Const]
  bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, T value)> keyframes,
      out IReadOnlyList<(T tangentIn, T tangentOut)>? tangentKeyframes);

  [Const]
  IAdvancingInterpolator<T> GetAdvancingInterpolator();
}

public interface IKeyframeInterpolator<in TKeyframe, out T>
    where TKeyframe : IKeyframe {
  T Interpolate(TKeyframe from,
                TKeyframe to,
                float frame,
                ISharedInterpolationConfig sharedInterpolationConfig);
}