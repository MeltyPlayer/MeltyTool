using System;

using f3dzex2.combiner;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.math;
using fin.schema;

using schema.binary;

namespace f3dzex2.displaylist.opcodes.f3dzex2;

public class F3dzex2OpcodeParser : IOpcodeParser {
  public IOpcodeCommand Parse(IReadOnlyN64Memory n64Memory,
                              IDisplayListReader dlr,
                              SchemaBinaryReader br) {
    var baseOffset = br.Position;
    var opcode = (F3dzex2Opcode) br.ReadByte();
    var opcodeCommand = ParseOpcodeCommand_(n64Memory, dlr, br, opcode);
    br.Position = baseOffset + GetCommandLength_(opcode);
    return opcodeCommand;
  }

  public DisplayListType Type => DisplayListType.F3DZEX2;

  [Unknown]
  private IOpcodeCommand ParseOpcodeCommand_(IReadOnlyN64Memory n64Memory,
                                             IDisplayListReader dlr,
                                             SchemaBinaryReader br,
                                             F3dzex2Opcode opcode) {
    switch (opcode) {
      case F3dzex2Opcode.G_NOOP:
        return new NoopOpcodeCommand();
      case F3dzex2Opcode.G_DL: {
        var storeReturnAddress = br.ReadByte() == 0;
        br.AssertUInt16(0);
        var address = br.ReadUInt32();
        return new DlOpcodeCommand {
            PossibleBranches =
                dlr.ReadPossibleDisplayLists(n64Memory, this, address),
            PushCurrentDlToStack = storeReturnAddress
        };
      }
      case F3dzex2Opcode.G_ENDDL:
        return new EndDlOpcodeCommand();
      case F3dzex2Opcode.G_VTX: {
        var numVerticesToLoad = (byte) (br.ReadUInt16() >> 4);
        var indexToBeginStoringVertices =
            (byte) ((br.ReadByte() >> 1) - numVerticesToLoad);
        using var sbr = n64Memory.OpenAtSegmentedAddress(br.ReadUInt32());
        return new VtxOpcodeCommand {
            IndexToBeginStoringVertices = indexToBeginStoringVertices,
            Vertices = sbr.ReadNews<F3dVertex>(numVerticesToLoad),
        };
      }
      case F3dzex2Opcode.G_TRI1: {
        return new Tri1OpcodeCommand {
            VertexIndexA = (byte) (br.ReadByte() >> 1),
            VertexIndexB = (byte) (br.ReadByte() >> 1),
            VertexIndexC = (byte) (br.ReadByte() >> 1),
        };
      }
      case F3dzex2Opcode.G_TRI2: {
        var a0 = (byte) (br.ReadByte() >> 1);
        var b0 = (byte) (br.ReadByte() >> 1);
        var c0 = (byte) (br.ReadByte() >> 1);

        var unk0 = br.ReadByte();
        var a1 = (byte) (br.ReadByte() >> 1);
        var b1 = (byte) (br.ReadByte() >> 1);
        var c1 = (byte) (br.ReadByte() >> 1);

        return new Tri2OpcodeCommand {
            VertexIndexA0 = a0,
            VertexIndexB0 = b0,
            VertexIndexC0 = c0,
            VertexIndexA1 = a1,
            VertexIndexB1 = b1,
            VertexIndexC1 = c1,
        };
      }
      case F3dzex2Opcode.G_TEXTURE: {
        br.AssertByte(0);

        var mipmapLevelsAndTileDescriptor = br.ReadByte();
        var tileDescriptor =
            (TileDescriptorIndex) BitLogic.ExtractFromRight(
                mipmapLevelsAndTileDescriptor,
                0,
                3);
        var maximumNumberOfMipmaps =
            (byte) BitLogic.ExtractFromRight(mipmapLevelsAndTileDescriptor,
                                             3,
                                             3);
        var newTileDescriptorState =
            (TileDescriptorState) (br.ReadByte() >> 1);
        var horizontalScale = br.ReadUInt16();
        var verticalScale = br.ReadUInt16();

        return new TextureOpcodeCommand {
            TileDescriptorIndex = tileDescriptor,
            NewTileDescriptorState = newTileDescriptorState,
            HorizontalScaling = horizontalScale,
            VerticalScaling = verticalScale,
            MaximumNumberOfMipmaps = maximumNumberOfMipmaps,
        };
      }
      case F3dzex2Opcode.G_POPMTX: {
        br.AssertByte(0x38);
        br.AssertByte(0);
        br.AssertByte(2);

        var numMatrices = br.ReadUInt32() / 64;
        return new PopMtxOpcodeCommand { NumberOfMatrices = numMatrices };
      }
      case F3dzex2Opcode.G_SETCOMBINE: {
        //           aaaa cccc ceee gggi iiik kkkk 
        // bbbb jjjj mmmo oodd dfff hhhl llnn nppp

        var first = br.ReadUInt24();
        var second = br.ReadUInt32();

        var a = BitLogic.ExtractFromRight(first, 20, 4);
        var c = BitLogic.ExtractFromRight(first, 15, 5);
        var e = BitLogic.ExtractFromRight(first, 12, 3);
        var g = BitLogic.ExtractFromRight(first, 9, 3);
        var i = BitLogic.ExtractFromRight(first, 5, 4);
        var k = BitLogic.ExtractFromRight(first, 0, 5);

        var b = BitLogic.ExtractFromRight(second, 28, 4);
        var j = BitLogic.ExtractFromRight(second, 24, 4);
        var m = BitLogic.ExtractFromRight(second, 21, 3);
        var o = BitLogic.ExtractFromRight(second, 18, 3);
        var d = BitLogic.ExtractFromRight(second, 15, 3);
        var f = BitLogic.ExtractFromRight(second, 12, 3);
        var h = BitLogic.ExtractFromRight(second, 9, 3);
        var l = BitLogic.ExtractFromRight(second, 6, 3);
        var n = BitLogic.ExtractFromRight(second, 3, 3);
        var p = BitLogic.ExtractFromRight(second, 0, 3);

        return new SetCombineOpcodeCommand {
            CombinerCycleParams0 = new CombinerCycleParams {
                ColorMuxA = GetColorMuxA_(a),
                ColorMuxB = GetColorMuxB_(b),
                ColorMuxC = GetColorMuxC_(c),
                ColorMuxD = GetColorMuxD_(d),
                AlphaMuxA = this.GetAlphaMuxABD_(e),
                AlphaMuxB = this.GetAlphaMuxABD_(f),
                AlphaMuxC = this.GetAlphaMuxC_(g),
                AlphaMuxD = this.GetAlphaMuxABD_(h),
            },
            CombinerCycleParams1 = new CombinerCycleParams {
                ColorMuxA = GetColorMuxA_(i),
                ColorMuxB = GetColorMuxB_(j),
                ColorMuxC = GetColorMuxC_(k),
                ColorMuxD = GetColorMuxD_(l),
                AlphaMuxA = this.GetAlphaMuxABD_(m),
                AlphaMuxB = this.GetAlphaMuxABD_(n),
                AlphaMuxC = this.GetAlphaMuxC_(o),
                AlphaMuxD = this.GetAlphaMuxABD_(p),
            }
        };
      }
      case F3dzex2Opcode.G_SETTIMG: {
        N64ImageParser.SplitN64ImageFormat(br.ReadByte(),
                                           out var colorFormat,
                                           out var bitSize);

        var args = br.ReadUInt24();
        var width = (ushort) ((args >> 12) + 1);

        return new SetTimgOpcodeCommand {
            ColorFormat = colorFormat,
            BitsPerTexel = bitSize,
            Width = width,
            TextureSegmentedAddress = br.ReadUInt32(),
        };
      }
      case F3dzex2Opcode.G_SETTILE: {
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
        var wrapModeT =
            (F3dWrapMode) BitLogic.ExtractFromRight(second, 18, 2);
        var wrapModeS = (F3dWrapMode) BitLogic.ExtractFromRight(second, 8, 2);

        return new SetTileOpcodeCommand {
            TileDescriptorIndex = tileDescriptor,
            ColorFormat = colorFormat,
            BitsPerTexel = bitSize,
            WrapModeT = wrapModeT,
            WrapModeS = wrapModeS,
            Num64BitValuesPerRow = num64BitValuesPerRow,
            OffsetOfTextureInTmem = offsetOfTextureInTmem,
        };
      }
      case F3dzex2Opcode.G_SETPRIMCOLOR: {
        var lodInfo = br.ReadUInt24();
        return new SetPrimColorOpcodeCommand {
            R = br.ReadByte(),
            G = br.ReadByte(),
            B = br.ReadByte(),
            A = br.ReadByte(),
        };
      }
      case F3dzex2Opcode.G_SETTILESIZE: {
        br.Position += 3;

        var tileDescriptor = (TileDescriptorIndex) br.ReadByte();
        var lr = br.ReadUInt24();
        var lrs = (ushort) (lr >> 12);
        var lrt = (ushort) (lr & 0xFFF);

        br.ReadByte();
        var ul = br.ReadUInt24();
        var uls = (ushort) (ul >> 12);
        var ult = (ushort) (ul & 0xFFF);

        var width = (ushort) (lrs - uls + 1);
        var height = (ushort) (lrt - ult + 1);

        return new SetTileSizeOpcodeCommand {
            TileDescriptorIndex = tileDescriptor,
            Uls = uls,
            Ult = ult,
            Width = width,
            Height = height,
        };
      }
      case F3dzex2Opcode.G_LOADBLOCK: {
        br.Position += 3;

        var tileDescriptor = (TileDescriptorIndex) br.ReadByte();
        var texelsAndDxt = br.ReadUInt24();
        var texels = texelsAndDxt >> 12;

        return new LoadBlockOpcodeCommand {
            TileDescriptorIndex = tileDescriptor, Texels = (ushort) texels,
        };
      }
      case F3dzex2Opcode.G_GEOMETRYMODE: {
        return new GeometryModeOpcodeCommand {
            FlagsToDisable = (GeometryMode) ((~br.ReadUInt24()) & 0xFFFFFF),
            FlagsToEnable = (GeometryMode) (br.ReadUInt32() & 0xFFFFFF),
        };
      }
      case F3dzex2Opcode.G_MTX: {
        br.AssertUInt16(0x3800);
        var mtxParams = (byte) (br.ReadByte() ^ 0x01);
        var address = br.ReadUInt32();
        return new MtxOpcodeCommand {
            Params = mtxParams, RamAddress = address,
        };
      }
      case F3dzex2Opcode.G_LOADTLUT: {
        br.AssertUInt24(0);

        var tileDescriptor = (TileDescriptorIndex) br.ReadByte();

        var rawNumColorsToLoad = br.ReadUInt16() >> 4;
        var numColorsToLoad = (rawNumColorsToLoad >> 2) + 1;

        return new LoadTlutOpcodeCommand {
            TileDescriptorIndex = tileDescriptor,
            NumColorsToLoad = (ushort) numColorsToLoad,
        };
      }
      case F3dzex2Opcode.G_MODIFYVTX:
        return new ModifyVtxOpcodeCommand();
      // TODO: Especially implement these
      case F3dzex2Opcode.G_SETCIMG:
      case F3dzex2Opcode.G_SETZIMG:
      // TODO: Implement these
      case F3dzex2Opcode.G_SETOTHERMODE_L:
      case F3dzex2Opcode.G_SETOTHERMODE_H:
      case F3dzex2Opcode.G_CULLDL:
      case F3dzex2Opcode.G_BRANCH_Z:
        return new NoopOpcodeCommand();
      case F3dzex2Opcode.G_RDPPIPESYNC:
      case F3dzex2Opcode.G_RDPTILESYNC:
      case F3dzex2Opcode.G_RDPFULLSYNC:
      case F3dzex2Opcode.G_RDPLOADSYNC:
        return new NoopOpcodeCommand();
      default:
        throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null);
    }
  }

  private int GetCommandLength_(F3dzex2Opcode opcode) {
    switch (opcode) {
      case F3dzex2Opcode.G_NOOP:
      case F3dzex2Opcode.G_VTX:
      case F3dzex2Opcode.G_MODIFYVTX:
      case F3dzex2Opcode.G_CULLDL:
      case F3dzex2Opcode.G_TRI1:
      case F3dzex2Opcode.G_TRI2:
      case F3dzex2Opcode.G_QUAD:
      case F3dzex2Opcode.G_SPECIAL_3:
      case F3dzex2Opcode.G_SPECIAL_2:
      case F3dzex2Opcode.G_SPECIAL_1:
      case F3dzex2Opcode.G_DMA_IO:
      case F3dzex2Opcode.G_TEXTURE:
      case F3dzex2Opcode.G_POPMTX:
      case F3dzex2Opcode.G_GEOMETRYMODE:
      case F3dzex2Opcode.G_MTX:
      case F3dzex2Opcode.G_MOVEWORD:
      case F3dzex2Opcode.G_MOVEMEM:
      case F3dzex2Opcode.G_DL:
      case F3dzex2Opcode.G_ENDDL:
      case F3dzex2Opcode.G_SPNOOP:
      case F3dzex2Opcode.G_RDPHALF_1:
      case F3dzex2Opcode.G_SETOTHERMODE_L:
      case F3dzex2Opcode.G_SETOTHERMODE_H:
      case F3dzex2Opcode.G_RDPLOADSYNC:
      case F3dzex2Opcode.G_RDPPIPESYNC:
      case F3dzex2Opcode.G_RDPTILESYNC:
      case F3dzex2Opcode.G_RDPFULLSYNC:
      case F3dzex2Opcode.G_SETKEYGB:
      case F3dzex2Opcode.G_SETKEYR:
      case F3dzex2Opcode.G_SETSCISSOR:
      case F3dzex2Opcode.G_SETPRIMDEPTH:
      case F3dzex2Opcode.G_RDPSETOTHERMODE:
      case F3dzex2Opcode.G_LOADTLUT:
      case F3dzex2Opcode.G_RDPHALF_2:
      case F3dzex2Opcode.G_SETTILESIZE:
      case F3dzex2Opcode.G_LOADBLOCK:
      case F3dzex2Opcode.G_LOADTILE:
      case F3dzex2Opcode.G_FILLRECT:
      case F3dzex2Opcode.G_SETFILLCOLOR:
      case F3dzex2Opcode.G_SETFOGCOLOR:
      case F3dzex2Opcode.G_SETBLENDCOLOR:
      case F3dzex2Opcode.G_SETPRIMCOLOR:
      case F3dzex2Opcode.G_SETENVCOLOR:
      case F3dzex2Opcode.G_SETTIMG:
      case F3dzex2Opcode.G_SETZIMG:
      case F3dzex2Opcode.G_SETCIMG:
        return 1 * 2 * 4;
      case F3dzex2Opcode.G_BRANCH_Z:
      case F3dzex2Opcode.G_LOAD_UCODE:
      case F3dzex2Opcode.G_SETCONVERT:
      case F3dzex2Opcode.G_SETTILE:
      case F3dzex2Opcode.G_SETCOMBINE:
        return 2 * 2 * 4;
      case F3dzex2Opcode.G_TEXRECT:
      case F3dzex2Opcode.G_TEXRECTFLIP:
        return 3 * 2 * 4;
      default:
        throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null);
    }
  }

  /// <summary>
  ///   http://ultra64.ca/files/documentation/online-manuals/man/pro-man/pro12/index12.6.html#:~:text=Color%20Combiner%20(CC)%20can%20perform,in%20one%2Dcycle%20mode).
  /// </summary>
  private GenericColorMux GetColorMuxA_(uint value) => value switch {
      0 => GenericColorMux.G_CCMUX_COMBINED,
      1 => GenericColorMux.G_CCMUX_TEXEL0,
      2 => GenericColorMux.G_CCMUX_TEXEL1,
      3 => GenericColorMux.G_CCMUX_PRIMITIVE,
      4 => GenericColorMux.G_CCMUX_SHADE,
      5 => GenericColorMux.G_CCMUX_ENVIRONMENT,
      6 => GenericColorMux.G_CCMUX_1,
      7 => GenericColorMux.G_CCMUX_NOISE,
      >= 8 and <= 15 => GenericColorMux.G_CCMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  private GenericColorMux GetColorMuxB_(uint value) => value switch {
      0 => GenericColorMux.G_CCMUX_COMBINED,
      1 => GenericColorMux.G_CCMUX_TEXEL0,
      2 => GenericColorMux.G_CCMUX_TEXEL1,
      3 => GenericColorMux.G_CCMUX_PRIMITIVE,
      4 => GenericColorMux.G_CCMUX_SHADE,
      5 => GenericColorMux.G_CCMUX_ENVIRONMENT,
      6 => GenericColorMux.G_CCMUX_CENTER,
      7 => GenericColorMux.G_CCMUX_K4,
      >= 8 and <= 15 => GenericColorMux.G_CCMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  private GenericColorMux GetColorMuxC_(uint value) => value switch {
      0 => GenericColorMux.G_CCMUX_COMBINED,
      1 => GenericColorMux.G_CCMUX_TEXEL0,
      2 => GenericColorMux.G_CCMUX_TEXEL1,
      3 => GenericColorMux.G_CCMUX_PRIMITIVE,
      4 => GenericColorMux.G_CCMUX_SHADE,
      5 => GenericColorMux.G_CCMUX_ENVIRONMENT,
      6 => GenericColorMux.G_CCMUX_SCALE,
      7 => GenericColorMux.G_CCMUX_COMBINED_ALPHA,
      8 => GenericColorMux.G_CCMUX_TEXEL0_ALPHA,
      9 => GenericColorMux.G_CCMUX_TEXEL1_ALPHA,
      10 => GenericColorMux.G_CCMUX_PRIMITIVE_ALPHA,
      11 => GenericColorMux.G_CCMUX_SHADE_ALPHA,
      12 => GenericColorMux.G_CCMUX_ENV_ALPHA,
      13 => GenericColorMux.G_CCMUX_LOD_FRAC,
      14 => GenericColorMux.G_CCMUX_PRIM_LOD_FRAC,
      15 => GenericColorMux.G_CCMUX_K5,
      >= 16 and <= 31 => GenericColorMux.G_CCMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  private GenericColorMux GetColorMuxD_(uint value) => value switch {
      0 => GenericColorMux.G_CCMUX_COMBINED,
      1 => GenericColorMux.G_CCMUX_TEXEL0,
      2 => GenericColorMux.G_CCMUX_TEXEL1,
      3 => GenericColorMux.G_CCMUX_PRIMITIVE,
      4 => GenericColorMux.G_CCMUX_SHADE,
      5 => GenericColorMux.G_CCMUX_ENVIRONMENT,
      6 => GenericColorMux.G_CCMUX_1,
      7 => GenericColorMux.G_CCMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  private GenericAlphaMux GetAlphaMuxABD_(uint value) => value switch {
      0 => GenericAlphaMux.G_ACMUX_COMBINED,
      1 => GenericAlphaMux.G_ACMUX_TEXEL0,
      2 => GenericAlphaMux.G_ACMUX_TEXEL1,
      3 => GenericAlphaMux.G_ACMUX_PRIMITIVE,
      4 => GenericAlphaMux.G_ACMUX_SHADE,
      5 => GenericAlphaMux.G_ACMUX_ENVIRONMENT,
      6 => GenericAlphaMux.G_ACMUX_1,
      7 => GenericAlphaMux.G_ACMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };

  private GenericAlphaMux GetAlphaMuxC_(uint value) => value switch {
      0 => GenericAlphaMux.G_ACMUX_LOD_FRACTION,
      1 => GenericAlphaMux.G_ACMUX_TEXEL0,
      2 => GenericAlphaMux.G_ACMUX_TEXEL1,
      3 => GenericAlphaMux.G_ACMUX_PRIMITIVE,
      4 => GenericAlphaMux.G_ACMUX_SHADE,
      5 => GenericAlphaMux.G_ACMUX_ENVIRONMENT,
      6 => GenericAlphaMux.G_ACMUX_PRIM_LOD_FRAC,
      7 => GenericAlphaMux.G_ACMUX_0,
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
  };
}