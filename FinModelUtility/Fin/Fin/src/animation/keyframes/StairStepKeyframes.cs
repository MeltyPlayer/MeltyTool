using System;
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
                                           individualConfig,
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

  public void GetAllFrames(Span<T> frames) {
    T defaultValue = default!;
    individualConfig.DefaultValue?.Try(out defaultValue);
    if (sharedConfig.Looping) {
      defaultValue = this.impl_[^1].ValueOut;
    }

    frames.Fill(defaultValue);
    if (this.impl_.Count == 0) {
      return;
    }

    var f = frames.Length - 1;
    for (var k = this.impl_.Count - 1; k >= 0; --k) {
      var keyframe = this.impl_[k];
      while (f >= keyframe.Frame) {
        frames[f--] = keyframe.ValueOut;
      }
    }
  }
}