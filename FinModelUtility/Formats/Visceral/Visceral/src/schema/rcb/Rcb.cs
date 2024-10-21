using fin.schema;
using fin.schema.matrix;

using schema.binary;

namespace visceral.schema.rcb;

public partial class Rcb : IBinaryDeserializable {
  public uint MainBnkId { get; set; }

  public const float SCALE = 100;

  public IReadOnlyList<Skeleton> Skeletons { get; private set; }

  [Unknown]
  public void Read(IBinaryReader br) {
    var fileLength = br.ReadUInt32();

    br.Position += 4;

    this.MainBnkId = br.ReadUInt32();

    br.Position += 12;

    var skeletonCount = br.ReadUInt32();

    var dataOffset = br.ReadUInt32();
    var unk4 = br.ReadUInt32();
    var dataOffset2 = br.ReadUInt32();
    br.Position += 16;

    var unk5 = br.ReadUInt32();
    var unk6 = br.ReadUInt32();
    var dataOffset3 = br.ReadUInt32();

    br.Position = dataOffset;
    this.Skeletons = br.ReadNews<Skeleton>((int) skeletonCount);
  }

  public class Skeleton : IBinaryDeserializable {
    public uint BnkId { get; private set; }
    public string SkeletonName { get; private set; }
    public IReadOnlyList<int> BoneParentIdMap { get; private set; }
    public IReadOnlyList<Bone> Bones { get; private set; }

    [Unknown]
    public void Read(IBinaryReader br) {
      // Read skeleton header
      this.BnkId = br.ReadUInt32();

      this.SkeletonName =
          br.SubreadAt(br.ReadUInt32(), ser => ser.ReadStringNT());
      var boneCount = br.ReadUInt32();
      var boneIdTableOffset = br.ReadUInt32();
      var boneStart = br.ReadUInt32();
      var ukwTableOffset = br.ReadUInt32();

      // Get bone parent table
      br.SubreadAt(
          boneIdTableOffset,
          ser => {
            ser.Position = boneIdTableOffset;

            var boneParentIdMap = new int[boneCount];
            for (var i = 0; i < boneCount; i++) {
              boneParentIdMap[i] = ser.ReadInt32();
              ser.Position += 12;
            }

            this.BoneParentIdMap = boneParentIdMap;
          });

      // Read bone matrices
      this.Bones = br.SubreadAt(
          boneStart,
          sbr => sbr.ReadNews<Bone>((int) boneCount));
    }
  }

  [BinarySchema]
  public partial class Bone : IBinaryConvertible {
    public Matrix4x4f Matrix { get; } = new();
  }
}