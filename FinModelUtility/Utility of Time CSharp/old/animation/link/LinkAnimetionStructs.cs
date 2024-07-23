using System.Collections.Generic;

namespace UoT {
  // Structs that store animations from the "link_animetion" (sic) file.
  //
  // Based on the structs at:
  // https://wiki.cloudmodding.com/oot/Animation_Format#C_code

  public class LinkAnimetion(
      ushort frameCount,
      IList<LinkAnimetionTrack> tracks,
      IList<Vec3s> positions,
      IList<FacialState> facialStates)
      : IAnimation {
    public ushort FrameCount { get; set; } = frameCount;

    public int PositionCount => positions.Count;
    public Vec3s GetPosition(int i) => positions[i];

    public int TrackCount => tracks.Count;
    public IAnimationTrack GetTrack(int i) => tracks[i];
    
    public FacialState GetFacialState(int i) => facialStates![i];
  }

  public class LinkAnimetionTrack(int type, IList<ushort> frames)
      : IAnimationTrack {
    public int Type { get; } = type; // 0 = constant, 1 = keyframe
    public IList<ushort> Frames { get; } = frames;
  }

  // TODO: Use below structs instead.
  /*public struct LinkAnimetionFace {
    public byte Mouth;
    public byte Eyes;
  }

  public struct LinkAnimetionFrame {
    public Vec3s RootTranslation;
    public Vec3s[] LimbRotations; // Should have length of 21.
    public LinkAnimetionFace FacialExpression;
  }*/
}
