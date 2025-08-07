using System;

using f3dzex2.combiner;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.math;

using schema.binary;


namespace f3dzex2.displaylist.opcodes.f3d;

public class F3dOpcodeParser : IOpcodeParser {
  public IOpcodeCommand Parse(IReadOnlyN64Memory n64Memory,
                              IDisplayListReader dlr,
                              SchemaBinaryReader br) {
    var baseOffset = br.Position;
    var opcode = (F3dOpcode) br.ReadByte();
    var opcodeCommand = ParseOpcodeCommand_(n64Memory, dlr, br, opcode);
    br.Position = baseOffset + GetCommandLength_(opcode);
    return opcodeCommand;
  }

  public DisplayListType Type => DisplayListType.FAST3D;

  private IOpcodeCommand ParseOpcodeCommand_(IReadOnlyN64Memory n64Memory,
                                             IDisplayListReader dlr,
                                             SchemaBinaryReader br,
                                             F3dOpcode opcode) {
    switch (opcode) {
      case F3dOpcode.G_MTX: {
        var mtxParams = br.ReadByte();
        br.AssertUInt16(0);
        var address = br.ReadUInt32();
        return new MtxOpcodeCommand {
            Params = mtxParams, RamAddress = address,
        };
      }
      case F3dOpcode.G_POPMTX: {
        br.AssertUInt24(0);
        br.AssertUInt32(0);
        return new PopMtxOpcodeCommand {NumberOfMatrices = 1};
      }
      case F3dOpcode.G_VTX: {
        var numVerticesMinusOneAndWriteOffset = br.ReadByte();
        var numVertices = BitLogic.ExtractFromRight(
            numVerticesMinusOneAndWriteOffset,
            4,
            4) + 1;
        var writeOffset = BitLogic.ExtractFromRight(
            numVerticesMinusOneAndWriteOffset,
            0,
            4);
        br.AssertUInt16((ushort) (numVertices * 0x10));

        var segmentedAddress = br.ReadUInt32();
        using var sbr = n64Memory.OpenAtSegmentedAddress(segmentedAddress);

        return new VtxOpcodeCommand {
            Vertices = sbr.ReadNews<F3dVertex>((int) numVertices),
            IndexToBeginStoringVertices = (byte) writeOffset,
        };
      }
      case F3dOpcode.G_DL: {
        var storeReturnAddress = br.ReadByte() == 0;
        br.AssertUInt16(0);
        var address = br.ReadUInt32();
        return new DlOpcodeCommand {
            PossibleBranches =
                dlr.ReadPossibleDisplayLists(n64Memory, this, address),
            PushCurrentDlToStack = storeReturnAddress
        };
      }
      case F3dOpcode.G_ENDDL: {
        br.AssertUInt24(0);
        br.AssertUInt32(0);
        return new EndDlOpcodeCommand();
      }
      case F3dOpcode.G_TRI1: {
        br.AssertUInt32(0);
        return new Tri1OpcodeCommand {
            VertexOrder = TriVertexOrder.ABC,
            VertexIndexA = (byte) (br.ReadByte() / 0xA),
            VertexIndexB = (byte) (br.ReadByte() / 0xA),
            VertexIndexC = (byte) (br.ReadByte() / 0xA),
        };
      }
      case F3dOpcode.G_SETENVCOLOR: {
        br.AssertUInt24(0);
        return new SetEnvColorOpcodeCommand {
            R = br.ReadByte(),
            G = br.ReadByte(),
            B = br.ReadByte(),
            A = br.ReadByte(),
        };
      }
      case F3dOpcode.G_SETFOGCOLOR: {
        br.AssertUInt24(0);
        return new SetFogColorOpcodeCommand {
            R = br.ReadByte(),
            G = br.ReadByte(),
            B = br.ReadByte(),
            A = br.ReadByte(),
        };
      }
      case F3dOpcode.G_SETTIMG: {
        N64ImageParser.SplitN64ImageFormat(br.ReadByte(),
                                           out var colorFormat,
                                           out var bitSize);
        br.AssertUInt16(0);
        return new SetTimgOpcodeCommand {
            ColorFormat = colorFormat,
            BitsPerTexel = bitSize,
            TextureSegmentedAddress = br.ReadUInt32(),
        };
      }
      case F3dOpcode.G_SETGEOMETRYMODE: {
        br.AssertUInt24(0);
        return new SetGeometryModeOpcodeCommand {
            FlagsToEnable = (GeometryMode) br.ReadUInt32()
        };
      }
      case F3dOpcode.G_CLEARGEOMETRYMODE: {
        br.AssertUInt24(0);
        return new ClearGeometryModeOpcodeCommand {
            FlagsToDisable = (GeometryMode) br.ReadUInt32()
        };
      }
      case F3dOpcode.G_TEXTURE: {
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
        var newTileDescriptorState = (TileDescriptorState) br.ReadByte();
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
      case F3dOpcode.G_SETTILE:
        return F3dUtil.ParseSetTileOpcodeCommand(br);
      case F3dOpcode.G_SETTILESIZE: {
        br.Position--;

        var first = br.ReadUInt32();
        var second = br.ReadUInt32();

        var ul = TmemUtil.ParseCoordAxes(first);

        var tileDescriptor
            = (TileDescriptorIndex) second.ExtractFromRight(24, 4);
        var lr = TmemUtil.ParseCoordAxes(second);

        return new SetTileSizeOpcodeCommand {
            TileDescriptorIndex = tileDescriptor,
            Uls = ul.X,
            Ult = ul.Y,
            Lrs = lr.X,
            Lrt = lr.Y,
        };
      }
      case F3dOpcode.G_SETCOMBINE: {
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
      case F3dOpcode.G_LOADBLOCK:
        return F3dUtil.ParseLoadBlockOpcodeCommand(br);
      case F3dOpcode.G_MOVEMEM: {
        var commandType = (DmemAddress) br.ReadByte();
        var sizeInBytes = br.ReadUInt16();
        var segmentedAddress = br.ReadUInt32();

        return new MoveMemOpcodeCommand {
            DmemAddress = commandType, SegmentedAddress = segmentedAddress,
        };
      }
      // TODO: Implement these
      case F3dOpcode.G_MOVEWORD:
      case F3dOpcode.G_SETOTHERMODE_L:
      case F3dOpcode.G_SETOTHERMODE_H:
        return new NoopOpcodeCommand();
      case F3dOpcode.G_RDPLOADSYNC:
      case F3dOpcode.G_RDPPIPESYNC:
      case F3dOpcode.G_RDPTILESYNC:
      case F3dOpcode.G_RDPFULLSYNC:
      case F3dOpcode.G_SPNOOP:
      case F3dOpcode.G_NOOP: {
        br.AssertUInt24(0);
        br.AssertUInt32(0);
        return new NoopOpcodeCommand();
      }
      default:
        throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null);
    }
  }

  private int GetCommandLength_(F3dOpcode opcode) {
    switch (opcode) {
      case F3dOpcode.G_SPNOOP:
      case F3dOpcode.G_MTX:
      case F3dOpcode.G_MOVEMEM:
      case F3dOpcode.G_VTX:
      case F3dOpcode.G_DL:
      case F3dOpcode.G_RDPHALF_CONT:
      case F3dOpcode.G_RDPHALF_2:
      case F3dOpcode.G_RDPHALF_1:
      case F3dOpcode.G_CLEARGEOMETRYMODE:
      case F3dOpcode.G_SETGEOMETRYMODE:
      case F3dOpcode.G_ENDDL:
      case F3dOpcode.G_SETOTHERMODE_H:
      case F3dOpcode.G_SETOTHERMODE_L:
      case F3dOpcode.G_TEXTURE:
      case F3dOpcode.G_MOVEWORD:
      case F3dOpcode.G_POPMTX:
      case F3dOpcode.G_CULLDL:
      case F3dOpcode.G_TRI1:
      case F3dOpcode.G_NOOP:
      case F3dOpcode.G_RDPLOADSYNC:
      case F3dOpcode.G_RDPPIPESYNC:
      case F3dOpcode.G_RDPTILESYNC:
      case F3dOpcode.G_RDPFULLSYNC:
      case F3dOpcode.G_SETKEYGB:
      case F3dOpcode.G_SETKEYR:
      case F3dOpcode.G_SETSCISSOR:
      case F3dOpcode.G_SETPRIMDEPTH:
      case F3dOpcode.G_RDPSETOTHERMODE:
      case F3dOpcode.G_LOADTLUT:
      case F3dOpcode.G_SETTILESIZE:
      case F3dOpcode.G_LOADBLOCK:
      case F3dOpcode.G_LOADTILE:
      case F3dOpcode.G_SETTILE:
      case F3dOpcode.G_FILLRECT:
      case F3dOpcode.G_SETFILLCOLOR:
      case F3dOpcode.G_SETFOGCOLOR:
      case F3dOpcode.G_SETBLENDCOLOR:
      case F3dOpcode.G_SETPRIMCOLOR:
      case F3dOpcode.G_SETENVCOLOR:
      case F3dOpcode.G_SETCOMBINE:
      case F3dOpcode.G_SETTIMG:
      case F3dOpcode.G_SETZIMG:
      case F3dOpcode.G_SETCIMG:
        return 1 * 2 * 4;
      case F3dOpcode.G_TEXRECT:
      case F3dOpcode.G_TEXRECTFLIP:
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