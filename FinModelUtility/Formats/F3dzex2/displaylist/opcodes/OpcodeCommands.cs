using System;
using System.Collections.Generic;

using f3dzex2.combiner;
using f3dzex2.image;
using f3dzex2.model;

using fin.model;
using fin.util.enums;

namespace f3dzex2.displaylist.opcodes;

public class NoopOpcodeCommand : IOpcodeCommand { }

public class DlOpcodeCommand : IOpcodeCommand {
  public IReadOnlyList<IDisplayList> PossibleBranches { get; set; }
  public bool PushCurrentDlToStack { get; set; }
}

/// <summary>
///   Stops executing current DL and returns to one at top of stack.
/// </summary>
public class EndDlOpcodeCommand : IOpcodeCommand { }


public class MtxOpcodeCommand : IOpcodeCommand {
  public uint RamAddress { get; set; }

  public byte Params { get; set; }

  public bool Push => (Params & 1) != 0;
  public bool NoPush => !Push;

  public bool Load => (Params & 2) != 0;
  public bool Mul => !Load;

  public bool Projection => (Params & 4) != 0;
  public bool ModelView => !Projection;
}

public class PopMtxOpcodeCommand : IOpcodeCommand {
  public uint NumberOfMatrices { get; set; }
}


public class VtxOpcodeCommand : IOpcodeCommand {
  public IReadOnlyList<F3dVertex> Vertices { get; set; }
  public byte IndexToBeginStoringVertices { get; set; }
}

public class ModifyVtxOpcodeCommand : IOpcodeCommand { }


public enum TriVertexOrder {
  ABC = 0,
  CAB = 1,
  BCA = 2
}

public class Tri1OpcodeCommand : IOpcodeCommand {
  public TriVertexOrder VertexOrder { get; set; }
  public byte VertexIndexA { get; set; }
  public byte VertexIndexB { get; set; }
  public byte VertexIndexC { get; set; }

  public IEnumerable<byte> VertexIndicesInOrder {
    get {
        var startOffset = VertexOrder switch {
            TriVertexOrder.ABC => 0,
            TriVertexOrder.BCA => 1,
            TriVertexOrder.CAB => 2,
            _                  => throw new ArgumentOutOfRangeException()
        };

        for (var i = 0; i < 3; ++i) {
          var current = (startOffset + i) % 3;
          yield return current switch {
              0 => VertexIndexA,
              1 => VertexIndexB,
              2 => VertexIndexC,
              _ => throw new ArgumentOutOfRangeException()
          };
        }
      }
  }
}

public class Tri2OpcodeCommand : IOpcodeCommand {
  public TriVertexOrder VertexOrder0 { get; set; }
  public byte VertexIndexA0 { get; set; }
  public byte VertexIndexB0 { get; set; }
  public byte VertexIndexC0 { get; set; }

  public TriVertexOrder VertexOrder1 { get; set; }
  public byte VertexIndexA1 { get; set; }
  public byte VertexIndexB1 { get; set; }
  public byte VertexIndexC1 { get; set; }

  public IEnumerable<byte> VertexIndicesInOrder0 {
    get {
        var startOffset = VertexOrder0 switch {
            TriVertexOrder.ABC => 0,
            TriVertexOrder.BCA => 1,
            TriVertexOrder.CAB => 2,
            _                  => throw new ArgumentOutOfRangeException()
        };

        for (var i = 0; i < 3; ++i) {
          var current = (startOffset + i) % 3;
          yield return current switch {
              0 => VertexIndexA0,
              1 => VertexIndexB0,
              2 => VertexIndexC0,
              _ => throw new ArgumentOutOfRangeException()
          };
        }
      }
  }

  public IEnumerable<byte> VertexIndicesInOrder1 {
    get {
        var startOffset = VertexOrder1 switch {
            TriVertexOrder.ABC => 0,
            TriVertexOrder.BCA => 1,
            TriVertexOrder.CAB => 2,
            _                  => throw new ArgumentOutOfRangeException()
        };

        for (var i = 0; i < 3; ++i) {
          var current = (startOffset + i) % 3;
          yield return current switch {
              0 => VertexIndexA1,
              1 => VertexIndexB1,
              2 => VertexIndexC1,
              _ => throw new ArgumentOutOfRangeException()
          };
        }
      }
  }
}

public class SetEnvColorOpcodeCommand : IOpcodeCommand {
  public byte R { get; set; }
  public byte G { get; set; }
  public byte B { get; set; }
  public byte A { get; set; }
}

public class SetFogColorOpcodeCommand : IOpcodeCommand {
  public byte R { get; set; }
  public byte G { get; set; }
  public byte B { get; set; }
  public byte A { get; set; }
}


public class SetPrimColorOpcodeCommand : IOpcodeCommand {
  public byte R { get; set; }
  public byte G { get; set; }
  public byte B { get; set; }
  public byte A { get; set; }

  // TODO: Handle LOD values
}


public enum TileDescriptorState {
  DISABLED = 0,
  ENABLED = 1,
}

public enum TileDescriptorIndex : byte {
  TX_RENDERTILE = 0x0,
  TX_LOADTILE = 0x7,
}

public static class TileDescriptorExtensions {
  public static bool IsRender(this TileDescriptorIndex tileDescriptorIndex)
    => tileDescriptorIndex == TileDescriptorIndex.TX_RENDERTILE;
}

public class TextureOpcodeCommand : IOpcodeCommand {
  public TileDescriptorIndex TileDescriptorIndex { get; set; }
  public TileDescriptorState NewTileDescriptorState { get; set; }
  public byte MaximumNumberOfMipmaps { get; set; }
  public ushort HorizontalScaling { get; set; }
  public ushort VerticalScaling { get; set; }
}

