using schema.binary;
using schema.binary.attributes;

namespace pm.schema.maps;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PaperMario64/tools/extractor.ts#L64
/// </summary>
[BinarySchema]
public sealed partial class Area : IBinaryConvertible {
  public uint MapCount { get; set; }
  public uint MapTableRamAddress { get; set; }
  public uint AreaNameRamAddress { get; set; }
  public uint AreaNameSjisRamAddress  { get; set; }

  [Skip]
  public uint MapTableOffset
    => MapTableUtil.ConvertRamAddressToRomOffset(this.MapTableRamAddress);

  [Skip]
  public uint AreaNameOffset
    => MapTableUtil.ConvertRamAddressToRomOffset(this.AreaNameRamAddress);

  [Skip]
  public uint AreaNameSjisOffset
    => MapTableUtil.ConvertRamAddressToRomOffset(this.AreaNameSjisRamAddress);

  [RAtPosition(nameof(MapTableOffset))]
  [RSequenceLengthSource(nameof(MapCount))]
  public Map[] Maps { get; set; }

  [RAtPosition(nameof(AreaNameOffset))]
  [StringLengthSource(0x10)]
  public string AreaName { get; set; }

  [RAtPosition(nameof(AreaNameSjisOffset))]
  [StringEncoding(StringEncodingType.SHIFT_JIS)]
  [StringLengthSource(0x20)]
  public string AreaNameSjis { get; set; }

  public override string ToString() => $"{this.AreaName} / {this.AreaNameSjis}";
}
