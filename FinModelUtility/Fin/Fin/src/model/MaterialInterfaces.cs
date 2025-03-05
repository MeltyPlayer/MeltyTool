using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

using fin.color;
using fin.data.indexable;
using fin.image;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.shaders.glsl;
using fin.image.util;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface IMaterialManager {
  new IReadOnlyList<IMaterial> All { get; }
  new IFixedFunctionRegisters? Registers { get; }

  // TODO: Name is actually required, should be required in the creation scripts?
  INullMaterial AddNullMaterial();
  IHiddenMaterial AddHiddenMaterial();
  ITextureMaterial AddTextureMaterial(IReadOnlyTexture texture);
  IColorMaterial AddColorMaterial(Color color);
  IShaderMaterial AddShaderMaterial(string vertexShader, string fragmentShader);
  IStandardMaterial AddStandardMaterial();
  IFixedFunctionMaterial AddFixedFunctionMaterial();

  ITexture CreateTexture(IReadOnlyImage image);
  ITexture CreateTexture(IReadOnlyImage[] mipmapImages);

  IScrollingTexture CreateScrollingTexture(IReadOnlyImage imageData,
                                           float scrollSpeedX,
                                           float scrollSpeedY);

  new IReadOnlyList<ITexture> Textures { get; }
}

public enum MaterialType {
  TEXTURED,
  PBR,
  LAYER,
}

public enum CullingMode {
  SHOW_FRONT_ONLY,
  SHOW_BACK_ONLY,
  SHOW_BOTH,
  SHOW_NEITHER,
}

[Flags]
public enum DepthMode {
  NONE,
  READ = 0x1,
  WRITE = 0x2,

  READ_ONLY = READ,
  WRITE_ONLY = WRITE,
  READ_AND_WRITE = READ | WRITE,
}

public enum DepthCompareType {
  LEqual,
  Less,
  Equal,
  Greater,
  NEqual,
  GEqual,
  Always,
  Never,
}

[GenerateReadOnly]
public partial interface IMaterial {
  new string? Name { get; set; }

  new IEnumerable<IReadOnlyTexture> Textures { get; }

  new CullingMode CullingMode { get; set; }

  new DepthMode DepthMode { get; set; }
  new DepthCompareType DepthCompareType { get; set; }

  new bool IgnoreLights { get; set; }
  new float Shininess { get; set; }

  new TransparencyType TransparencyType { get; set; }

  new bool UpdateColorChannel { get; set; }
  new bool UpdateAlphaChannel { get; set; }

  new IFogParams? FogParams { get; }
  IMaterial SetFog(float nearDistance, float farDistance, IColor color);

  // TODO: Merge this into a single type
  new BlendEquation ColorBlendEquation { get; }
  new BlendFactor ColorSrcFactor { get; }
  new BlendFactor ColorDstFactor { get; }
  new BlendEquation AlphaBlendEquation { get; }
  new BlendFactor AlphaSrcFactor { get; }
  new BlendFactor AlphaDstFactor { get; }
  new LogicOp LogicOp { get; }

  IMaterial SetBlending(
      BlendEquation blendEquation,
      BlendFactor srcFactor,
      BlendFactor dstFactor,
      LogicOp logicOp);

  IMaterial SetBlendingSeparate(
      BlendEquation colorBlendEquation,
      BlendFactor colorSrcFactor,
      BlendFactor colorDstFactor,
      BlendEquation alphaBlendEquation,
      BlendFactor alphaSrcFactor,
      BlendFactor alphaDstFactor,
      LogicOp logicOp);
}

[GenerateReadOnly]
public partial interface IMaterialWithNormalTexture : IMaterial {
  new IReadOnlyTexture? NormalTexture { get; set; }
}

[GenerateReadOnly]
public partial interface INullMaterial : IMaterial;

[GenerateReadOnly]
public partial interface IHiddenMaterial : IMaterial;