public class SetTimgOpcodeCommand : IOpcodeCommand {
  public N64ColorFormat ColorFormat { get; set; }
  public BitsPerTexel BitsPerTexel { get; set; }
  public ushort Width { get; set; }
  public uint TextureSegmentedAddress { get; set; }
}

[Flags]
public enum GeometryMode : uint {
  G_ZBUFFER = 1 << 0,
  G_SHADE = 1 << 2,
  G_CULL_FRONT_EX2 = 1 << 9,
  G_SHADING_SMOOTH_NONEX2 = 1 << 9,
  G_CULL_BACK_EX2 = 1 << 10,
  G_CULL_FRONT_NONEX2 = 1 << 12,
  G_CULL_BACK_NONEX2 = 1 << 13,
  G_FOG = 1 << 16,
  G_LIGHTING = 1 << 17,
  G_TEXTURE_GEN = 1 << 18,
  G_TEXTURE_GEN_LINEAR = 1 << 19,
  G_SHADING_SMOOTH_EX2 = 1 << 21,
  G_CLIPPING_EX2 = 1 << 23,
}

public static class GeometryModeExtensions {
  public static CullingMode GetCullingModeNonEx2(
      this GeometryMode geometryMode)
    => GetCullingMode_(
        geometryMode.CheckFlag(GeometryMode.G_CULL_BACK_NONEX2),
        geometryMode.CheckFlag(GeometryMode.G_CULL_FRONT_NONEX2));

  public static CullingMode GetCullingModeEx2(
      this GeometryMode geometryMode)
    => GetCullingMode_(
        geometryMode.CheckFlag(GeometryMode.G_CULL_BACK_EX2),
        geometryMode.CheckFlag(GeometryMode.G_CULL_FRONT_EX2));

  private static CullingMode GetCullingMode_(bool cullBack, bool cullFront) {
      return (cullBack, cullFront) switch {
          (false, false) => CullingMode.SHOW_BOTH,
          (false, true)  => CullingMode.SHOW_BACK_ONLY,
          (true, false)  => CullingMode.SHOW_FRONT_ONLY,
          (true, true)   => CullingMode.SHOW_NEITHER,
      };
    }


  public static UvType GetUvType(this GeometryMode geometryMode)
    => geometryMode.CheckFlag(GeometryMode.G_TEXTURE_GEN)
        ? UvType.SPHERICAL
        : geometryMode.CheckFlag(GeometryMode.G_TEXTURE_GEN_LINEAR)
            ? UvType.LINEAR
            : UvType.STANDARD;
}

public class SetGeometryModeOpcodeCommand : IOpcodeCommand {
  public GeometryMode FlagsToEnable { get; set; }
}

public class ClearGeometryModeOpcodeCommand : IOpcodeCommand {
  public GeometryMode FlagsToDisable { get; set; }
}

public class GeometryModeOpcodeCommand : IOpcodeCommand {
  public GeometryMode FlagsToDisable { get; set; }
  public GeometryMode FlagsToEnable { get; set; }
}

public enum F3dWrapMode : byte {
  REPEAT = 0,
  MIRROR_REPEAT = 1,
  CLAMP = 2,
}

public static class F3dWrapModeExtensions {
  public static WrapMode AsFinWrapMode(this F3dWrapMode f3dWrapMode)
    => f3dWrapMode switch {
        F3dWrapMode.REPEAT        => WrapMode.REPEAT,
        F3dWrapMode.MIRROR_REPEAT => WrapMode.MIRROR_REPEAT,
        F3dWrapMode.CLAMP         => WrapMode.CLAMP,
        // TODO: What does this actually mean?
        (F3dWrapMode) 3 => WrapMode.MIRROR_REPEAT,
        _ => throw new ArgumentOutOfRangeException(
            nameof(f3dWrapMode),
            f3dWrapMode,
            null)
    };
}

public class SetTileOpcodeCommand : IOpcodeCommand {
  public TileDescriptorIndex TileDescriptorIndex { get; set; }
  public N64ColorFormat ColorFormat { get; set; }
  public BitsPerTexel BitsPerTexel { get; set; }

  public F3dWrapMode WrapModeT { get; set; }
  public F3dWrapMode WrapModeS { get; set; }

  public ushort Num64BitValuesPerRow { get; set; }
  public ushort OffsetOfTextureInTmem { get; set; }

  // TODO: Support the rest
}

public class SetTileSizeOpcodeCommand : IOpcodeCommand {
  public TileDescriptorIndex TileDescriptorIndex { get; set; }
  public ushort Uls { get; set; }
  public ushort Ult { get; set; }
  public ushort Width { get; set; }
  public ushort Height { get; set; }

  // TODO: Support the rest
}

public class SetCombineOpcodeCommand : IOpcodeCommand {
  public required CombinerCycleParams CombinerCycleParams0 { get; init; }
  public required CombinerCycleParams CombinerCycleParams1 { get; init; }
}

public class LoadBlockOpcodeCommand : IOpcodeCommand {
  public required TileDescriptorIndex TileDescriptorIndex { get; init; }
  public required ushort Texels { get; init; }
}

public class LoadTlutOpcodeCommand : IOpcodeCommand {
  public required TileDescriptorIndex TileDescriptorIndex { get; init; }
  public required ushort NumColorsToLoad { get; init; }
}


public enum DmemAddress {
  G_MV_L0 = 0x86,
  G_MV_L1 = 0x88,
}

public class MoveMemOpcodeCommand : IOpcodeCommand {
  public DmemAddress DmemAddress { get; set; }

  public uint SegmentedAddress { get; set; }
}