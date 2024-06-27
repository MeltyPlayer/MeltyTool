using schema.binary;
using schema.binary.attributes;

namespace dat.schema.animation;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/93a906444f34951c6eed4d8c6172bba43d4ada98/HSDRaw/Common/Animation/HSD_FigaTree.cs#L14
/// </summary>
[BinarySchema]
public partial class FigaTree : IDatNode, IBinaryDeserializable {
  public int Type { get; set; }
  public int Unknown1 { get; set; }
  public float FrameCount { get; set; }
  public int TrackCountsOffset { get; set; }
  public int TrackDataOffset { get; set; }


  [Skip]
  public LinkedList<byte> TrackCounts { get; } = [];

  [Skip]
  public LinkedList<LinkedList<FigaTreeTrack>> TrackNodes { get; } = [];


  [ReadLogic]
  private void ReadTracks_(IBinaryReader br) {
      this.TrackCounts.Clear();
      this.TrackNodes.Clear();

      br.SubreadAt(
          this.TrackCountsOffset,
          sbr => {
            byte trackCount;
            while ((trackCount = sbr.ReadByte()) != byte.MaxValue) {
              this.TrackCounts.AddLast(trackCount);
            }
          });

      br.SubreadAt(
          this.TrackDataOffset,
          sbr => {
            foreach (var trackCount in TrackCounts) {
              var treeTracks = new LinkedList<FigaTreeTrack>();
              for (var i = 0; i < trackCount; i++) {
                treeTracks.AddLast(sbr.ReadNew<FigaTreeTrack>());
              }

              this.TrackNodes.AddLast(treeTracks);
            }
          });
    }
}