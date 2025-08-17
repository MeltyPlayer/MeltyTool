using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

using f3dzex2.combiner;
using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.image;

using fin.data.lazy;
using fin.image;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.model;
using fin.model.impl;
using fin.image.util;
using fin.io.bundles;
using fin.util.enums;

using schema.binary;


namespace f3dzex2.model;

public class DlModelBuilder {
  private readonly IN64Hardware n64Hardware_;
  private IMesh? currentMesh_;

  private readonly LazyDictionary<ImageParams, IImage>
      lazyImageDictionary_;

  private readonly LazyDictionary<TextureParams?, ITexture?>
      lazyTextureDictionary_;

  private readonly LazyDictionary<MaterialParams, IMaterial>
      lazyMaterialDictionary_;

  private MaterialParams cachedMaterialParams_;
  private IMaterial cachedMaterial_;
  private readonly IF3dVertices vertices_;
  private bool isMaterialTransparent_ = false;

  /// <summary>
  ///   Each model gets its own DlModelBuilder, but they all need to share
  ///   the same N64 hardware state (RSP/RDP). If they don't, you'll run into
  ///   weird graphical bugs that you'll spend ages debugging. :(
  /// </summary>
  public DlModelBuilder(IN64Hardware n64Hardware,
                        IFileBundle? fileBundle = null,
                        IReadOnlySet<IReadOnlyGenericFile>? files = null) {
    this.n64Hardware_ = n64Hardware;
    this.Model = new ModelImpl {
        FileBundle = fileBundle,
        Files = files ?? new HashSet<IReadOnlyGenericFile>(),
    };

    this.vertices_ = new F3dVertices(n64Hardware, this.Model);

    this.lazyImageDictionary_ =
        new(imageParams => {
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
              br = possibilities.First();
            }
          }

          if (br != null) {
            var sizeInBytes = imageParams.BitsPerTexel.GetByteCount(
                (uint) (imageParams.Width * imageParams.Height));
            var imageData = br.ReadBytes(sizeInBytes);
            br.Dispose();
            return new N64ImageParser(this.n64Hardware_).Parse(
                imageParams.ColorFormat,
                imageParams.BitsPerTexel,
                imageData,
                imageParams.Width,
                imageParams.Height);
          } else {
            return FinImage.Create1x1FromColor(Color.Magenta);
          }
        });

    this.lazyTextureDictionary_ =
        new(textureParamsOrNull => {
          if (textureParamsOrNull == null) {
            return null;
          }

          var textureParams = textureParamsOrNull.Value;
          var imageParams = textureParams.ImageParams;
          var image = this.lazyImageDictionary_[imageParams];
          var texture = this.Model.MaterialManager.CreateTexture(image);

          var color = this.vertices_.OverrideVertexColor;
          texture.Name = !imageParams.IsInvalid
              ? String.Format("0x{0:X8}", textureParams.SegmentedAddress)
              : $"rgb({color.R}, {color.G}, {color.B})";

          texture.WrapModeU = textureParams.WrapModeS.AsFinWrapMode();
          texture.WrapModeV = textureParams.WrapModeT.AsFinWrapMode();
          texture.UvType = textureParams.UvType;
          texture.UvIndex = textureParams.Index;
          return texture;
        });

    this.lazyMaterialDictionary_ =
        new(materialParams
                => {
              var texture0 =
                  this.lazyTextureDictionary_[materialParams.TextureParams0];
              var texture1 =
                  this.lazyTextureDictionary_[materialParams.TextureParams1];

              var finMaterial = this.Model.MaterialManager
                                    .AddFixedFunctionMaterial();

              finMaterial.Name = (texture0 == null, texture1 == null) switch {
                  (false, false) => $"[{texture0.Name}]/[{texture1.Name}]",
                  (false, true)  => $"[{texture0.Name}]",
                  (true, true)   => $"{materialParams.GetHashCode()}",
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

              var rsp = this.n64Hardware_.Rsp;
              var environmentColor = equations.CreateColorConstant(
                  rsp.EnvironmentColor.R / 255.0,
                  rsp.EnvironmentColor.G / 255.0,
                  rsp.EnvironmentColor.B / 255.0);
              var environmentAlpha = equations.CreateScalarConstant(
                  rsp.EnvironmentColor.A / 255.0);
              var primColor = equations.CreateColorConstant(
                  rsp.PrimColor.R / 255.0,
                  rsp.PrimColor.G / 255.0,
                  rsp.PrimColor.B / 255.0);
              var primAlpha = equations.CreateScalarConstant(
                  rsp.PrimColor.A / 255.0);

              var shadeColor =
                  rsp.GeometryMode.CheckFlag(GeometryMode.G_LIGHTING)
                      ? equations.GetMergedLightDiffuseColor()
                      : equations.CreateOrGetColorInput(
                          FixedFunctionSource.VERTEX_COLOR_0);

              IColorValue combinedColor = color0;
              IScalarValue combinedAlpha = scalar0;

              Func<GenericColorMux, IColorValue> getColorValue =
                  (colorMux) => colorMux switch {
                      GenericColorMux.G_CCMUX_COMBINED => combinedColor,
                      GenericColorMux.G_CCMUX_TEXEL0
                          => equations.CreateOrGetColorInput(
                              FixedFunctionSource.TEXTURE_COLOR_0),
                      GenericColorMux.G_CCMUX_TEXEL1
                          => equations.CreateOrGetColorInput(
                              FixedFunctionSource.TEXTURE_COLOR_1),
                      GenericColorMux.G_CCMUX_PRIMITIVE   => primColor,
                      GenericColorMux.G_CCMUX_SHADE       => shadeColor,
                      GenericColorMux.G_CCMUX_ENVIRONMENT => environmentColor,
                      GenericColorMux.G_CCMUX_1           => color1,
                      GenericColorMux.G_CCMUX_0           => color0,
                      GenericColorMux.G_CCMUX_NOISE       => color1,
                      GenericColorMux.G_CCMUX_CENTER      => color1,
                      GenericColorMux.G_CCMUX_K4          => color1,
                      GenericColorMux.G_CCMUX_COMBINED_ALPHA =>
                          equations.CreateColor(combinedAlpha),
                      GenericColorMux.G_CCMUX_TEXEL0_ALPHA
                          => equations.CreateOrGetColorInput(
                              FixedFunctionSource.TEXTURE_ALPHA_0),
                      GenericColorMux.G_CCMUX_TEXEL1_ALPHA
                          => equations.CreateOrGetColorInput(
                              FixedFunctionSource.TEXTURE_ALPHA_1),
                      GenericColorMux.G_CCMUX_PRIMITIVE_ALPHA =>
                          equations.CreateColor(primAlpha),
                      GenericColorMux.G_CCMUX_SHADE_ALPHA
                          => equations.CreateOrGetColorInput(
                              FixedFunctionSource.VERTEX_ALPHA_0),
                      GenericColorMux.G_CCMUX_ENV_ALPHA =>
                          equations.CreateColor(environmentAlpha),
                      GenericColorMux.G_CCMUX_PRIM_LOD_FRAC => color1,
                      GenericColorMux.G_CCMUX_SCALE         => color1,
                      GenericColorMux.G_CCMUX_K5            => color1,
                      _ => throw new ArgumentOutOfRangeException(
                          nameof(colorMux),
                          colorMux,
                          null)
                  };

              Func<GenericAlphaMux, IScalarValue> getAlphaValue =
                  (alphaMux) => alphaMux switch {
                      GenericAlphaMux.G_ACMUX_COMBINED => combinedAlpha,
                      GenericAlphaMux.G_ACMUX_TEXEL0 =>
                          equations.CreateOrGetScalarInput(
                              FixedFunctionSource.TEXTURE_ALPHA_0),
                      GenericAlphaMux.G_ACMUX_TEXEL1 =>
                          equations.CreateOrGetScalarInput(
                              FixedFunctionSource.TEXTURE_ALPHA_1),
                      GenericAlphaMux.G_ACMUX_PRIMITIVE => primAlpha,
                      GenericAlphaMux.G_ACMUX_SHADE
                          => equations.CreateOrGetScalarInput(
                              FixedFunctionSource.VERTEX_ALPHA_0),
                      GenericAlphaMux.G_ACMUX_ENVIRONMENT   => environmentAlpha,
                      GenericAlphaMux.G_ACMUX_1             => scalar1,
                      GenericAlphaMux.G_ACMUX_0             => scalar0,
                      GenericAlphaMux.G_ACMUX_PRIM_LOD_FRAC => scalar1,
                      GenericAlphaMux.G_ACMUX_LOD_FRACTION  => scalar1,
                      _ => throw new ArgumentOutOfRangeException(
                          nameof(alphaMux),
                          alphaMux,
                          null)
                  };

              var cycleParams0 = materialParams.CombinerCycleParams0;
              var cycleParams1 = materialParams.CombinerCycleParams1;
              ReadOnlySpan<CombinerCycleParams> cycleParams =
                  cycleParams1 != null
                      ? [
                          cycleParams0,
                          cycleParams1.Value
                      ]
                      : [cycleParams0];

              foreach (var combinerCycleParams in cycleParams) {
                var cA = getColorValue(combinerCycleParams.ColorMuxA);
                var cB = getColorValue(combinerCycleParams.ColorMuxB);
                var cC = getColorValue(combinerCycleParams.ColorMuxC);
                var cD = getColorValue(combinerCycleParams.ColorMuxD);

                combinedColor = colorOps.Add(
                                    colorOps.Multiply(
                                        colorOps.Subtract(cA, cB),
                                        cC),
                                    cD) ??
                                colorOps.Zero;

                var aA = getAlphaValue(combinerCycleParams.AlphaMuxA);
                var aB = getAlphaValue(combinerCycleParams.AlphaMuxB);
                var aC = getAlphaValue(combinerCycleParams.AlphaMuxC);
                var aD = getAlphaValue(combinerCycleParams.AlphaMuxD);

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

              if (finMaterial.Textures.Any(texture
                                               => TransparencyTypeUtil
                                                      .GetTransparencyType(
                                                          texture.Image) ==
                                                  TransparencyType
                                                      .TRANSPARENT)) {
                finMaterial.SetAlphaCompare(AlphaOp.Or,
                                            AlphaCompareType.Always,
                                            .5f,
                                            AlphaCompareType.Always,
                                            0);
                finMaterial.SetBlending(BlendEquation.ADD,
                                        BlendFactor.SRC_ALPHA,
                                        BlendFactor.ONE_MINUS_SRC_ALPHA,
                                        LogicOp.UNDEFINED);
              } else {
                finMaterial.SetAlphaCompare(AlphaCompareType.Greater,
                                            .5f);
              }

              return finMaterial;
            });
  }

  public ModelImpl Model { get; }

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
              textureOpcodeCommand.MaximumNumberOfMipmaps,
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

          var mask = ((1 << length) - 1) << shift;
          rdp.OtherModeH = (uint) ((rdp.OtherModeH & ~mask) | (data & mask));
          break;
        }
        case SetOtherModeLOpcodeCommand setOtherModeLOpcodeCommand: {
          var rdp = this.n64Hardware_.Rdp;

          var length = setOtherModeLOpcodeCommand.Length;
          var shift = setOtherModeLOpcodeCommand.Shift;
          var data = setOtherModeLOpcodeCommand.Data;

          var mask = ((1 << length) - 1) << shift;
          rdp.OtherModeL = (uint) ((rdp.OtherModeL & ~mask) | (data & mask));
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
      this.n64Hardware_.Rdp.Tmem.CullingMode = newCullingMode;
    }
  }

  private CullingMode GetCullingMode_(DisplayListType displayListType)
    => displayListType switch {
        DisplayListType.F3DZEX2 => this.n64Hardware_.Rsp.GeometryMode
                                       .GetCullingModeEx2(),
        _ => this.n64Hardware_.Rsp.GeometryMode.GetCullingModeNonEx2()
    };

  private IMaterial GetOrCreateMaterial_() {
    var newMaterialParams = this.n64Hardware_.Rdp.Tmem.GetMaterialParams();
    if (!this.cachedMaterialParams_.Equals(newMaterialParams)) {
      this.cachedMaterialParams_ = newMaterialParams;
      this.cachedMaterial_ = this.lazyMaterialDictionary_[newMaterialParams];

      var textures = this.cachedMaterial_.Textures;
      this.isMaterialTransparent_ =
          textures.Any(t => TransparencyTypeUtil.GetTransparencyType(t.Image) ==
                            TransparencyType.TRANSPARENT);
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