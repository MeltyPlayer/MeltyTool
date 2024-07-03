using System.Drawing;

namespace gx;

public enum GxCullMode {
  None = 0,  // Do not cull any primitives
  Front = 1, // Cull front-facing primitives
  Back = 2,  // Cull back-facing primitives
  All = 3    // Cull all primitives
}

public interface IPopulatedMaterial {
  string Name { get; }
  GxCullMode CullMode { get; }

  (int, Color)[] MaterialColors { get; }
  IColorChannelControl?[] ColorChannelControls { get; }
  (int, Color)[] AmbientColors { get; }
  Color?[] LightColors { get; }

  Color[] KonstColors { get; }
  IColorRegister?[] ColorRegisters { get; }

  ITevOrder?[] TevOrderInfos { get; }
  ITevStageProps?[] TevStageInfos { get; }

  ITevSwapMode?[] TevSwapModes { get; }
  ITevSwapModeTable?[] TevSwapModeTables { get; }

  IAlphaCompare AlphaCompare { get; }
  IBlendFunction BlendMode { get; }

  ITexCoordGen?[] TexCoordGens { get; }
  ITextureMatrixInfo?[] TextureMatrices { get; }

  IDepthFunction DepthFunction { get; }

  short[] TextureIndices { get; }
}