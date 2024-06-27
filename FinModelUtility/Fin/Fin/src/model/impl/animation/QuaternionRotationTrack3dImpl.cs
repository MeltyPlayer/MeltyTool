using System.Numerics;

using fin.math.interpolation;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  public class QuaternionRotationTrack3dImpl
      : InputOutputTrackImpl<Quaternion, QuaternionInterpolator>,
        IQuaternionRotationTrack3d {
    public QuaternionRotationTrack3dImpl(IAnimation animation,
                                         int initialCapacity) : base(
        animation,
        initialCapacity,
        new QuaternionInterpolator()) { }
  }
}