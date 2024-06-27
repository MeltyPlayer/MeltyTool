using fin.schema.vector;

using schema.binary;

namespace granny3d {
  public class GrannyAnimation : IGrannyAnimation, IBinaryDeserializable {
    public string Name { get; private set; }
    public float Duration { get; private set; }
    public float TimeStep { get; private set; }
    public float Oversampling { get; private set; }

    public IList<IGrannyTrackGroup> TrackGroups { get; } =
      new List<IGrannyTrackGroup>();

    public void Read(IBinaryReader br) {
      GrannyUtils.SubreadRef(
          br, sbr => { this.Name = sbr.ReadStringNT(); });

      this.Duration = br.ReadSingle();
      this.TimeStep = br.ReadSingle();
      this.Oversampling = br.ReadSingle();

      GrannyUtils.SubreadRefToArray(br, (sbr, trackGroupCount) => {
        for (var i = 0; i < trackGroupCount; ++i) {
          GrannyUtils.SubreadRef(sbr, ssbr => {
            this.TrackGroups.Add(ssbr.ReadNew<GrannyTrackGroup>());
          });
        }
      });
    }
  }

  public class GrannyTrackGroup : IGrannyTrackGroup, IBinaryDeserializable {
    public string Name { get; private set; }

    public GrannyTransform InitialPlacement { get; } = new();
    public Vector3f LoopTranslation { get; } = new();
    public GrannyVariant ExtendedData { get; } = new();

    public void Read(IBinaryReader br) {
      GrannyUtils.SubreadRef(
          br, ser => this.Name = ser.ReadStringNT());

      // TODO: vector tracks header
      GrannyUtils.SubreadRefToArray(
          br, (sbr, count) => { });
      // TODO: transform tracks header
      GrannyUtils.SubreadRefToArray(
          br, (sbr, count) => { });
      // TODO: transform lod errors header
      GrannyUtils.SubreadRefToArray(
          br, (sbr, count) => { });
      // TODO: text tracks header
      GrannyUtils.SubreadRefToArray(
          br, (sbr, count) => { });

      this.InitialPlacement.Read(br);
      var flags = br.ReadUInt32();
      this.LoopTranslation.Read(br);
      // TODO: periodic loop ref
      GrannyUtils.SubreadRef(
          br, sbr => { });
      // TODO: root motion ref
      GrannyUtils.SubreadRef(
          br, sbr => { });
      this.ExtendedData.Read(br);
    }
  }

  public class GrannyVariant : IBinaryDeserializable {
    public void Read(IBinaryReader br) {
      // TODO: type
      GrannyUtils.SubreadRef(
          br, sbr => { });
      // TODO: object
      GrannyUtils.SubreadRef(
          br, sbr => { });
    }
  }
}