[GenerateReadOnly]
public partial interface ITextureMaterial : IMaterial {
  new IReadOnlyTexture Texture { get; }
  new Color? DiffuseColor { get; set; }
}

[GenerateReadOnly]
public partial interface IColorMaterial : IMaterial {
  new Color Color { get; set; }
}

[GenerateReadOnly]
public partial interface IStandardMaterial : IMaterialWithNormalTexture {
  new IReadOnlyTexture? DiffuseTexture { get; set; }
  new IReadOnlyTexture? AmbientOcclusionTexture { get; set; }
  new IReadOnlyTexture? EmissiveTexture { get; set; }
  new IReadOnlyTexture? SpecularTexture { get; set; }
}

[GenerateReadOnly]
public partial interface IShaderMaterial : IMaterial {
  new string VertexShader { get; set; }
  new string FragmentShader { get; set; }

  IReadOnlyDictionary<string, IReadOnlyTexture> TextureByUniform { get; }
  void AddTexture(string uniformName, IReadOnlyTexture texture);
}

// TODO: Support empty white materials
// TODO: Support basic diffuse materials
// TODO: Support lit/unlit
// TODO: Support merged diffuse/normal/etc. materials

public enum BlendEquation {
  NONE,
  ADD,
  SUBTRACT,
  REVERSE_SUBTRACT,
  MIN,
  MAX
}

public enum BlendFactor {
  ZERO,
  ONE,
  SRC_COLOR,
  ONE_MINUS_SRC_COLOR,
  SRC_ALPHA,
  ONE_MINUS_SRC_ALPHA,
  DST_COLOR,
  ONE_MINUS_DST_COLOR,
  DST_ALPHA,
  ONE_MINUS_DST_ALPHA,
  CONST_COLOR,
  ONE_MINUS_CONST_COLOR,
  CONST_ALPHA,
  ONE_MINUS_CONST_ALPHA,
}

public enum LogicOp {
  UNDEFINED,
  CLEAR,
  AND,
  AND_REVERSE,
  COPY,
  AND_INVERTED,
  NOOP,
  XOR,
  OR,
  NOR,
  EQUIV,
  INVERT,
  OR_REVERSE,
  COPY_INVERTED,
  OR_INVERTED,
  NAND,
  SET,
}

public enum AlphaCompareType : byte {
  Never = 0,
  Less = 1,
  Equal = 2,
  LEqual = 3,
  Greater = 4,
  NEqual = 5,
  GEqual = 6,
  Always = 7
}

public enum AlphaOp : byte {
  And = 0,
  Or = 1,
  XOR = 2,
  XNOR = 3
}

public enum FixedFunctionSource {
  TEXTURE_COLOR_0,
  TEXTURE_COLOR_1,
  TEXTURE_COLOR_2,
  TEXTURE_COLOR_3,
  TEXTURE_COLOR_4,
  TEXTURE_COLOR_5,
  TEXTURE_COLOR_6,
  TEXTURE_COLOR_7,

  TEXTURE_ALPHA_0,
  TEXTURE_ALPHA_1,
  TEXTURE_ALPHA_2,
  TEXTURE_ALPHA_3,
  TEXTURE_ALPHA_4,
  TEXTURE_ALPHA_5,
  TEXTURE_ALPHA_6,
  TEXTURE_ALPHA_7,

  CONST_COLOR_0,
  CONST_COLOR_1,
  CONST_COLOR_2,
  CONST_COLOR_3,
  CONST_COLOR_4,
  CONST_COLOR_5,
  CONST_COLOR_6,
  CONST_COLOR_7,
  CONST_COLOR_8,
  CONST_COLOR_9,
  CONST_COLOR_10,
  CONST_COLOR_11,
  CONST_COLOR_12,
  CONST_COLOR_13,
  CONST_COLOR_14,
  CONST_COLOR_15,

  CONST_ALPHA_0,
  CONST_ALPHA_1,
  CONST_ALPHA_2,

  VERTEX_COLOR_0,
  VERTEX_COLOR_1,

  VERTEX_ALPHA_0,
  VERTEX_ALPHA_1,

