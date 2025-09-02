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
    IndividualInterpolationConfig<T>? individualConfig = null)
    : IStairStepKeyframes<T> {
  private readonly List<Keyframe<T>> impl_
      = new(individualConfig?.InitialCapacity ?? 0);

  public ISharedInterpolationConfig SharedConfig => sharedConfig;
  public IndividualInterpolationConfig<T> IndividualConfig
    => individualConfig ?? IndividualInterpolationConfig<T>.DEFAULT;

  public IReadOnlyList<Keyframe<T>> Definitions => this.impl_;
  public bool HasAnyData => this.Definitions.Count > 0;

  public void Add(Keyframe<T> keyframe) => this.impl_.AddKeyframe(keyframe);

  public bool TryGetAtFrame(float frame, out T value) {
    if (this.impl_.TryGetPrecedingKeyframe(frame,
                                           sharedConfig,
                                           this.IndividualConfig,
                                           out var keyframe,
                                           out _,
                                           out _)) {
      value = keyframe.ValueOut;
      return true;
    }

    if (this.IndividualConfig.DefaultValue?.Try(out value) ?? false) {
      return true;
    }

    value = default!;
    return false;
  }

  public void GetAllFrames(Span<T> dst) {
    T defaultValue = default!;
    this.IndividualConfig.DefaultValue?.Try(out defaultValue);
    if (sharedConfig.Looping) {
      defaultValue = this.impl_[^1].ValueOut;
    }

    dst.Fill(defaultValue);
    if (this.impl_.Count == 0) {
      return;
    }

    var f = dst.Length - 1;
    for (var k = this.impl_.Count - 1; k >= 0; --k) {
      var keyframe = this.impl_[k];
      while (f >= keyframe.Frame) {
        dst[f--] = keyframe.ValueOut;
      }
    }
  }
}