using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Numerics;

using f3dzex2.combiner;
using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.image;
using f3dzex2.io;

using fin.data.lazy;
using fin.image;
using fin.image.util;
using fin.io;
using fin.io.bundles;
using fin.language.equations.fixedFunction;
using fin.language.equations.fixedFunction.impl;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.util.enums;
using fin.util.linq;

using schema.binary;

using Color = System.Drawing.Color;


namespace f3dzex2.model;

public sealed partial class DlModelBuilder {
  private readonly IN64Hardware n64Hardware_;
  private IMesh? currentMesh_;

  public const bool DEDUPLICATE_TEXTURES = true;
  public const bool DEDUPLICATE_MATERIALS = true;

  private readonly LazyDictionary<(ISegmentChunk?, ImageParams), IReadOnlyImage>
      lazyImageDictionary_;

  private readonly LazyDictionary<(ISegmentChunk?, TextureParams)?, IReadOnlyTexture?>
      lazyTextureDictionary_;

  private readonly LazyDictionary<MaterialParams, IReadOnlyMaterial>
      lazyMaterialDictionary_;

  private MaterialParams cachedMaterialParams_;
  private IReadOnlyMaterial cachedMaterial_;
  private readonly IF3dVertices vertices_;
  private bool isMaterialTransparent_ = false;

  public float TransparentCutoff { get; set; } = .5f;

