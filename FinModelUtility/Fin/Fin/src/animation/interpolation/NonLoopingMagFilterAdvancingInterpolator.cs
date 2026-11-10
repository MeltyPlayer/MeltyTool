using fin.math;
using fin.math.floats;
using fin.math.interpolation;

namespace fin.animation.interpolation;

public sealed class NonLoopingMagFilterAdvancingInterpolator<T>
    : IAdvancingInterpolator<T> {
  private readonly (bool hadValue, T value)[] cachedFrames_;
  private readonly IInterpolator<T> interpolator_;

  public NonLoopingMagFilterAdvancingInterpolator(
      MagFilterInterpolatable<T> parent,
      IInterpolator<T> interpolator) {
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
    intFrame = intFrame.Clamp(0, animationLength - 1);

    var (fromHadValue, fromValue) = this.cachedFrames_[intFrame];
    if (intFrame == animationLength - 1) {
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