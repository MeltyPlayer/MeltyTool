using fin.math.matrix.four;

using schema.binary;

namespace granny3d {
  public class GrannySkeleton : IGrannySkeleton, IBinaryDeserializable {
    public string Name { get; private set; }
    public IList<IGrannyBone> Bones { get; } = new List<IGrannyBone>();
    public int LodType { get; private set; }

    public void Read(IBinaryReader br) {
      GrannyUtils.SubreadRef(
          br, ser => { this.Name = ser.ReadStringNT(); });

      GrannyUtils.SubreadRefToArray(br, (ser, boneCount) => {
        for (var i = 0; i < boneCount; ++i) {
          Bones.Add(ser.ReadNew<GrannyBone>());
        }
      });
    }
  }

  public class GrannyBone : IGrannyBone, IBinaryDeserializable {
    public string Name { get; private set; }
    public int ParentIndex { get; private set; }
    public IGrannyTransform LocalTransform { get; } = new GrannyTransform();
    public IFinMatrix4x4 InverseWorld4x4 { get; } = new FinMatrix4x4();
    public float LodError { get; private set; }

    public void Read(IBinaryReader br) {
      GrannyUtils.SubreadRef(
          br, ser => { this.Name = ser.ReadStringNT(); });

      this.ParentIndex = br.ReadInt32();

      (this.LocalTransform as GrannyTransform).Read(br);

      // inverse_world_4x4
      for (var y = 0; y < 4; ++y) {
        for (var x = 0; x < 4; x++) {
          this.InverseWorld4x4[x, y] = br.ReadSingle();
        }
      }

      // lod_error
      br.Position += 4;

      // extended_data
      br.Position += 2 * 8;
    }
  }
}