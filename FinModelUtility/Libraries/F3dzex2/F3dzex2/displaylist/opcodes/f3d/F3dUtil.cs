using f3dzex2.image;

using fin.math;
using fin.math.fixedPoint;

using schema.binary;


namespace f3dzex2.displaylist.opcodes.f3d;

public static class F3dUtil {
  public static LoadBlockOpcodeCommand ParseLoadBlockOpcodeCommand(
      IBinaryReader br) {
    br.Position -= 1;

    var first = br.ReadUInt32();
    var second = br.ReadUInt32();

    var uls = FixedPointFloatUtil.Convert16(
        (ushort) first.ExtractFromRight(12, 12),
        false,
        10,
        2);
    var ult = FixedPointFloatUtil.Convert16(
        (ushort) first.ExtractFromRight(0, 12),
        false,
        10,
        2);

    var tileDescriptor =
        (TileDescriptorIndex) second.ExtractFromRight(24, 4);
    var texels = (ushort) second.ExtractFromRight(12, 12);
    var dxt = (ushort) second.ExtractFromRight(0, 12);

    return new LoadBlockOpcodeCommand {
        TileDescriptorIndex = tileDescriptor,
        Uls = uls,
        Ult = ult,
        Texels = texels,
        Dxt = dxt,
    };
  }

  public static SetTileOpcodeCommand ParseSetTileOpcodeCommand(
      IBinaryReader br) {
    br.Position -= 1;
    var first = br.ReadUInt32();
    var second = br.ReadUInt32();

    var colorFormat =
        (N64ColorFormat) BitLogic.ExtractFromRight(first, 21, 3);
    var bitSize =
        (BitsPerTexel) BitLogic.ExtractFromRight(first, 19, 2);
    var num64BitValuesPerRow =
        (ushort) BitLogic.ExtractFromRight(first, 9, 9);
    var offsetOfTextureInTmem =
        (ushort) BitLogic.ExtractFromRight(first, 0, 9);

    var tileDescriptor =
        (TileDescriptorIndex) BitLogic.ExtractFromRight(second, 24, 3);
    var palette = (ushort) BitLogic.ExtractFromRight(second, 20, 4);

    var wrapModeT = (F3dWrapMode) second.ExtractFromRight(18, 2);
    var maskT = (ushort) second.ExtractFromRight(14, 4);
    var shiftT = (ushort) second.ExtractFromRight(10, 4);

    var wrapModeS = (F3dWrapMode) second.ExtractFromRight(8, 2);
    var maskS = (ushort) second.ExtractFromRight(4, 4);
    var shiftS = (ushort) second.ExtractFromRight(0, 4);

    return new SetTileOpcodeCommand {
        TileDescriptorIndex = tileDescriptor,
        ColorFormat = colorFormat,
        BitsPerTexel = bitSize,
        Palette = palette,
        WrapModeS = wrapModeS,
        MaskS = maskS,
        ShiftS = shiftS,
        WrapModeT = wrapModeT,
        MaskT = maskT,
        ShiftT = shiftT,
        Num64BitValuesPerRow = num64BitValuesPerRow,
        OffsetOfTextureInTmem = offsetOfTextureInTmem,
    };
  }
}