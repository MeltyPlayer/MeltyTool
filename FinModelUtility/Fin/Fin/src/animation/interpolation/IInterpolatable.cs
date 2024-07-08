using fin.animation.keyframes;

using schema.readOnly;

namespace fin.animation.interpolation;

public class SharedInterpolationConfig : ISharedInterpolationConfig {
  public int AnimationLength { get; set; }
  public bool Looping { get; set; }
}

public interface ISharedInterpolationConfig {
  int AnimationLength { get; }
  bool Looping { get; }
}

public interface IInterpolatable<T> {
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