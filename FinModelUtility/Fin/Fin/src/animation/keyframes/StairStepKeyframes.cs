using System.Collections.Generic;

using fin.animation.interpolation;

namespace fin.animation.keyframes;

public interface IStairStepKeyframes<T>
    : IKeyframes<Keyframe<T>>, IInterpolatable<T>;

public class StairStepKeyframes<T>(
    ISharedInterpolationConfig sharedConfig,
    IndividualInterpolationConfig<T> individualConfig = default)
    : IStairStepKeyframes<T> {
  private readonly List<Keyframe<T>> impl_
      = new(individualConfig.InitialCapacity);

  public IReadOnlyList<Keyframe<T>> Definitions => this.impl_;

  public void Add(Keyframe<T> keyframe) => this.impl_.AddKeyframe(keyframe);

  public bool TryGetAtFrame(float frame, out T value) {
    if (this.impl_.TryGetPrecedingKeyframe(frame,
                                           sharedConfig.Looping,
                                           out var keyframe,
                                           out _)) {
      value = keyframe.Value;
      return true;
    }

    if (individualConfig.DefaultValue?.Try(out value) ?? false) {
      return true;
    }

    value = default;
    return false;
  }
}