  OUTPUT_COLOR,
  OUTPUT_ALPHA,

  LIGHT_AMBIENT_COLOR,
  LIGHT_AMBIENT_ALPHA,

  LIGHT_DIFFUSE_COLOR_MERGED,
  LIGHT_DIFFUSE_ALPHA_MERGED,

  LIGHT_DIFFUSE_COLOR_0,
  LIGHT_DIFFUSE_COLOR_1,
  LIGHT_DIFFUSE_COLOR_2,
  LIGHT_DIFFUSE_COLOR_3,
  LIGHT_DIFFUSE_COLOR_4,
  LIGHT_DIFFUSE_COLOR_5,
  LIGHT_DIFFUSE_COLOR_6,
  LIGHT_DIFFUSE_COLOR_7,

  LIGHT_DIFFUSE_ALPHA_0,
  LIGHT_DIFFUSE_ALPHA_1,
  LIGHT_DIFFUSE_ALPHA_2,
  LIGHT_DIFFUSE_ALPHA_3,
  LIGHT_DIFFUSE_ALPHA_4,
  LIGHT_DIFFUSE_ALPHA_5,
  LIGHT_DIFFUSE_ALPHA_6,
  LIGHT_DIFFUSE_ALPHA_7,

  LIGHT_SPECULAR_COLOR_MERGED,
  LIGHT_SPECULAR_ALPHA_MERGED,

  LIGHT_SPECULAR_COLOR_0,
  LIGHT_SPECULAR_COLOR_1,
  LIGHT_SPECULAR_COLOR_2,
  LIGHT_SPECULAR_COLOR_3,
  LIGHT_SPECULAR_COLOR_4,
  LIGHT_SPECULAR_COLOR_5,
  LIGHT_SPECULAR_COLOR_6,
  LIGHT_SPECULAR_COLOR_7,

  LIGHT_SPECULAR_ALPHA_0,
  LIGHT_SPECULAR_ALPHA_1,
  LIGHT_SPECULAR_ALPHA_2,
  LIGHT_SPECULAR_ALPHA_3,
  LIGHT_SPECULAR_ALPHA_4,
  LIGHT_SPECULAR_ALPHA_5,
  LIGHT_SPECULAR_ALPHA_6,
  LIGHT_SPECULAR_ALPHA_7,

  UNDEFINED,
}

[GenerateReadOnly]
public partial interface IFixedFunctionMaterial : IMaterialWithNormalTexture {
  new IFixedFunctionEquations<FixedFunctionSource> Equations { get; }
  new IFixedFunctionRegisters Registers { get; }

  new IReadOnlyList<IReadOnlyTexture?> TextureSources { get; }

  IFixedFunctionMaterial SetTextureSource(int textureIndex,
                                          IReadOnlyTexture texture);

  new IReadOnlyTexture? CompiledTexture { get; set; }

  // TODO: Merge this into a single type
  new AlphaOp AlphaOp { get; }
  new AlphaCompareType AlphaCompareType0 { get; }
  new float AlphaReference0 { get; }
  new AlphaCompareType AlphaCompareType1 { get; }
  new float AlphaReference1 { get; }

  IFixedFunctionMaterial SetAlphaCompare(
      AlphaOp alphaOp,
      AlphaCompareType alphaCompareType0,
      float reference0,
      AlphaCompareType alphaCompareType1,
      float reference1);

  IFixedFunctionMaterial SetAlphaCompare(AlphaCompareType alphaCompareType,
                                         float reference)
    => this.SetAlphaCompare(AlphaOp.Or,
                            AlphaCompareType.Never,
                            0,
                            alphaCompareType,
                            reference);