  /// <summary>
  ///   Each model gets its own DlModelBuilder, but they all need to share
  ///   the same N64 hardware state (RSP/RDP). If they don't, you'll run into
  ///   weird graphical bugs that you'll spend ages debugging. :(
  /// </summary>
  public DlModelBuilder(IN64Hardware n64Hardware,
                        IFileBundle? fileBundle = null,
                        IReadOnlySet<IReadOnlyGenericFile>? files = null) {
    this.n64Hardware_ = n64Hardware;
    this.Model
        = new ModelImpl<Normal1Color2UvVertexImpl>((index, position) => new
                                                       Normal1Color2UvVertexImpl(
                                                           index,
                                                           position)) {
            FileBundle = fileBundle,
            Files = files ?? new HashSet<IReadOnlyGenericFile>(),
        };

    this.vertices_ = new F3dVertices(n64Hardware, this.Model);

    var imageByDataCrc32 = new Dictionary<uint, IReadOnlyImage>();

    // TODO: Deduplicate images with exact same data
    this.lazyImageDictionary_ =
        new(segmentAndImageParams => {
          var (_, imageParams) = segmentAndImageParams;

          if (!TmemUtil.AreColorFormatAndBitsPerTexelValid(
                  imageParams.ColorFormat,
                  imageParams.BitsPerTexel)) {
            return FinImage.Create1x1FromColor(Color.Magenta);
          }

          if (imageParams.IsInvalid) {
            return FinImage.Create1x1FromColor(
                this.vertices_.OverrideVertexColor);
          }

          SchemaBinaryReader? br = null;
          if (Constants.STRICT) {
            br = this.n64Hardware_.Memory.OpenAtSegmentedAddress(
                imageParams.SegmentedAddress);
          } else {
            if (this.n64Hardware_.Memory
                    .TryToOpenPossibilitiesAtSegmentedAddress(
                        imageParams.SegmentedAddress,
                        out var possibilities)) {
              if (!possibilities.TryGetFirst(out br)) {
                return FinImage.Create1x1FromColor(Color.Magenta);
                //Asserts.Fail($"Failed to find an image at address 0x{imageParams.SegmentedAddress.ToHex()}");
              }
            }
          }

          if (br != null) {
            var loadTileParams = imageParams.LoadTileParams;

            var sizeInBytes = imageParams.BitsPerTexel.GetByteCount(
                (uint) (imageParams.Width * imageParams.Height));

            try {
              byte[] imageData;
              if (loadTileParams == null) {
                imageData = br.ReadBytes(sizeInBytes);
              } else {
                var (fullWidth, uls, ult) = loadTileParams.Value;

                var fullLineSizeInBytes
                    = (int) imageParams.BitsPerTexel.GetByteCount(
                        fullWidth);
                var lineOffsetInBytes
                    = (int) imageParams.BitsPerTexel.GetByteCount(
                        uls);
                var usedLineSizeInBytes
                    = (int) imageParams.BitsPerTexel.GetByteCount(
                        imageParams.Width);

                imageData = new byte[sizeInBytes];

                Span<byte> fullLineSpan = stackalloc byte[fullLineSizeInBytes];
                for (var i = 0; i < ult; ++i) {
                  br.ReadBytes(fullLineSpan);
                }

                for (var i = 0; i < imageParams.Height; ++i) {
                  br.ReadBytes(fullLineSpan);
                  fullLineSpan.Slice(lineOffsetInBytes, usedLineSizeInBytes)
                              .CopyTo(imageData.AsSpan()
                                               .Slice(usedLineSizeInBytes * i,
                                                 usedLineSizeInBytes));
                }
              }

              br.Dispose();

              var crc32 = Crc32.HashToUInt32(imageData);
              if (DEDUPLICATE_TEXTURES) {
                if (imageByDataCrc32.TryGetValue(crc32, out var image)) {
                  return image;
                }
              }

              return imageByDataCrc32[crc32]
                  = new N64ImageParser(this.n64Hardware_).Parse(
                      imageParams.ColorFormat,
                      imageParams.BitsPerTexel,
                      imageData,
                      imageParams.Width,
                      imageParams.Height);
            } catch (Exception e) {
              return FinImage.Create1x1FromColor(Color.Magenta);
            }
          }

          return FinImage.Create1x1FromColor(Color.Magenta);
        });

    var textureByImageAndParams
        = new Dictionary<(IReadOnlyImage, WrapMode wrapModeU, WrapMode wrapModeV, float repeatCountS, float repeatCountT, UvType uvType, int uvIndex), IReadOnlyTexture>();

    // TODO: Deduplicate textures with exact same data
    this.lazyTextureDictionary_ =
        new(segmentAndTextureParamsOrNull => {
          if (segmentAndTextureParamsOrNull == null) {
            return null;
          }

          var (segment, textureParams) = segmentAndTextureParamsOrNull.Value;

          var imageParams = textureParams.ImageParams;
          var image = this.lazyImageDictionary_[(segment, imageParams)];

          var fullWidth = textureParams.Lrs - textureParams.Uls + 1;
          var fullHeight = textureParams.Lrt - textureParams.Ult + 1;

          var repeatCountS = fullWidth / image.Width;
          var repeatCountT = fullHeight / image.Height;

          var wrapModeU = textureParams.WrapModeS.AsFinWrapMode(repeatCountS);
          var wrapModeV = textureParams.WrapModeT.AsFinWrapMode(repeatCountT);
          var uvType = textureParams.UvType;
          var uvIndex = textureParams.Index;

          // Reuse existing texture if possible.
          if (DEDUPLICATE_TEXTURES) {
            if (textureByImageAndParams.TryGetValue(
                    (image, wrapModeU, wrapModeV, repeatCountS, repeatCountT,
                     uvType, uvIndex),
                    out var existingTexture)) {
              return existingTexture;
            }
          }

          var texture = this.Model.MaterialManager.CreateTexture(image);

          var color = this.vertices_.OverrideVertexColor;
          texture.Name = !imageParams.IsInvalid
              ? $"texture{this.lazyTextureDictionary_.Count}_0x{textureParams.SegmentedAddress:X8}"
              : $"rgb({color.R}, {color.G}, {color.B})";

          texture.ThreePointFiltering = true;

          texture.WrapModeU = wrapModeU;
          texture.WrapModeV = wrapModeV;
          texture.ClampS = new Vector2(0, repeatCountS);
          texture.ClampT = new Vector2(0, repeatCountT);

          texture.UvType = uvType;
          texture.UvIndex = uvIndex;

          textureByImageAndParams[
              (image, wrapModeU, wrapModeV, repeatCountS, repeatCountT, uvType, uvIndex)] = texture;

          return texture;
        });

    var materialByTexturesAndParams
        = new Dictionary<(IReadOnlyTexture? texture0, IReadOnlyTexture? texture1
            , GeometryMode geometryMode, uint otherModeH, uint otherModeL),
            IReadOnlyMaterial>();

    this.lazyMaterialDictionary_ =
        new(materialParams
                => {
              var cycleParams0 = materialParams.CombinerCycleParams0;
              var cycleParams1 = materialParams.CombinerCycleParams1;

              IReadOnlyTexture? texture0;
              IReadOnlyTexture? texture1;

              if (materialParams.HardcodedTexture0 != null) {
                texture0 = materialParams.HardcodedTexture0;
              } else {
                (ISegmentChunk?, TextureParams)? segmentAndTextureParams0
                    = materialParams.TextureParams0 != null
                        ? (n64Hardware.Memory.GetSegmentOrNull(
                               materialParams.TextureParams0
                                             .SegmentedAddress >>
                               24),
                           materialParams.TextureParams0)
                        : null;
               
                var usesTexel0 = cycleParams0.DependsOnTexel(0) ||
                                 (cycleParams1?.DependsOnTexel(1) ?? false);
                if (!usesTexel0) {
                  segmentAndTextureParams0 = null;
                }

                texture0 = this.lazyTextureDictionary_[segmentAndTextureParams0];
              }

              if (materialParams.HardcodedTexture1 != null) {
                texture1 = materialParams.HardcodedTexture1;
              } else {
                (ISegmentChunk?, TextureParams)? segmentAndTextureParams1
                    = materialParams.TextureParams1 != null
                        ? (n64Hardware.Memory.GetSegmentOrNull(
                               materialParams.TextureParams1.SegmentedAddress >>
                               24),
                           materialParams.TextureParams1)
                        : null;

                var usesTexel1 = cycleParams0.DependsOnTexel(1) ||
                                 (cycleParams1?.DependsOnTexel(0) ?? false);

                if (!usesTexel1) {
                  segmentAndTextureParams1 = null;
                }

                texture1 = this.lazyTextureDictionary_[segmentAndTextureParams1];
              }

              var rsp = this.n64Hardware_.Rsp;
              var rdp = n64Hardware.Rdp;

              var geometryMode = rsp.GeometryMode;
              var otherModeH = rdp.OtherModeH;
              var otherModeL = rdp.OtherModeL;

              if (DEDUPLICATE_MATERIALS) {
                if (materialByTexturesAndParams.TryGetValue((
                          texture0,
                          texture1,
                          geometryMode,
                          otherModeH,
                          otherModeL),
                      out var existingMaterial)) {
                  return existingMaterial;
                }
              }

              var finMaterial = this.Model.MaterialManager
                                    .AddFixedFunctionMaterial();
              materialByTexturesAndParams[(texture0, texture1, geometryMode,
                                           otherModeH, otherModeL)]
                  = finMaterial;

              finMaterial.Name = (texture0 == null, texture1 == null) switch {
                  (false, false) => $"[{texture0.Name}]/[{texture1.Name}]",
                  (false, true)  => $"[{texture0.Name}]",
                  (true, true)   => $"material{this.lazyMaterialDictionary_.Count}_{materialParams.GetHashCode()}",
              };
              finMaterial.CullingMode = materialParams.CullingMode;
              finMaterial.UpdateAlphaChannel = false;

              if (texture0 != null) {
                finMaterial.SetTextureSource(0, texture0);
                finMaterial.SetTextureSource(1, texture0);
              }

              if (texture1 != null) {
                finMaterial.SetTextureSource(1, texture1);
              }

              var equations = finMaterial.Equations;
              var color0 = equations.CreateColorConstant(0);
              var color1 = equations.CreateColorConstant(1);
              var scalar1 = equations.CreateScalarConstant(1);
              var scalar0 = equations.CreateScalarConstant(0);

              var colorOps = equations.ColorOps;
              var scalarOps = equations.ScalarOps;

              var environmentColor = equations.CreateColorConstant(
                  rsp.EnvironmentColor.R / 255f,
                  rsp.EnvironmentColor.G / 255f,
                  rsp.EnvironmentColor.B / 255f);
              var environmentAlpha = equations.CreateScalarConstant(
                  rsp.EnvironmentColor.A / 255f);
              var lodFracColor = color0;
              var lodFracAlpha = scalar0;
              var primLodFrac
                  = equations.CreateScalarConstant(rsp.PrimLodFraction);
              var primColorConstant = equations
                      .CreateColorConstant(
                          rsp.PrimColor.R / 255f,
                          rsp.PrimColor.G / 255f,
                          rsp.PrimColor.B / 255f);
              var primAlphaConstant = equations.CreateScalarConstant(
                  rsp.PrimColor.A / 255f);
              var shadeColor =
                  rsp.GeometryMode.CheckFlag(GeometryMode.G_LIGHTING)
                      ? equations.GetMergedLightDiffuseColor()
                      : equations.CreateOrGetColorInput(
                          FixedFunctionSource.VERTEX_COLOR_0);

              IColorValue combinedColor = color0;
              IScalarValue combinedAlpha = scalar0;

              var lazyAlphaValues
                  = new LazyDictionary<(GenericAlphaMux, int cycle),
                      IScalarValue>(tuple => {
                    var (alphaMux, cycle) = tuple;
                    return alphaMux switch {
                        GenericAlphaMux.G_ACMUX_COMBINED => combinedAlpha,
                        GenericAlphaMux.G_ACMUX_TEXEL0
                            => cycle == 0
                                ? equations.CreateOrGetScalarInput(
                                    FixedFunctionSource.TEXTURE_ALPHA_0)
                                : equations.CreateOrGetScalarInput(
                                    FixedFunctionSource.TEXTURE_ALPHA_1),
                        GenericAlphaMux.G_ACMUX_TEXEL1
                            => cycle == 0
                                ? equations.CreateOrGetScalarInput(
                                    FixedFunctionSource.TEXTURE_ALPHA_1)
                                : equations.CreateOrGetScalarInput(
                                    FixedFunctionSource.TEXTURE_ALPHA_0),
                        GenericAlphaMux.G_ACMUX_PRIMITIVE => rsp.UseRegisterForPrimColor
                            ? finMaterial.Registers.GetOrCreateScalarRegister(
                                "primitiveAlpha",
                                primAlphaConstant)
                            : primAlphaConstant,
                        GenericAlphaMux.G_ACMUX_SHADE
                            => equations.CreateOrGetScalarInput(
                                FixedFunctionSource.VERTEX_ALPHA_0),
                        GenericAlphaMux.G_ACMUX_ENVIRONMENT => environmentAlpha,
                        GenericAlphaMux.G_ACMUX_1 => scalar1,
                        GenericAlphaMux.G_ACMUX_0 => scalar0,
                        GenericAlphaMux.G_ACMUX_PRIM_LOD_FRAC => primLodFrac,
                        GenericAlphaMux.G_ACMUX_LOD_FRACTION => lodFracAlpha,
                        _ => throw new ArgumentOutOfRangeException(
                            nameof(alphaMux),
                            alphaMux,
                            null)
                    };
                  });
              var lazyColorValues
                  = new LazyDictionary<(GenericColorMux, int cycle),
                      IColorValue>(tuple => {
                    var (colorMux, cycle) = tuple;
                    return colorMux switch {
                        GenericColorMux.G_CCMUX_COMBINED =>
                            combinedColor,
                        GenericColorMux.G_CCMUX_TEXEL0
                            => cycle == 0
                                ? equations
                                    .CreateOrGetColorInput(
                                        FixedFunctionSource
                                            .TEXTURE_COLOR_0)
                                : equations
                                    .CreateOrGetColorInput(
                                        FixedFunctionSource
                                            .TEXTURE_COLOR_1),
                        GenericColorMux.G_CCMUX_TEXEL1
                            => cycle == 0
                                ? equations
                                    .CreateOrGetColorInput(
                                        FixedFunctionSource
                                            .TEXTURE_COLOR_1)
                                : equations
                                    .CreateOrGetColorInput(
                                        FixedFunctionSource
                                            .TEXTURE_COLOR_0),
                        GenericColorMux.G_CCMUX_PRIMITIVE =>
                            rsp.UseRegisterForPrimColor
                                ? finMaterial.Registers.GetOrCreateColorRegister(
                                    "primitiveColor",
                                    primColorConstant)
                                : primColorConstant,
                        GenericColorMux.G_CCMUX_SHADE =>
                            shadeColor,
                        GenericColorMux.G_CCMUX_ENVIRONMENT =>
                            environmentColor,
                        GenericColorMux.G_CCMUX_1 => color1,
                        GenericColorMux.G_CCMUX_0 => color0,
                        // TODO: Implement these
                        GenericColorMux.G_CCMUX_NOISE =>
                            color1,
                        GenericColorMux.G_CCMUX_LOD_FRAC =>
                            lodFracColor,
                        GenericColorMux.G_CCMUX_CENTER =>
                            color1,
                        GenericColorMux.G_CCMUX_K4 => color1,
                        GenericColorMux.G_CCMUX_COMBINED_ALPHA
                            =>
                            equations.CreateColor(
                                combinedAlpha),
                        GenericColorMux.G_CCMUX_TEXEL0_ALPHA
                            => cycle == 0
                                ? equations
                                    .CreateOrGetColorInput(
                                        FixedFunctionSource
                                            .TEXTURE_ALPHA_0)
                                : equations
                                    .CreateOrGetColorInput(
                                        FixedFunctionSource
                                            .TEXTURE_ALPHA_1),
                        GenericColorMux.G_CCMUX_TEXEL1_ALPHA
                            => cycle == 0
                                ? equations
                                    .CreateOrGetColorInput(
                                        FixedFunctionSource
                                            .TEXTURE_ALPHA_1)
                                : equations
                                    .CreateOrGetColorInput(
                                        FixedFunctionSource
                                            .TEXTURE_ALPHA_0),
                        GenericColorMux.G_CCMUX_PRIMITIVE_ALPHA
                            => equations.CreateColor(
                                lazyAlphaValues[
                                    (GenericAlphaMux.G_ACMUX_PRIMITIVE,
                                     cycle)]),
                        GenericColorMux.G_CCMUX_SHADE_ALPHA
                            => equations.CreateOrGetColorInput(
                                FixedFunctionSource
                                    .VERTEX_ALPHA_0),
                        GenericColorMux.G_CCMUX_ENV_ALPHA =>
                            equations.CreateColor(
                                environmentAlpha),
                        GenericColorMux.G_CCMUX_PRIM_LOD_FRAC
                            => primLodFrac.Wrap(),
                        GenericColorMux.G_CCMUX_SCALE =>
                            color1,
                        GenericColorMux.G_CCMUX_K5 => color1,
                        _ => throw new
                            ArgumentOutOfRangeException(
                                nameof(colorMux),
                                colorMux,
                                null)
                    };
                  });

              ReadOnlySpan<CombinerCycleParams> cycleParams =
                  cycleParams1 != null
                      ? [
                          cycleParams0,
                          cycleParams1
                      ]
                      : [cycleParams0];

              for (var cycle = 0; cycle < cycleParams.Length; ++cycle) {
                var combinerCycleParams = cycleParams[cycle];
                var cA = lazyColorValues[(combinerCycleParams.ColorMuxA, cycle)];
                var cB = lazyColorValues[(combinerCycleParams.ColorMuxB, cycle)];
                var cC = lazyColorValues[(combinerCycleParams.ColorMuxC, cycle)];
                var cD = lazyColorValues[(combinerCycleParams.ColorMuxD, cycle)];

                combinedColor = colorOps.Add(
                                    colorOps.Multiply(
                                        colorOps.Subtract(cA, cB),
                                        cC),
                                    cD) ??
                                colorOps.Zero;

                var aA = lazyAlphaValues[(combinerCycleParams.AlphaMuxA, cycle)];
                var aB = lazyAlphaValues[(combinerCycleParams.AlphaMuxB, cycle)];
                var aC = lazyAlphaValues[(combinerCycleParams.AlphaMuxC, cycle)];
                var aD = lazyAlphaValues[(combinerCycleParams.AlphaMuxD, cycle)];

                combinedAlpha = scalarOps.Add(
                                    scalarOps.Multiply(
                                        scalarOps.Subtract(aA, aB),
                                        aC),
                                    aD) ??
                                scalarOps.Zero;
              }

              equations.CreateColorOutput(FixedFunctionSource.OUTPUT_COLOR,
                                          combinedColor);
              equations.CreateScalarOutput(FixedFunctionSource.OUTPUT_ALPHA,
                                           combinedAlpha);

              ApplyBlendMode_(n64Hardware, finMaterial);

              // TODO: I'm not sure if alpha compare is ever used on the N64
              finMaterial.DisableAlphaCompare();

              // Shamelessly stolen from:
              // https://github.com/magcius/noclip.website/blob/main/src/zelview/f3dzex.ts#L109
              /*if (!rdp.ZCompare) {
                finMaterial.DepthCompareType = DepthCompareType.Always;
              } else {
                finMaterial.DepthCompareType = rdp.ZMode switch {
                    ZMode.ZMODE_DEC => DepthCompareType.GEqual,
                    _               => DepthCompareType.Greater,
                };
              }

              finMaterial.DepthMode = rdp.ZUpdate
                  ? DepthMode.READ_AND_WRITE
                  : DepthMode.READ_ONLY;*/

              return finMaterial;
            });
  }

