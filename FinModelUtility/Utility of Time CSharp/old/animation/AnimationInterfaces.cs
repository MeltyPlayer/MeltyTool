using System.Collections.Generic;

namespace UoT {
  public interface IAnimation {
    ushort FrameCount { get; }

    int PositionCount { get; }
    Vec3s GetPosition(int frameIndex);

    int TrackCount { get; }
    IAnimationTrack GetTrack(int i);
    
    FacialState GetFacialState(int i);
  }

  public interface IAnimationTrack {
    // TODO: Convert this to an enum.
    int Type { get; }
    IList<ushort> Frames { get; }
  }

  public class Vec3s {
    public short X { get; set; }
    public short Y { get; set; }
    public short Z { get; set; }
  }

  public class FacialState(EyeState eyeState, MouthState mouthState) {
    public static FacialState DEFAULT = new FacialState(default, default);

    public EyeState EyeState { get; } = eyeState;
    public MouthState MouthState { get; } = mouthState;
  }
}