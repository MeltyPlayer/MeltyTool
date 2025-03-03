using System;

using fin.schema;
using fin.schema.color;

using schema.binary;

namespace grezzo.schema.cmb.mats;

public class Material : IBinaryConvertible {
  public bool isFragmentLightingEnabled;
  public bool isVertexLightingEnabled;
  public bool IsFogEnabled { get; set; }
  public RenderLayer RenderLayer { get; set; }
  public CullMode faceCulling;
  public bool isPolygonOffsetEnabled;
  public float polygonOffset;

  [Unknown]
  public uint unk0;

  public uint textureMappersUsed;
  public uint textureCoordsUsed;
  public readonly TexMapper[] texMappers = new TexMapper[3];
  public readonly TexCoords[] texCoords = new TexCoords[3];

  public Rgba32 emissionColor { get; private set; }
  public Rgba32 ambientColor { get; private set; }
  public Rgba32 diffuseRgba { get; private set; }
  public Rgba32 specular0Color { get; private set; }
  public Rgba32 specular1Color { get; private set; }

  public Rgba32[] constantColors { get; } = new Rgba32[6];

  public readonly float[] bufferColor = new float[4];

  public BumpTexture bumpTexture;
  public BumpMode bumpMode;
  public bool isBumpRenormalized;

  public LayerConfig layerConfig;
  public FresnelConfig fresnelSelector;
  public bool isClampHighlight;
  public bool isDistribution0Enabled;
  public bool isDistribution1Enabled;
  public bool isGeometricFactor0Enabled;
  public bool isGeometricFactor1Enabled;
  public bool isReflectionEnabled;

  public readonly Sampler reflectanceRSampler = new();
  public readonly Sampler reflectanceGSampler = new();
  public readonly Sampler reflectanceBSampler = new();
  public readonly Sampler distibution0Sampler = new();
  public readonly Sampler distibution1Sampler = new();
  public readonly Sampler fresnelSampler = new();

  public uint texEnvStageCount;
  public short[] texEnvStagesIndices = new short[6];

  public bool alphaTestEnabled;
  public float alphaTestReferenceValue;
  public TestFunc alphaTestFunction;
  public bool depthTestEnabled;
  public bool depthWriteEnabled;
  public TestFunc depthTestFunction;
  public BlendMode blendMode;

  public bool LogicOpEnabled { get; set; }
  public ushort LogicOp { get; set; }

  public BlendFactor colorSrcFunc;
  public BlendFactor colorDstFunc;
  public BlendEquation colorEquation;
  public BlendFactor alphaSrcFunc;
  public BlendFactor alphaDstFunc;
  public BlendEquation alphaEquation;
  public readonly float[] blendColor = new float[4];

  public bool stencilEnabled;
  public byte stencilReferenceValue;
  public byte stencilBufferMask;
  public byte stencilBuffer;
  public TestFunc stencilFunc;
  public StencilTestOp stencilFailOp;
  public StencilTestOp stencilZFailOp;
  public StencilTestOp stencilZPassOp;

  [Unknown]
  public uint stenilUnk1; // CRC32 of something

