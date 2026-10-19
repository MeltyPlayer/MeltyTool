using System;

using fin.math;
using fin.math.floats;
using fin.math.interpolation;

namespace fin.animation.interpolation;

public sealed class MagFilterAdvancingInterpolator<T>
    : IAdvancingInterpolator<T> {
  private readonly (bool hadValue, T value)[] cachedFrames_;

  private readonly bool looping_;
  private readonly IInterpolator<T> interpolator_;

  public MagFilterAdvancingInterpolator(MagFilterInterpolatable<T> parent,
                                        IInterpolator<T> interpolator) {
    this.looping_ = parent.Looping;
    this.interpolator_ = interpolator;

    var advancingInterpolator = parent.Impl.GetAdvancingInterpolator();
    this.cachedFrames_ = new (bool hadValue, T value)[parent.AnimationLength];
    for (var i = 0; i < this.cachedFrames_.Length; ++i) {
      var hadValue = advancingInterpolator.TryGetAtFrame(i, out var value);
      this.cachedFrames_[i] = (hadValue, value);
    }
  }

  public bool TryGetAtFrame(float frame, out T value) {
    var animationLength = this.cachedFrames_.Length;
    
    var intFrame = (int) frame;
    if (this.looping_) {
      intFrame = intFrame.Mod0ToMax(animationLength);
    } else {
      intFrame = intFrame.Clamp(0, animationLength - 1);
    }
    
    var (fromHadValue, fromValue) = this.cachedFrames_[intFrame];
    if (!this.looping_ && intFrame == animationLength - 1) {
      value = fromValue;
      return fromHadValue;
    }

    var frac = frame - intFrame;
    if (frac.IsRoughly0()) {
      value = fromValue;
      return fromHadValue;
    }

    var (toHadValue, toValue)
        = this.cachedFrames_[(intFrame + 1) % animationLength];
    if (fromHadValue && toHadValue) {
      value = this.interpolator_.Interpolate(fromValue, toValue, frac);
      return true;
    }

    value = default!;
    return false;
  }
}