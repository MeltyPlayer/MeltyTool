using System.Drawing;

using fin.color;
using fin.util.enums;

using schema.binary;
using schema.binary.attributes;

namespace hm64.schema.mesh;

[BinarySchema]
public partial class TileDefinition : IBinaryDeserializable {
  private uint pointer_;

  public byte YOffset { get; set; }
  public byte FallbackH { get; set; }
  public byte RawTexIndex { get; set; }

  [SequenceLengthSource(SchemaIntegerType.BYTE)]
  public Vertex[] Vertices { get; set; }

  [Skip]
  public Face[] Faces { get; set; }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/harvestwhisperer/hm64-decomp/blob/master/tools/modding/map/blender_import.py#L574
  /// </summary>
  [ReadLogic]
  public void ReadFaces_(IBinaryReader br) {
    var faces = new LinkedList<Face>();

    var tileUvs = new (int, int)[32];
    var currentSolidColor = FinColor.FromSystemColor(Color.White);

    while (true) {
      var flags = (FaceFlags) br.ReadByte();

      if (flags.CheckFlag(FaceFlags.UNK)) {
        br.ReadByte();
        br.ReadByte();
      }

      var isTextured = flags.CheckFlag(FaceFlags.IS_TEXTURED);
      var isQuad = flags.CheckFlag(FaceFlags.IS_QUAD);
      var isLastCommand = flags.CheckFlag(FaceFlags.IS_LAST_COMMAND);

      if (isTextured) {
        for (var i = 0; i < (isQuad ? 4 : 3); ++i) {
          var v_idx = br.ReadByte();
          var s_raw = br.ReadByte();
          var t_raw = br.ReadByte();

          // corresponds to: (tileRenderingInfo[i].data2[0] << 0x16) | (tileRenderingInfo[i].data3[0] << 6)
          var u_px = s_raw * 2;
          var v_px = t_raw * 2;

          // gSPModifyVertex expects absolute texel coordinates (e.g., pixel 32, not float 0.5). Blender expects normalized coordinates (0.0 to 1.0).
          // Normalize to Full Image
          var u = u_px / 1; // t_width
          var v = v_px / 1; // t_height

          tileUvs[v_idx] = (u, v);
        }
      } else {
        currentSolidColor
            = FinColor.FromRgbBytes(br.ReadByte(),
                                    br.ReadByte(),
                                    br.ReadByte());
      }

      br.PushMemberEndianness(Endianness.LittleEndian);
      faces.AddLast(new Face {
          Color = currentSolidColor,
          IsTextured = isTextured,
          Vertices
              = UnpackTriangleIndices_(br.ReadUInt16(), ((uint) flags & 0x0C) >> 2)
      });

      (uint, uint, uint)? triangle2Indices = null;
      if (isQuad) {
        faces.AddLast(new Face {
            Color = currentSolidColor,
            IsTextured = isTextured,
            Vertices
                = UnpackTriangleIndices_(br.ReadUInt16(), (uint) flags & 0x03)
        });
      }

      br.PopEndianness();

      if (isLastCommand) {
        break;
      }
    }

    this.Faces = faces.ToArray();
  }

  private static (uint, uint, uint) UnpackTriangleIndices_(
      uint bitfield,
      uint permutationFlag) {
    var i1 = (bitfield >> 10) & 0x1F;
    var i2 = (bitfield >> 5) & 0x1F;
    var i3 = bitfield & 0x1F;

    return permutationFlag switch {
        0 => (i1, i2, i3),
        1 => (i2, i3, i1),
        _ => (i3, i1, i2)
    };
  }
}

[Flags]
public enum FaceFlags : byte {
  IS_LAST_COMMAND = 0x10,
  UNK = 0x20,
  IS_QUAD = 0x40,
  IS_TEXTURED = 0x80,
}

public class Face {
  public required (uint, uint, uint) Vertices { get; init; }
  public required IColor Color { get; init; }
  public bool IsTextured { get; init; }
}