  private static void ApplyBlendMode_(
      IN64Hardware n64Hardware,
      IMaterial finMaterial) {
    // TODO: What does "FORCE_BL" do?
    // TODO: Is it possible to support two-cycle blending, maybe via a shader?
    // TODO: Figure out how to better support this

    // HACK: Just kind of tries to see if we should do additive blending.
    // I have no idea how to properly implement this. 

    var rdp = n64Hardware.Rdp;

    var doAdditiveBlending
        = ShouldDoAdditiveBlendingFor_(rdp.P0, rdp.A0, rdp.M0, rdp.B0);
    if (rdp.CycleType is CycleType.TWO_CYCLE) {
      doAdditiveBlending
          |= ShouldDoAdditiveBlendingFor_(rdp.P1, rdp.A1, rdp.M1, rdp.B1);
    }

    var useCoverageInsteadOfAlpha = rdp is {
        UseCoverageForAlpha: true,
        MultiplyCoverageWithAlpha: false
    };

    if (doAdditiveBlending && !useCoverageInsteadOfAlpha) {
      finMaterial.SetBlending(
          BlendEquation.ADD,
          BlendFactor.SRC_ALPHA,
          BlendFactor.ONE_MINUS_SRC_ALPHA,
          LogicOp.UNDEFINED);
    } else {
      finMaterial.SetBlending(
          BlendEquation.ADD,
          BlendFactor.ONE,
          BlendFactor.ZERO,
          LogicOp.UNDEFINED);
    }
  }

