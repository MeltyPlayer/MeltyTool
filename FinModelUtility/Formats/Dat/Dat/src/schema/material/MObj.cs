using dat.schema.texture;

using schema.binary;
using schema.binary.attributes;

namespace dat.schema.material;

[Flags]
public enum RenderMode : int {
  CONSTANT = (1 << 0),
  VERTEX = (1 << 1),
  BOTH = CONSTANT | VERTEX,
  DIFFUSE = (1 << 2),
  SPECULAR = (1 << 3),
  TEX0 = (1 << 4),
  TEX1 = (1 << 5),
  TEX2 = (1 << 6),
  TEX3 = (1 << 7),
  TEX4 = (1 << 8),
  TEX5 = (1 << 9),
  TEX6 = (1 << 10),
  TEX7 = (1 << 11),
  TOON = (1 << 12),

  //ALPHA_COMPAT = (0 << 13),
  ALPHA_MAT = (1 << 13),
  ALPHA_VTX = (2 << 13),
  ALPHA_BOTH = (3 << 13),
  ZOFST = (1 << 24),
  EFFECT = (1 << 25),
  SHADOW = (1 << 26),
  ZMODE_ALWAYS = (1 << 27),
  DF_ALL = (1 << 28),
  NO_ZUPDATE = (1 << 29),
  XLU = (1 << 30),
  USER = (1 << 31),
}

/// <summary>
///   Material object.
///
///   Shamelessly copied from:
///   https://github.com/jam1garner/Smash-Forge/blob/c0075bca364366bbea2d3803f5aeae45a4168640/Smash%20Forge/Filetypes/Melee/DAT.cs#L1256
/// </summary>
[BinarySchema]
public partial class MObj : IBinaryDeserializable {
  public uint StringOffset { get; set; }
  public RenderMode RenderMode { get; set; }
  public uint FirstTObjOffset { get; set; }
  public uint MaterialOffset { get; private set; }
  public uint Unk3 { get; set; }
  public uint PeDescOffset { get; set; }


  [Skip]
  public string? Name { get; set; }

  [RAtPositionOrNull(nameof(FirstTObjOffset))]
  public TObj? FirstTObj { get; set; }

  [RAtPositionOrNull(nameof(MaterialOffset))]
  public DatMaterial? Material { get; set; }

  [RAtPositionOrNull(nameof(PeDescOffset))]
  public PeDesc? PeDesc { get; set; }

  [Skip]
  public IEnumerable<(uint, TObj)> TObjsAndOffsets {
    get {
        var tObjOffset = this.FirstTObjOffset;
        var tObj = this.FirstTObj;

        while (tObj != null) {
          yield return (tObjOffset, tObj);

          tObjOffset = tObj.NextTObjOffset;
          tObj = tObj.NextTObj;
        }
      }
  }
}