  public void Read(IBinaryReader br) {
      this.isFragmentLightingEnabled = br.ReadByte() != 0;
      this.isVertexLightingEnabled = br.ReadByte() != 0;
      this.IsFogEnabled = br.ReadByte() != 0;
      this.RenderLayer = (RenderLayer) br.ReadByte();

      this.faceCulling = (CullMode) br.ReadByte();

      this.isPolygonOffsetEnabled = br.ReadByte() != 0;
      this.polygonOffset = br.ReadInt16() / 65534f;

      if (CmbHeader.Version > Version.MAJORAS_MASK_3D) {
        this.unk0 = br.ReadUInt32();
        this.textureMappersUsed = (uint) br.ReadInt16();
        this.textureCoordsUsed = (uint) br.ReadInt16();
      } else {
        this.textureMappersUsed = br.ReadUInt32();
        this.textureCoordsUsed = br.ReadUInt32();
      }

      for (var i = 0; i < 3; ++i) {
        var texMapper = new TexMapper();
        texMapper.Read(br);
        this.texMappers[i] = texMapper;
      }

      for (var i = 0; i < 3; ++i) {
        var texCoord = new TexCoords();
        texCoord.Read(br);
        this.texCoords[i] = texCoord;
      }

      this.emissionColor = br.ReadNew<Rgba32>();
      this.ambientColor = br.ReadNew<Rgba32>();
      this.diffuseRgba = br.ReadNew<Rgba32>();
      this.specular0Color = br.ReadNew<Rgba32>();
      this.specular1Color = br.ReadNew<Rgba32>();

      for (var i = 0; i < this.constantColors.Length; ++i) {
        this.constantColors[i] = br.ReadNew<Rgba32>();
      }

      br.ReadSingles(this.bufferColor);

      this.bumpTexture = (BumpTexture) br.ReadUInt16();
      this.bumpMode = (BumpMode) br.ReadUInt16();
      this.isBumpRenormalized = br.ReadUInt32() != 0;

      this.layerConfig = (LayerConfig) br.ReadUInt32();
      this.fresnelSelector = (FresnelConfig) br.ReadUInt16();
      this.isClampHighlight = br.ReadByte() != 0;
      this.isDistribution0Enabled = br.ReadByte() != 0;
      this.isDistribution1Enabled = br.ReadByte() != 0;
      this.isGeometricFactor0Enabled = br.ReadByte() != 0;
      this.isGeometricFactor1Enabled = br.ReadByte() != 0;
      this.isReflectionEnabled = br.ReadByte() != 0;

      this.distibution0Sampler.Read(br);
      this.distibution1Sampler.Read(br);
      this.reflectanceRSampler.Read(br);
      this.reflectanceGSampler.Read(br);
      this.reflectanceBSampler.Read(br);
      this.fresnelSampler.Read(br);

      this.texEnvStageCount = br.ReadUInt32();
      for (var i = 0; i < 6; ++i) {
        this.texEnvStagesIndices[i] = br.ReadInt16();
      }

      this.alphaTestEnabled = br.ReadByte() != 0;
      this.alphaTestReferenceValue = br.ReadByte() / 255f;
      this.alphaTestFunction = (TestFunc) br.ReadUInt16();
      this.depthTestEnabled = br.ReadByte() != 0;
      this.depthWriteEnabled = br.ReadByte() != 0;
      this.depthTestFunction = (TestFunc) br.ReadUInt16();

      this.blendMode = (BlendMode) (br.ReadByte());

      this.LogicOpEnabled = br.ReadByte() != 0;
      this.LogicOp = br.ReadUInt16();

      this.colorSrcFunc = (BlendFactor) (br.ReadUInt16());
      this.colorDstFunc = (BlendFactor) (br.ReadUInt16());
      this.colorEquation = (BlendEquation) (br.ReadUInt32());
      this.alphaSrcFunc = (BlendFactor) (br.ReadUInt16());
      this.alphaDstFunc = (BlendFactor) (br.ReadUInt16());
      this.alphaEquation = (BlendEquation) (br.ReadUInt32());
      br.ReadSingles(this.blendColor);

      if (CmbHeader.Version.SupportsStencilBuffer()) {
        this.stencilEnabled = br.ReadByte() != 0;
        this.stencilReferenceValue = br.ReadByte();
        this.stencilBufferMask = br.ReadByte();
        this.stencilBuffer = br.ReadByte();
        this.stencilFunc = (TestFunc) br.ReadUInt16();
        this.stencilFailOp = (StencilTestOp) br.ReadUInt16();
        this.stencilZFailOp = (StencilTestOp) br.ReadUInt16();
        this.stencilZPassOp = (StencilTestOp) br.ReadUInt16();
        this.stenilUnk1 = br.ReadUInt32();
      }
    }

  public void Write(IBinaryWriter bw) {
      throw new NotImplementedException();
    }
}