  private static bool ShouldDoAdditiveBlendingFor_(
      BlenderPm p,
      BlenderA a,
      BlenderPm m,
      BlenderB b) {
    if (p is BlenderPm.G_BL_CLR_IN &&
        m is BlenderPm.G_BL_CLR_MEM &&
        a is BlenderA.G_BL_A_IN &&
        b is BlenderB.G_BL_1MA or BlenderB.G_BL_A_MEM) {
      return true;
    }

    return false;
  }

  public ModelImpl<Normal1Color2UvVertexImpl> Model { get; }

  public IMesh StartNewMesh(string name) {
    this.currentMesh_ = this.Model.Skin.AddMesh();
    this.currentMesh_.Name = name;
    return this.currentMesh_;
  }

  private IMesh CurrentMesh => this.currentMesh_ ??= this.Model.Skin.AddMesh();

  public int GetNumberOfTriangles() =>
      this.Model.Skin.Meshes
          .SelectMany(mesh => mesh.Primitives)
          .Select(primitive => primitive.Vertices.Count / 3)
          .Sum();

  public void AddDl(IDisplayList dl) {
    foreach (var opcodeCommand in dl.OpcodeCommands) {
      switch (opcodeCommand) {
        case NoopOpcodeCommand _:
          break;
        case DlOpcodeCommand dlOpcodeCommand: {
          foreach (var childDl in dlOpcodeCommand.PossibleBranches) {
            this.AddDl(childDl);
          }

          if (!dlOpcodeCommand.PushCurrentDlToStack) {
            return;
          }

          break;
        }
        case EndDlOpcodeCommand _: {
          return;
        }
        case MtxOpcodeCommand mtxOpcodeCommand: {
          if (mtxOpcodeCommand.ModelView && mtxOpcodeCommand.Load) {
            if (this.n64Hardware_.Rsp.BoneMapper
                    .TryToGetBoneAtSegmentedAddress(
                        mtxOpcodeCommand.RamAddress,
                        out var bone)) {
              this.n64Hardware_.Rsp.ActiveBoneWeights =
                  bone != null
                      ? this.Model.Skin.GetOrCreateBoneWeights(
                          VertexSpace.RELATIVE_TO_BONE,
                          bone)
                      : null;
            }
          }

          break;
        }
        case PopMtxOpcodeCommand popMtxOpcodeCommand:
          break;
        case SetEnvColorOpcodeCommand setEnvColorOpcodeCommand: {
          this.n64Hardware_.Rsp.EnvironmentColor = Color.FromArgb(
              setEnvColorOpcodeCommand.A,
              setEnvColorOpcodeCommand.R,
              setEnvColorOpcodeCommand.G,
              setEnvColorOpcodeCommand.B);
          break;
        }
        case SetPrimColorOpcodeCommand setPrimColorOpcodeCommand: {
          this.n64Hardware_.Rsp.PrimColor = Color.FromArgb(
              setPrimColorOpcodeCommand.A,
              setPrimColorOpcodeCommand.R,
              setPrimColorOpcodeCommand.G,
              setPrimColorOpcodeCommand.B);
          break;
        }
        case SetFogColorOpcodeCommand setFogColorOpcodeCommand:
          break;
        // Geometry mode commands
        case SetGeometryModeOpcodeCommand setGeometryModeOpcodeCommand: {
          this.UpdateGeometryMode_(
              dl.Type,
              default,
              setGeometryModeOpcodeCommand.FlagsToEnable);
          break;
        }
        case ClearGeometryModeOpcodeCommand clearGeometryModeOpcodeCommand: {
          this.UpdateGeometryMode_(
              dl.Type,
              clearGeometryModeOpcodeCommand.FlagsToDisable,
              default);
          break;
        }
        case GeometryModeOpcodeCommand geometryModeOpcodeCommand: {
          this.UpdateGeometryMode_(
              dl.Type,
              geometryModeOpcodeCommand.FlagsToDisable,
              geometryModeOpcodeCommand.FlagsToEnable);
          break;
        }
        case SetTileOpcodeCommand setTileOpcodeCommand: {
          this.n64Hardware_.Rdp.Tmem.GsDpSetTile(
              setTileOpcodeCommand.ColorFormat,
              setTileOpcodeCommand.BitsPerTexel,
              setTileOpcodeCommand.Num64BitValuesPerRow,
              setTileOpcodeCommand.OffsetOfTextureInTmem,
              setTileOpcodeCommand.TileDescriptorIndex,
              setTileOpcodeCommand.Palette,
              setTileOpcodeCommand.WrapModeS,
              setTileOpcodeCommand.MaskS,
              setTileOpcodeCommand.ShiftS,
              setTileOpcodeCommand.WrapModeT,
              setTileOpcodeCommand.MaskT,
              setTileOpcodeCommand.ShiftT);
          break;
        }
        case SetTileSizeOpcodeCommand setTileSizeOpcodeCommand: {
          this.n64Hardware_.Rdp.Tmem.GsDpSetTileSize(
              setTileSizeOpcodeCommand.Uls,
              setTileSizeOpcodeCommand.Ult,
              setTileSizeOpcodeCommand.TileDescriptorIndex,
              setTileSizeOpcodeCommand.Lrs,
              setTileSizeOpcodeCommand.Lrt);
          break;
        }
        case SetTimgOpcodeCommand setTimgOpcodeCommand: {
          this.n64Hardware_.Rdp.Tmem.GsDpSetTextureImage(
              setTimgOpcodeCommand.ColorFormat,
              setTimgOpcodeCommand.BitsPerTexel,
              setTimgOpcodeCommand.Width,
              setTimgOpcodeCommand
                  .TextureSegmentedAddress);
          break;
        }
        case TextureOpcodeCommand textureOpcodeCommand: {
          this.n64Hardware_.Rdp.Tmem.GsSpTexture(
              textureOpcodeCommand.HorizontalScaling,
              textureOpcodeCommand.VerticalScaling,
              textureOpcodeCommand.MaximumNumberOfMipmapsMinus1,
              textureOpcodeCommand.TileDescriptorIndex,
              textureOpcodeCommand.NewTileDescriptorState);
          break;
        }
        case SetCombineOpcodeCommand setCombineOpcodeCommand: {
          this.n64Hardware_.Rdp.CombinerCycleParams0 =
              setCombineOpcodeCommand.CombinerCycleParams0;
          this.n64Hardware_.Rdp.CombinerCycleParams1 =
              setCombineOpcodeCommand.CombinerCycleParams1;
          break;
        }
        case VtxOpcodeCommand vtxOpcodeCommand: {
          this.vertices_.LoadVertices(
              vtxOpcodeCommand.Vertices,
              vtxOpcodeCommand.IndexToBeginStoringVertices);
          break;
        }
        case Tri1OpcodeCommand tri1OpcodeCommand: {
          var material = this.GetOrCreateMaterial_();
          var vertices =
              tri1OpcodeCommand.VertexIndicesInOrder.Select(
                  this.vertices_.GetOrCreateVertexAtIndex);
          var triangles = this.CurrentMesh.AddTriangles(vertices.ToArray())
                              .SetMaterial(material)
                              .SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);

          if (this.isMaterialTransparent_) {
            triangles.SetInversePriority(1);
          } else {
            triangles.SetInversePriority(0);
          }

          break;
        }
        case Tri2OpcodeCommand tri2OpcodeCommand: {
          var material = this.GetOrCreateMaterial_();
          var vertices =
              tri2OpcodeCommand
                  .VertexIndicesInOrder0
                  .Concat(tri2OpcodeCommand.VertexIndicesInOrder1)
                  .Select(this.vertices_.GetOrCreateVertexAtIndex);
          var triangles = this.CurrentMesh.AddTriangles(vertices.ToArray())
                              .SetMaterial(material)
                              .SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);

          if (this.isMaterialTransparent_) {
            triangles.SetInversePriority(1);
          } else {
            triangles.SetInversePriority(0);
          }

          break;
        }
        case SetOtherModeHOpcodeCommand setOtherModeHOpcodeCommand: {
          var rdp = this.n64Hardware_.Rdp;

          var length = setOtherModeHOpcodeCommand.Length;
          var shift = setOtherModeHOpcodeCommand.Shift;
          var data = setOtherModeHOpcodeCommand.Data;

          rdp.OtherModeH =
              ApplyDataToOtherMode_(rdp.OtherModeH, length, shift, data);
          break;
        }
        case SetOtherModeLOpcodeCommand setOtherModeLOpcodeCommand: {
          var rdp = this.n64Hardware_.Rdp;

          var length = setOtherModeLOpcodeCommand.Length;
          var shift = setOtherModeLOpcodeCommand.Shift;
          var data = setOtherModeLOpcodeCommand.Data;

          rdp.OtherModeL =
              ApplyDataToOtherMode_(rdp.OtherModeL, length, shift, data);
          break;
        }
        case LoadBlockOpcodeCommand loadBlockOpcodeCommand: {
          this.n64Hardware_.Rdp.Tmem.GsDpLoadBlock(
              loadBlockOpcodeCommand.Uls,
              loadBlockOpcodeCommand.Ult,
              loadBlockOpcodeCommand.TileDescriptorIndex,
              loadBlockOpcodeCommand.Texels,
              loadBlockOpcodeCommand.Dxt);
          break;
        }
        case LoadTileOpcodeCommand loadTileOpcodeCommand: {
          this.n64Hardware_.Rdp.Tmem.GsDpLoadTile(
              loadTileOpcodeCommand.TileDescriptorIndex,
              loadTileOpcodeCommand.Uls,
              loadTileOpcodeCommand.Ult,
              loadTileOpcodeCommand.Lrs,
              loadTileOpcodeCommand.Lrt);
          break;
        }
        case LoadTlutOpcodeCommand loadTlutOpcodeCommand: {
          this.n64Hardware_.Rdp.Tmem.GsDpLoadTlut(
              loadTlutOpcodeCommand.TileDescriptorIndex,
              loadTlutOpcodeCommand.NumColorsToLoad);
          break;
        }
        case MoveMemOpcodeCommand moveMemOpcodeCommand: {
          // TODO: How to handle this in a more generalized way?
          switch (moveMemOpcodeCommand.DmemAddress) {
            // Diffuse light
            // https://hack64.net/wiki/doku.php?id=super_mario_64:fast3d_display_list_commands
            case DmemAddress.G_MV_L0: {
              using var br =
                  this.n64Hardware_.Memory.OpenAtSegmentedAddress(
                      moveMemOpcodeCommand.SegmentedAddress);
              var r = br.ReadByte();
              var g = br.ReadByte();
              var b = br.ReadByte();

              // TODO: Support normalized light direction

              this.vertices_.OverrideVertexColor = Color.FromArgb(r, g, b);

              break;
            }
            // Ambient light
            case DmemAddress.G_MV_L1: {
              break;
            }
          }

          break;
        }
        case ModifyVtxOpcodeCommand: {
          break;
        }
        default:
          throw new ArgumentOutOfRangeException(nameof(opcodeCommand));
      }
    }
  }

  private static uint ApplyDataToOtherMode_(uint otherMode,
                                            ushort length,
                                            ushort shift,
                                            uint data) {
    var mask = ~(((1 << length) - 1) << shift);
    return (uint) ((otherMode & mask) | data);
  }

  private void UpdateGeometryMode_(
      DisplayListType displayListType,
      GeometryMode flagsToDisable,
      GeometryMode flagsToEnable) {
    var originalCullingMode = this.GetCullingMode_(displayListType);

    this.n64Hardware_.Rsp.GeometryMode =
        (this.n64Hardware_.Rsp.GeometryMode & ~flagsToDisable) |
        flagsToEnable;

    var newCullingMode = this.GetCullingMode_(displayListType);
    if (newCullingMode != originalCullingMode) {
      this.n64Hardware_.Rsp.CullingMode = newCullingMode;
    }
  }

  private CullingMode GetCullingMode_(DisplayListType displayListType)
    => displayListType switch {
        DisplayListType.F3DZEX2 => this.n64Hardware_.Rsp.GeometryMode
                                       .GetCullingModeEx2(),
        _ => this.n64Hardware_.Rsp.GeometryMode.GetCullingModeNonEx2()
    };

  private IReadOnlyMaterial GetOrCreateMaterial_() {
    var newMaterialParams = this.n64Hardware_.Rdp.Tmem.GetMaterialParams();
    if (this.cachedMaterialParams_ == null ||
        !this.cachedMaterialParams_.Equals(newMaterialParams)) {
      this.cachedMaterialParams_ = newMaterialParams;
      this.cachedMaterial_ = this.lazyMaterialDictionary_[newMaterialParams];

      this.isMaterialTransparent_
          = this.cachedMaterial_.GetTransparencyType() ==
            TransparencyType.TRANSPARENT;
    }

    return this.cachedMaterial_;
  }

  public Color OverrideVertexColor {
    get => this.vertices_.OverrideVertexColor;
    set => this.vertices_.OverrideVertexColor = value;
  }

  public Matrix4x4 Matrix {
    get => this.vertices_.Matrix;
    set => this.vertices_.Matrix = value;
  }
}