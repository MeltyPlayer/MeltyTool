using System.Collections.Generic;

namespace UoT {
  // Structs that store animations from the "link_animetion" (sic) file.
  //
  // Based on the structs at:
  // https://wiki.cloudmodding.com/oot/Animation_Format#C_code

  public class LinkAnimetion : IAnimation {
    private readonly IList<LinkAnimetionTrack> tracks_;
    private readonly IList<Vec3s> positions_;
    private readonly IList<FacialState> facialStates_;

    public LinkAnimetion(ushort frameCount,
                         IList<LinkAnimetionTrack> tracks,
                         IList<Vec3s> positions,
                         IList<FacialState> facialStates) {
      this.FrameCount = frameCount;
      this.tracks_ = tracks;
      this.positions_ = positions;
      this.facialStates_ = facialStates;
    }

    public ushort FrameCount { get; set; }

    public int PositionCount => this.positions_.Count;
    public Vec3s GetPosition(int i) => this.positions_[i];

    public int TrackCount => this.tracks_.Count;
    public IAnimationTrack GetTrack(int i) => this.tracks_[i];
    
    public FacialState GetFacialState(int i) => this.facialStates_![i];
  }

  public class LinkAnimetionTrack : IAnimationTrack {
    public LinkAnimetionTrack(int type, IList<ushort> frames) {
      this.Type = type;
      this.Frames = frames;
    }

    public int Type { get; } // 0 = constant, 1 = keyframe
    public IList<ushort> Frames { get; }
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
