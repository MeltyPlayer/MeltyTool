using System.Collections.Generic;

using fin.animation.interpolation;

using schema.readOnly;

namespace fin.animation.keyframes;

[GenerateReadOnly]
public partial interface IStairStepKeyframes<T>
    : IKeyframes<Keyframe<T>>, IConfiguredInterpolatable<T>;

public class StairStepKeyframes<T>(
    ISharedInterpolationConfig sharedConfig,
    IndividualInterpolationConfig<T> individualConfig = default)
    : IStairStepKeyframes<T> {
  private readonly List<Keyframe<T>> impl_
      = new(individualConfig.InitialCapacity);

  public ISharedInterpolationConfig SharedConfig => sharedConfig;
  public IndividualInterpolationConfig<T> IndividualConfig => individualConfig;

  public IReadOnlyList<Keyframe<T>> Definitions => this.impl_;
  public bool HasAnyData => this.Definitions.Count > 0;

  public void Add(Keyframe<T> keyframe) => this.impl_.AddKeyframe(keyframe);

  public bool TryGetAtFrame(float frame, out T value) {
    if (this.impl_.TryGetPrecedingKeyframe(frame,
                                           sharedConfig,
                                           out var keyframe,
                                           out _,
                                           out _)) {
      value = keyframe.ValueOut;
      return true;
    }

    if (individualConfig.DefaultValue?.Try(out value) ?? false) {
      return true;
    }

    value = default;
    return false;
  }
}