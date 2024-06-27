using grezzo.schema.cmb;

using schema.binary;

namespace grezzo.schema.zsi;

public class MeshEntry : IZsiSection, IBinaryDeserializable {
  public short XMax { get; set; }
  public short ZMax { get; set; }
  public short XMin { get; set; }
  public short ZMin { get; set; }
  public int OpaqueMeshOffset { get; set; }
  public int TranslucentMeshOffset { get; set; }

  public Cmb? OpaqueMesh { get; set; }
  public Cmb? TranslucentMesh { get; set; }

  public void Read(IBinaryReader br) {
      this.XMax = br.ReadInt16();
      this.ZMax = br.ReadInt16();
      this.XMin = br.ReadInt16();
      this.ZMin = br.ReadInt16();

      this.OpaqueMeshOffset = br.ReadInt32();
      this.TranslucentMeshOffset = br.ReadInt32();

      if (this.OpaqueMeshOffset != 0) {
        this.OpaqueMesh
            = br.SubreadAt(this.OpaqueMeshOffset, sbr => sbr.ReadNew<Cmb>());
      }

      if (this.TranslucentMeshOffset != 0) {
        this.TranslucentMesh
            = br.SubreadAt(this.TranslucentMeshOffset, sbr => sbr.ReadNew<Cmb>());
      }
    }
}