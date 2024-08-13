using gx;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd.mat3;

/// <summary>
///   Represents how one of the four color channels will be generated. These
///   are, by index:
/// 
///   <list type="bullet">
///     <item>0 - Color0</item>
///     <item>1 - Alpha0</item>
///     <item>2 - Color1</item>
///     <item>3 - Alpha1</item>
///   </list>
///
///   <seealso href="https://github.com/LordNed/JStudio/blob/93c5c4479ffb1babefe829cfc9794694a1cb93e6/JStudio/J3D/ShaderGen/VertexShaderGen.cs">
///     Based on information from JStudio's VertexShader generation logic.
///   </seealso>
/// </summary>
[BinarySchema]
public partial class ColorChannelControl : IColorChannelControl,
                                           IBinaryConvertible {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool LightingEnabled { get; set; }

  public GxColorSrc MaterialSrc { get; set; }

  /// <summary>
  ///   Which lights affect the vertex.
  /// </summary>
  public GxLightMask LitMask { get; set; }

  /// <summary>
  ///   How to merge the lights together.
  /// </summary>
  public GxDiffuseFunction DiffuseFunction { get; set; }

  /// <summary>
  ///   What type of attenuation function to use for the lights.
  /// </summary>
  public GxAttenuationFunction AttenuationFunction { get; set; }

  public GxColorSrc AmbientSrc { get; set; }

  private readonly ushort padding_ = ushort.MaxValue;
}