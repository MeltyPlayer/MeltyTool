using fin.animation.keyframes;

using schema.readOnly;

namespace fin.animation.interpolation;

public interface IInterpolatable<T> {
  bool HasAnyData { get; }

  [Const]
  bool TryGetAtFrame(float frame, out T value);
}

public interface IKeyframeInterpolator<in TKeyframe, out T>
    where TKeyframe : IKeyframe {
  T Interpolate(TKeyframe from,
                TKeyframe to,
                float frame,
                ISharedInterpolationConfig sharedInterpolationConfig);
}