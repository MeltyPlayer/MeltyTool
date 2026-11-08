using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class ModelAnimationImpl {
    public IReadOnlyMorphTrack? MorphTrack { get; private set; }

    public IMorphTrack UseMorphTrack() {
      var morphTrack = new MorphTrackImpl(this.sharedInterpolationConfig_);
      this.MorphTrack = morphTrack;
      return morphTrack;
    }
  }

  private sealed class MorphTrackImpl(ISharedInterpolationConfig sharedConfig)
      : IMorphTrack {
    public IStairStepKeyframes<Keyframe<IReadOnlyMorphTarget>> Keyframes {
      get;
    } = new StairStepKeyframes<Keyframe<IReadOnlyMorphTarget>>(sharedConfig);
  }
}