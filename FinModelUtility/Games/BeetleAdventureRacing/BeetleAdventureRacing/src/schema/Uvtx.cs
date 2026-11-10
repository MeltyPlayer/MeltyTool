using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace bar.schema;

/// <summary>
///   "TeXture"
/// 
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/ParsedFiles/UVTX.ts
/// </summary>
[BinarySchema]
public sealed partial class Uvtx : IBinaryDeserializable {
  private ushort TexelDataSize { get; set; }
  public ushort DlCommandCount { get; set; }

  public Vector2 TexScrollAnim0Velocity { get; set; }
  public Vector2 TexScrollAnim1Velocity { get; set; }
  public Vector2 TexScrollAnim1Offset { get; set; }

  [RSequenceLengthSource(nameof(TexelDataSize))]
  public byte[] TexelData { get; private set; }

  [Skip]
  public int DlCommandSize => this.DlCommandCount * 8;

  [RSequenceLengthSource(nameof(DlCommandSize))]
  public byte[] DlCommandsData { get; private set; }

  public ushort Width { get; set; }
  public ushort Height { get; set; }

  public byte UnkByte3 { get; set; }
  public byte UnkByte4 { get; set; }
  public byte UnkByte5 { get; set; }

  public uint FlagsAndIndex { get; set; }
  public ushort OtherUvtxIndex { get; set; }

  public ushort Unk6 { get; set; }
  public byte Unk1 { get; set; }
  public byte Unk8 { get; set; }
  public byte Unk9 { get; set; }
  public byte Unk10 { get; set; }
  public byte Unk11 { get; set; }
  public uint Unk12 { get; set; }
  public byte BlendAlpha { get; set; }
  public byte LevelCount { get; set; }

  [Skip]
  public byte[][]? PalettesData { get; set; }

  [ReadLogic]
  private void ReadPalettes_(IBinaryReader br) {
    if (this.Unk1 != 0) {
      this.PalettesData = null;
    } else {
      this.PalettesData = new byte[this.LevelCount][];
      for (var i = 0; i < this.LevelCount; ++i) {
        this.PalettesData[i] = br.ReadBytes(32);
      }
    }
  }
}