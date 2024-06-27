using System.Numerics;

using fin.math.interpolation;

namespace fin.model.impl {
  public partial class ModelImpl<TVertex> {
    public class CombinedPositionAxesTrack3dImpl
        : InputOutputTrackImpl<Vector3, Vector3Interpolator>,
          ICombinedPositionAxesTrack3d {
      public CombinedPositionAxesTrack3dImpl(IAnimation animation,
                                             int initialCapacity) : base(
          animation,
          initialCapacity,
          new Vector3Interpolator()) { }
    }
  }
}