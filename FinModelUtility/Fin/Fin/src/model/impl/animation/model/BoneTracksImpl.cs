using fin.animation.interpolation;
using fin.data.indexable;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class ModelAnimationImpl {
    private readonly IndexableDictionary<IReadOnlyBone, IBoneTracks>
        boneTracks_ = new(boneCount);

    public IReadOnlyIndexableDictionary<IReadOnlyBone, IBoneTracks>
        BoneTracks => this.boneTracks_;

    public IBoneTracks AddBoneTracks(IReadOnlyBone bone)
      => this.boneTracks_[bone]
          = new BoneTracksImpl(this, this.sharedInterpolationConfig_, bone);
  }

  private partial class BoneTracksImpl(
      IAnimation animation,
      ISharedInterpolationConfig sharedConfig,
      IReadOnlyBone bone)
      : IBoneTracks {
    public override string ToString() => $"BoneTracks[{bone}]";

    public IAnimation Animation => animation;
    public IReadOnlyBone Bone => bone;

    // TODO: Add pattern for specifying WITH given tracks
  }
}