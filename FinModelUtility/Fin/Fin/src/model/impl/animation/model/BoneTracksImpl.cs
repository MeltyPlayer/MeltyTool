using System;

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

    public IRotationTrack3d? Rotations { get; private set; }

    public IQuaternionRotationTrack3d UseQuaternionRotationTrack(
        int initialCapacity)
      => (IQuaternionRotationTrack3d) (this.Rotations =
          new QuaternionRotationTrack3dImpl(this.Animation, initialCapacity));

    public IQuaternionAxesRotationTrack3d UseQuaternionAxesRotationTrack()
      => (IQuaternionAxesRotationTrack3d) (this.Rotations =
          new QuaternionAxesRotationTrack3dImpl(this.Animation, this.Bone));


    public IEulerRadiansRotationTrack3d UseEulerRadiansRotationTrack(
        int initialCapacity)
      => this.UseEulerRadiansRotationTrack(initialCapacity,
                                           initialCapacity,
                                           initialCapacity);

    public IEulerRadiansRotationTrack3d UseEulerRadiansRotationTrack(
        int initialXCapacity,
        int initialYCapacity,
        int initialZCapacity) {
      Span<int> initialAxisCapacities = stackalloc int[3];
      initialAxisCapacities[0] = initialXCapacity;
      initialAxisCapacities[1] = initialYCapacity;
      initialAxisCapacities[2] = initialZCapacity;

      return (IEulerRadiansRotationTrack3d) (this.Rotations =
          new EulerRadiansRotationTrack3dImpl(
              this.Animation,
              this.Bone,
              initialAxisCapacities));
    }


    // TODO: Add pattern for specifying WITH given tracks
  }
}