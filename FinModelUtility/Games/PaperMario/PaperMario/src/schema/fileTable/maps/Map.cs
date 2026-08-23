using schema.binary;
using schema.binary.attributes;

namespace pm.schema.fileTable.maps;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PaperMario64/tools/extractor.ts#L76
/// </summary>
[BinarySchema]
public sealed partial class Map : IBinaryConvertible {
  public uint MapNameRamAddress { get; set; }
  public uint HeaderRamAddress { get; set; }
  public uint RomOverlayStartOffset { get; set; }
  public uint RomOverlayEndOffset { get; set; }
  public uint RomOverlayDestOffset { get; set; }
  public uint BackgroundNameRamAddress { get; set; }
  public uint InitCodeRamAddress { get; set; }
  public uint MapFlags { get; set; }

  [Skip]
  public uint MapNameOffset
    => MapTableUtil.ConvertRamAddressToRomOffset(this.MapNameRamAddress);

  [Skip]
  public uint BackgroundNameOffset
    => this.BackgroundNameRamAddress != 0
        ? MapTableUtil.ConvertRamAddressToRomOffset(
            this.BackgroundNameRamAddress)
        : 0;

  [RAtPosition(nameof(MapNameOffset))]
  [StringLengthSource(0x20)]
  public string MapName { get; set; }

  [RAtPositionOrNull(nameof(BackgroundNameOffset))]
  [StringLengthSource(0x10)]
  public string? BackgroundName { get; set; }

  public override string ToString() => $"{this.MapName}";
}