  IFixedFunctionMaterial SetDefaultAlphaCompare() {
    switch (this.TransparencyType) {
      case TransparencyType.MASK: {
        this.SetAlphaCompare(AlphaCompareType.Greater,
                             GlslConstants.MIN_ALPHA_BEFORE_DISCARD_MASK);
        break;
      }
      case TransparencyType.TRANSPARENT: {
        this.SetAlphaCompare(AlphaCompareType.Greater,
                             GlslConstants
                                 .MIN_ALPHA_BEFORE_DISCARD_TRANSPARENT);
        break;
      }
    }

    return this;
  }
}

public enum UvType {
  STANDARD,
  SPHERICAL,
  LINEAR
}

public enum WrapMode {
  CLAMP,
  REPEAT,
  MIRROR_REPEAT,
  MIRROR_CLAMP,
}

public static class WrapModeUtil {
  public static WrapMode FromMirrorAndRepeat(bool mirror, bool repeat)
    => ((mirror, repeat)) switch {
        (false, false) => WrapMode.CLAMP,
        (false, true)  => WrapMode.REPEAT,
        (true, false)  => WrapMode.MIRROR_CLAMP,
        (true, true)   => WrapMode.MIRROR_REPEAT,
    };
}

public enum ColorType {
  COLOR,
  INTENSITY,
}

public enum TextureMagFilter {
  NEAR,
  LINEAR,
}

public enum TextureMinFilter {
  NEAR,
  LINEAR,
  NEAR_MIPMAP_NEAR,
  NEAR_MIPMAP_LINEAR,
  LINEAR_MIPMAP_NEAR,
  LINEAR_MIPMAP_LINEAR,
}

[GenerateReadOnly]
public partial interface ITexture : IIndexable {
  new string Name { get; set; }

  new LocalImageFormat BestImageFormat { get; }
  new string ValidFileName { get; }

  new int UvIndex { get; set; }
  new UvType UvType { get; set; }
  new ColorType ColorType { get; set; }

  new IReadOnlyImage[] MipmapImages { get; }
  new IReadOnlyImage Image => this.MipmapImages[0];
  new Bitmap ImageData { get; }


  [Const]
  new void WriteToStream(Stream stream);

  [Const]
  new void SaveInDirectory(ISystemDirectory directory);

  new TransparencyType TransparencyType { get; }

  new WrapMode WrapModeU { get; set; }
  new WrapMode WrapModeV { get; set; }

  new IColor? BorderColor { get; set; }

  new TextureMagFilter MagFilter { get; set; }
  new TextureMinFilter MinFilter { get; set; }
  new float MinLod { get; set; }
  new float MaxLod { get; set; }
  new float LodBias { get; set; }

  new Vector2? ClampS { get; set; }
  new Vector2? ClampT { get; set; }

  new bool IsTransform3d { get; }

  new Vector3? Center { get; }
  ITexture SetCenter2d(float x, float y);
  ITexture SetCenter3d(float x, float y, float z);

  new Vector3? Translation { get; }

  ITexture SetTranslation2d(float x, float y)
    => SetTranslation2d(new Vector2(x, y));

  ITexture SetTranslation2d(in Vector2 xy);

  ITexture SetTranslation3d(float x, float y, float z)
    => SetTranslation3d(new Vector3(x, y, z));

  ITexture SetTranslation3d(in Vector3 xyz);

  new Vector3? Scale { get; }
  ITexture SetScale2d(float x, float y) => SetScale2d(new Vector2(x, y));
  ITexture SetScale2d(in Vector2 xy);

  ITexture SetScale3d(float x, float y, float z)
    => SetScale3d(new Vector3(x, y, z));

  ITexture SetScale3d(in Vector3 xyz);

  new Vector3? RotationRadians { get; }
  ITexture SetRotationRadians2d(float rotationRadians);

  ITexture SetRotationRadians3d(float x, float y, float z)
    => SetRotationRadians3d(new Vector3(x, y, z));

  ITexture SetRotationRadians3d(in Vector3 xyz);

  // TODO: Support fixed # of repeats
  // TODO: Support animated textures
  // TODO: Support animated texture index param
}

public interface IScrollingTexture : ITexture {
  float ScrollSpeedX { get; }
  float ScrollSpeedY { get; }
}