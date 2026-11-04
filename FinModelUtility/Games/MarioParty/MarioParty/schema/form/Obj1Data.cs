using fin.schema;
using fin.util.types;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L223
/// </summary>
[BinarySchema]
public sealed partial class Obj1Data : IFormChunkData {
  private ushort objCount_;

  [Unknown]
  public ushort Unk0 { get; set; }

  [Skip]
  public IObject[] Objects { get; set; }

  [ReadLogic]
  private void Read_(IBinaryReader br) {
    var baseOffset = br.Position;

    var size = br.ReadUInt16();
    var type = br.ReadByte();
    var globalIndex = br.ReadUInt16();

    var objects = this.Objects = new IObject[this.objCount_];
    for (var i = 0; i < this.objCount_; ++i) {
      switch (type) {
        case 0x10: {
          var skeletonGlobalIndex = br.ReadUInt16();
          var transform = br.ReadNew<Transform>();
          objects[i] = new Object0x10(skeletonGlobalIndex, transform);
          break;
        }
        case 0x3a: {
          var unk0 = br.ReadByte();
          var faceIndex = br.ReadUInt16();
          var faceCount = br.ReadUInt16();
          var transform = br.ReadNew<Transform>();
          objects[i] = new Object0x3a(unk0, faceIndex, faceCount, transform);
          break;
        }
        case 0x3b: {
          // TODO: Support whatever properties are in this
          objects[i] = new NoopObject {
              Type = type,
              GlobalIndex = globalIndex,
          };
          break;
        }
        case 0x3d: {
          var subObjCount = br.ReadUInt16();
          var children = br.ReadUInt16s(subObjCount);
          var transform = br.ReadNew<Transform>();
          objects[i] = new Object0x3d(children, transform);
          break;
        }
        case 0x3e: {
          // TODO: Support whatever properties are in this
          objects[i] = new NoopObject {
              Type = type,
              GlobalIndex = globalIndex,
          };
          break;
        }
        case 0x5d: {
          // TODO: Support whatever properties are in this
          objects[i] = new NoopObject {
              Type = type,
              GlobalIndex = globalIndex,
          };
          break;
        }
        case 0x61: {
          objects[i] = new Object0x61(br.ReadNew<Transform>()) {
              Type = type,
              GlobalIndex = globalIndex,
          };
          break;
        }
        default:
          throw new NotImplementedException();
      }

      br.Position = baseOffset + size - 3;
    }
  }
}

[UnionCandidate]
public interface IObject {
  byte Type { get; init; }
  ushort GlobalIndex { get; init; }
}

public abstract record BObject : IObject {
  public byte Type { get; init; }
  public ushort GlobalIndex { get; init; }
}

public sealed record NoopObject : BObject;

public sealed record Object0x10(ushort SkeletonGlobalIndex, Transform Transform)
    : BObject;

public sealed record Object0x3a(byte Unk0, ushort FaceIndex, ushort FaceCount, Transform Transform)
    : BObject;

public sealed record Object0x3d(ushort[] Children, Transform Transform)
    : BObject;

public sealed record Object0x61(Transform Transform) : BObject;