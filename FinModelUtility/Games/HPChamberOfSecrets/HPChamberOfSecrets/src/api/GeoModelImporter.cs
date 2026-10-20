using System.Numerics;
using System.Runtime.InteropServices;

using fin.data.lazy;
using fin.image;
using fin.image.util;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.math.rotations;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using hpcos.schema.geo;

using schema.binary;

namespace hpcos.api;

/// <summary>
///   Importer for HP COS GCN GEO models, based on lumos by pixel#8843.
///   https://github.com/metallicafan212/HarryPotterUnrealWiki/blob/master/Other/Console%20Model%20and%20Texture%20Exporter/lumos.py
///   Treats all files as static meshes for reduced scope.
/// </summary>
public sealed class GeoModelImporter
    : IModelImporter<GeoModelImporter, GeoModelFileBundle> {
  /// <summary>
  ///   Layer flags that affect how a material is rendered. Other bits are
  ///   masked out so they don't split otherwise identical materials.
  /// </summary>
  internal const GeoLayerFlags SUPPORTED_LAYER_FLAGS
      = GeoLayerFlags.ENVIRONMENT_MAP |
        GeoLayerFlags.ALPHA_BLEND |
        GeoLayerFlags.DOUBLE_SIDED |
        GeoLayerFlags.ADDITIVE |
        GeoLayerFlags.SUBTRACTIVE |
        GeoLayerFlags.ALPHA_TEST |
        GeoLayerFlags.UV_SCROLL;

  // A guess at a reasonable scroll speed for UV_SCROLL materials. It's not
  // clear where this is actually stored in the game files.
  private const float UV_SCROLL_SPEED_X_ = 0.05f;
  private const float UV_SCROLL_SPEED_Y_ = 0;

  /// <summary>
  ///   Model units per texture pixel for planes generated from texture-only
  ///   GEOs.
  /// </summary>
  private const float TEXTURE_PLANE_SCALE_ = 0.01f;

  /// <summary>
  ///   Full brightness, since vertex colors are doubled (see CreateMaterial_()).
  /// </summary>
  private static ReadOnlySpan<byte> NEUTRAL_VERTEX_COLOR_
    => [0x80, 0x80, 0x80, 0xff];

  private readonly record struct TextureKey(
      int TextureIndex,
      int UvIndex,
      bool EnvironmentMap,
      bool UvScroll);

  /// <summary>
  ///   MaskTextureIndex is null unless the strip is a terrain blend, see
  ///   AddMesh_().
  /// </summary>
  private readonly record struct MaterialKey(
      int TextureIndex,
      int? MaskTextureIndex,
      GeoLayerFlags Flags,
      bool HasVertexColors);

  /// <summary>
  ///   Overlays are kept in separate primitives from regular geometry with
  ///   the same material, since they're drawn at a different priority.
  /// </summary>
  private readonly record struct PrimitiveKey(
      MaterialKey MaterialKey,
      byte OverlayPass);

  /// <summary>
  ///   How a layer is blended, from its flags. If several blending flags are
  ///   set, SUBTRACTIVE takes precedence, then ADDITIVE, then ALPHA_BLEND.
  /// </summary>
  private enum GeoBlendMode {
    OPAQUE,
    ALPHA_BLEND,
    ADDITIVE,
    SUBTRACTIVE,
  }

  public IModel Import(GeoModelFileBundle fileBundle) {
    IReadOnlyTreeFile geoFile = fileBundle.GeoFile;

    using IBinaryReader geoBr = geoFile.OpenReadAsBinary(Endianness.BigEndian);
    Geo geo = geoBr.ReadNew<Geo>();
    GeoHeader geoHeader = geo.Header;

    HashSet<IReadOnlyStandaloneFile> files = geoFile.AsFileSet();
    if (fileBundle.GdtFile != null) {
      files.Add(fileBundle.GdtFile);
    }

    ModelImpl finModel = new() {
      FileBundle = fileBundle,
      Files = files,
    };

    IMaterialManager finMaterialManager = finModel.MaterialManager;
    // UV generation and scrolling are properties of the texture rather than
    // the material, so separate textures are created for each combination,
    // sharing the same image.
    LazyDictionary<int, IReadOnlyImage> lazyImages
        = new(textureIndex => geo.Textures[textureIndex].ToImage(geoBr));
    LazyDictionary<TextureKey, ITexture> lazyFinTextures = new(key => {
      ITexture finTexture = key.UvScroll
          ? finMaterialManager.CreateScrollingTexture(
              lazyImages[key.TextureIndex],
              UV_SCROLL_SPEED_X_,
              UV_SCROLL_SPEED_Y_)
          : finMaterialManager.CreateTexture(lazyImages[key.TextureIndex]);
      finTexture.Name = GetTextureName_(geoFile, geo.Textures[key.TextureIndex]);
      finTexture.UvIndex = key.UvIndex;
      finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;
      if (key.EnvironmentMap) {
        finTexture.UvType = UvType.SPHERICAL;
      }

      return finTexture;
    });

    // The same texture can be drawn with different blending in different
    // strips, so a separate material is created for each combination.
    LazyDictionary<MaterialKey, IMaterial?> lazyFinMaterials
        = new(key => CreateMaterial_(finMaterialManager,
                                     lazyFinTextures,
                                     geo.Textures,
                                     key));

    // The game is left-handed, so the Z axis is flipped to convert it.
    IBone finRootBone = finModel.Skeleton.Root.AddRoot(0, 0, 0);
    finRootBone.Name = geoFile.NameWithoutExtension.ToString();
    finRootBone.Transform.SetScale(1, 1, -1);

    // Some GEOs (pause menu maps, wizard cards, fonts, etc.) only contain
    // textures, so a row of planes is generated to display them. Loading
    // screen GEOs contain a background, loading bar, and logo.
    if (geoHeader.TotalMeshCount == 0) {
      GeoTexture[] planeTextures = geo.Textures;
      float left = -planeTextures.Sum(t => t.Width) * TEXTURE_PLANE_SCALE_ / 2;
      for (int i = 0; i < planeTextures.Length; ++i) {
        GeoTexture geoTexture = planeTextures[i];
        AddTexturePlane_(finModel,
                         finRootBone,
                         geoTexture,
                         GetTextureName_(geoFile, geoTexture),
                         left,
                         lazyFinMaterials[new MaterialKey(
                             i,
                             null,
                             GeoLayerFlags.ALPHA_BLEND |
                             GeoLayerFlags.DOUBLE_SIDED,
                             false)]);
        left += geoTexture.Width * TEXTURE_PLANE_SCALE_;
      }

      return finModel;
    }

    GeoMeshTable meshTable = new(geoHeader, geoBr, fileBundle);

    // Single-mesh bundles hold a mesh that the layout doesn't place, so it's
    // shown at the origin.
    if (fileBundle.UnplacedMesh != null) {
      GeoMesh geoMesh
          = meshTable.ReadMesh(fileBundle.UnplacedMesh.MeshOrdinal);
      geoMesh.DiscardColorsIfUnlit();
      AddMesh_(finModel,
               finRootBone,
               geoMesh,
               Triangulate_(geoMesh),
               lazyFinMaterials);
      return finModel;
    }

    // Meshes that no layout object places (doors, gates, pickups, effects)
    // are positioned by the game at runtime, so they're left out of the level
    // model rather than piled up at the origin. They get their own
    // single-mesh bundles instead (see GeoUnplacedMeshCollector).
    if (geoHeader.LayoutObjects.Length > 0) {
      Dictionary<int, GeoMesh> geoMeshesByIndex
          = meshTable.ReadPlacedMeshes().ToDictionary(m => m.Index);
      // Layouts often place the same mesh many times, so each mesh is only
      // triangulated once.
      LazyDictionary<GeoMesh, Triangulation_> lazyTriangulations
          = new(Triangulate_);
      foreach (GeoLayoutObject geoLayoutObject in geoHeader.LayoutObjects) {
        if (!geoMeshesByIndex.TryGetValue(geoLayoutObject.MeshIndex,
                                          out GeoMesh? geoMesh)) {
          continue;
        }

        IBone finBone = finRootBone.AddChild(geoLayoutObject.Position);
        finBone.Name = geoMesh.Name;
        finBone.Transform.SetRotation(
            QuaternionUtil.CreateZyxRadians(-geoLayoutObject.RotationRadians));
        finBone.Transform.SetScale(geoLayoutObject.Scale);

        AddMesh_(finModel,
                 finBone,
                 geoMesh,
                 lazyTriangulations[geoMesh],
                 lazyFinMaterials);
      }
    } else {
      foreach (GeoMesh geoMesh in meshTable.ReadMeshes()
                                           .OrderBy(m => m.Index)) {
        AddMesh_(finModel,
                 finRootBone,
                 geoMesh,
                 Triangulate_(geoMesh),
                 lazyFinMaterials);
      }
    }

    return finModel;
  }

  private static string GetTextureName_(IReadOnlyTreeFile geoFile,
                                        GeoTexture geoTexture)
    => geoTexture.Name.Length > 0
        ? geoTexture.Name
        : $"{geoFile.NameWithoutExtension}_{geoTexture.Offset}";

  private static IMaterial? CreateMaterial_(
      IMaterialManager finMaterialManager,
      LazyDictionary<TextureKey, ITexture> lazyFinTextures,
      GeoTexture[] geoTextures,
      MaterialKey key) {
    (int textureIndex, int? maskTextureIndex, GeoLayerFlags flags,
     bool hasVertexColors) = key;
    if (!IsValidTextureIndex_(textureIndex, geoTextures)) {
      return null;
    }

    // Terrain blends' color layer always uses the second UV channel, even if
    // their mask texture is missing.
    bool isTerrainBlend = maskTextureIndex != null;
    ITexture finTexture = lazyFinTextures[
        new TextureKey(textureIndex,
                       isTerrainBlend ? 1 : 0,
                       flags.HasFlag(GeoLayerFlags.ENVIRONMENT_MAP),
                       flags.HasFlag(GeoLayerFlags.UV_SCROLL))];
    IFixedFunctionMaterial finMaterial
        = finMaterialManager.AddFixedFunctionMaterial();
    finMaterial.CullingMode = flags.HasFlag(GeoLayerFlags.DOUBLE_SIDED)
        ? CullingMode.SHOW_BOTH
        : CullingMode.SHOW_FRONT_ONLY;
    finMaterial.Shininess = MaterialConstants.DISABLED_SHININESS;

    IFixedFunctionEquations<FixedFunctionSource> equations
        = finMaterial.Equations;
    (IColorValue color, IScalarValue alpha) outputColorAlpha
        = finMaterial.GenerateDiffuse(
            (equations.ColorOps.One, equations.ScalarOps.One),
            finTexture,
            (hasVertexColors, false));

    // Lighting is baked into the vertex colors, where 0x80 is full
    // brightness, so they're doubled to let them overbrighten the
    // texture like in-game.
    if (hasVertexColors) {
      outputColorAlpha.color
          = equations.ColorOps.MultiplyWithConstant(outputColorAlpha.color, 2);
    }

    string name = finTexture.Name;

    // The mask's color is always black, so it only provides the alpha.
    if (isTerrainBlend &&
        IsValidTextureIndex_(maskTextureIndex!.Value, geoTextures)) {
      ITexture finMaskTexture
          = lazyFinTextures[
              new TextureKey(maskTextureIndex.Value, 0, false, false)];
      name = $"{finMaskTexture.Name}_{name}";
      (_, IScalarValue maskAlpha)
          = finMaterial.AddTextureSourceColorAlpha(finMaskTexture);
      outputColorAlpha.alpha
          = equations.ScalarOps.Multiply(outputColorAlpha.alpha, maskAlpha);
    }

    equations.SetOutputColorAlpha(outputColorAlpha);

    GeoBlendMode blendMode = GetBlendMode_(flags);
    ApplyBlendMode_(finMaterial, blendMode, flags);

    if (flags.HasFlag(GeoLayerFlags.ALPHA_TEST)) {
      finMaterial.SetDefaultAlphaCompare(TransparencyType.MASK);
    }

    // Covers everything in the key, so different materials don't share a name.
    finMaterial.Name
        = name +
          (hasVertexColors ? "" : "_nocolor") +
          (flags.HasFlag(GeoLayerFlags.DOUBLE_SIDED) ? "_2sided" : "") +
          (flags.HasFlag(GeoLayerFlags.ENVIRONMENT_MAP) ? "_env" : "") +
          (flags.HasFlag(GeoLayerFlags.UV_SCROLL) ? "_scroll" : "") +
          blendMode switch {
              GeoBlendMode.SUBTRACTIVE => "_subtract",
              GeoBlendMode.ADDITIVE => "_add",
              GeoBlendMode.ALPHA_BLEND => "_blend",
              _ => "",
          } +
          (flags.HasFlag(GeoLayerFlags.ALPHA_TEST) ? "_test" : "");

    return finMaterial;
  }

  /// <summary>
  ///   Textures in unknown formats are treated like missing ones, so the rest
  ///   of the model can still be imported.
  /// </summary>
  private static bool IsValidTextureIndex_(int textureIndex,
                                           GeoTexture[] geoTextures)
    => textureIndex >= 0 &&
       textureIndex < geoTextures.Length &&
       geoTextures[textureIndex].IsSupported;

  private static GeoBlendMode GetBlendMode_(GeoLayerFlags flags) {
    if (flags.HasFlag(GeoLayerFlags.SUBTRACTIVE)) {
      return GeoBlendMode.SUBTRACTIVE;
    }

    if (flags.HasFlag(GeoLayerFlags.ADDITIVE)) {
      return GeoBlendMode.ADDITIVE;
    }

    return flags.HasFlag(GeoLayerFlags.ALPHA_BLEND)
        ? GeoBlendMode.ALPHA_BLEND
        : GeoBlendMode.OPAQUE;
  }

  private static void ApplyBlendMode_(IFixedFunctionMaterial finMaterial,
                                      GeoBlendMode blendMode,
                                      GeoLayerFlags flags) {
    switch (blendMode) {
      case GeoBlendMode.SUBTRACTIVE: {
        // Shadows: white darkens the surface underneath, black leaves it
        // untouched. The destination alpha is preserved, since
        // subtracting from it would punch holes in the framebuffer's
        // alpha.
        finMaterial.SetBlendingSeparate(BlendEquation.REVERSE_SUBTRACT,
                                        BlendFactor.ONE,
                                        BlendFactor.ONE,
                                        BlendEquation.ADD,
                                        BlendFactor.ZERO,
                                        BlendFactor.ONE,
                                        LogicOp.UNDEFINED);
        finMaterial.DepthMode = DepthMode.READ_ONLY;
        break;
      }
      case GeoBlendMode.ADDITIVE: {
        // The viewer composites the framebuffer over a checkerboard using its
        // alpha, so the destination alpha is preserved; otherwise these would
        // undo the coverage of any translucent surfaces drawn before them.
        finMaterial.SetBlendingSeparate(BlendEquation.ADD,
                                        BlendFactor.SRC_ALPHA,
                                        BlendFactor.ONE,
                                        BlendEquation.ADD,
                                        BlendFactor.ZERO,
                                        BlendFactor.ONE,
                                        LogicOp.UNDEFINED);
        finMaterial.CullingMode = CullingMode.SHOW_BOTH;
        // Additive blending is order-independent, so these don't need to
        // write depth (which would otherwise occlude each other).
        finMaterial.DepthMode = DepthMode.READ_ONLY;
        break;
      }
      case GeoBlendMode.ALPHA_BLEND: {
        // Alpha uses the standard "over" operator (a + (1 - a) * dst), so
        // drawing over opaque geometry keeps the framebuffer opaque. Blending
        // alpha by SRC_ALPHA would lower it to a² + (1 - a), which lets the
        // viewer's checkerboard background show through the glass.
        finMaterial.SetBlendingSeparate(BlendEquation.ADD,
                                        BlendFactor.SRC_ALPHA,
                                        BlendFactor.ONE_MINUS_SRC_ALPHA,
                                        BlendEquation.ADD,
                                        BlendFactor.ONE,
                                        BlendFactor.ONE_MINUS_SRC_ALPHA,
                                        LogicOp.UNDEFINED);
        // Alpha-tested surfaces (foliage) discard their transparent parts, so
        // they can still write depth without occluding what's behind them.
        if (!flags.HasFlag(GeoLayerFlags.ALPHA_TEST)) {
          finMaterial.DepthMode = DepthMode.READ_ONLY;
        }

        break;
      }
      default: {
        // Without any blending flags, layers are drawn opaque even if their
        // texture has alpha. Otherwise they'd be treated as transparent and
        // drawn after (and over) actual transparent surfaces like glass.
        // Alpha testing still discards pixels, since that's independent of
        // blending.
        finMaterial.SetSimpleBlending(SimpleBlendMode.NONE);
        break;
      }
    }
  }

  /// <summary>
  ///   A mesh's triangles as strip vertices, grouped by primitive. Doesn't
  ///   depend on where the mesh is placed, so instances can share it.
  /// </summary>
  private sealed class Triangulation_
      : Dictionary<PrimitiveKey, List<GeoStripVertex>>;

  private static Triangulation_ Triangulate_(GeoMesh geoMesh) {
    bool hasVertexColors = geoMesh.Colors != null;

    Triangulation_ triangulation = [];
    foreach (GeoStrip geoStrip in geoMesh.Strips) {
      GeoStripVertex[] geoStripVertices = geoStrip.Vertices;
      GeoStripLayer[] geoLayers = geoStrip.Layers;
      if (geoLayers.Length == 0) {
        continue;
      }

      // The only strips with multiple layers are terrain blends, where both
      // layers are combined into a single pass: the first layer is a mask
      // that only provides the alpha, and the second layer provides the
      // color, tiled with its own texcoords.
      if (geoLayers.Length > 2) {
        throw new NotSupportedException(
            $"Strips with {geoLayers.Length} layers aren't supported yet!");
      }

      GeoLayerFlags flags = 0;
      foreach (GeoStripLayer geoLayer in geoLayers) {
        flags |= geoLayer.Flags & SUPPORTED_LAYER_FLAGS;
      }

      PrimitiveKey primitiveKey = new(
          new MaterialKey(geoLayers[^1].TextureIndex,
                          geoLayers.Length == 2
                              ? geoLayers[0].TextureIndex
                              : null,
                          flags,
                          hasVertexColors),
          geoStrip.OverlayPass);

      for (int k = 0; k < geoStripVertices.Length - 2; ++k) {
        GeoStripVertex v0 = geoStripVertices[k];
        GeoStripVertex v1 = geoStripVertices[k + 1];
        GeoStripVertex v2 = geoStripVertices[k + 2];

        // Skips degenerate triangles.
        if (v0.PositionIndex == v1.PositionIndex ||
            v1.PositionIndex == v2.PositionIndex ||
            v0.PositionIndex == v2.PositionIndex) {
          continue;
        }

        if (k % 2 == 1) {
          (v1, v2) = (v2, v1);
        }

        ref List<GeoStripVertex>? triangleVertices
            = ref CollectionsMarshal.GetValueRefOrAddDefault(
                triangulation,
                primitiveKey,
                out _);
        triangleVertices ??= [];
        triangleVertices.AddRange([v0, v1, v2]);
      }
    }

    return triangulation;
  }

  private static void AddMesh_(
      ModelImpl finModel,
      IReadOnlyBone finBone,
      GeoMesh geoMesh,
      Triangulation_ triangulation,
      LazyDictionary<MaterialKey, IMaterial?> lazyFinMaterials) {
    if (triangulation.Count == 0) {
      return;
    }

    (ISkin<NormalTangentMultiColorMultiUvVertexImpl> finSkin,
     IBoneWeights finBoneWeights) = GetSkinAndBoneWeights_(finModel, finBone);

    Vector3[] geoPositions = geoMesh.Positions;
    Vector2[] geoTexcoords = geoMesh.Texcoords;
    Vector3[]? geoNormals = geoMesh.Normals;
    byte[]? geoColors = geoMesh.Colors;
    bool hasTerrainBlends = geoMesh.Strips.Any(s => s.Layers.Length == 2);

    // Strips index positions and texcoords separately, so each unique
    // combination becomes one vertex. Colors are stored per texcoord of the
    // first texture layer, which is also used for the first UV channel. The
    // second UV channel is only used by terrain blends.
    LazyDictionary<GeoStripVertex, IReadOnlyVertex> finVerticesByGeoVertex
        = new(geoVertex => {
          (ushort positionIndex, ushort texcoordIndex0, ushort texcoordIndex1)
              = geoVertex;

          NormalTangentMultiColorMultiUvVertexImpl finVertex
              = AddVertex_(finSkin,
                           finBoneWeights,
                           geoPositions[positionIndex],
                           geoNormals?[positionIndex],
                           GetTexcoord_(geoTexcoords, texcoordIndex0));
          if (hasTerrainBlends) {
            finVertex.SetUv(1, GetTexcoord_(geoTexcoords, texcoordIndex1));
          }

          if (geoColors != null) {
            ReadOnlySpan<byte> color
                = (texcoordIndex0 + 1) * 4 <= geoColors.Length
                    ? geoColors.AsSpan(texcoordIndex0 * 4, 4)
                    : NEUTRAL_VERTEX_COLOR_;
            finVertex.SetColorBytes(color[0], color[1], color[2], color[3]);
          }

          return finVertex;
        });

    IMesh finMesh = finSkin.AddMesh();
    finMesh.Name = geoMesh.Name;

    foreach ((PrimitiveKey primitiveKey, List<GeoStripVertex> geoVertices) in
             triangulation) {
      IPrimitive finPrimitive
          = finMesh.AddTriangles(
                       geoVertices.ConvertAll(v => finVerticesByGeoVertex[v]))
                   .SetVertexOrder(VertexOrder.CLOCKWISE)
                   .SetInversePriority(
                       GetInversePriority_(primitiveKey.MaterialKey.Flags,
                                           primitiveKey.OverlayPass));

      IMaterial? finMaterial = lazyFinMaterials[primitiveKey.MaterialKey];
      if (finMaterial != null) {
        finPrimitive.SetMaterial(finMaterial);
      }
    }
  }

  /// <summary>
  ///   Adds an upright plane the size of the texture in pixels, with its
  ///   bottom-left corner at (left, 0).
  /// </summary>
  private static void AddTexturePlane_(ModelImpl finModel,
                                       IReadOnlyBone finBone,
                                       GeoTexture geoTexture,
                                       string name,
                                       float left,
                                       IMaterial? finMaterial) {
    (ISkin<NormalTangentMultiColorMultiUvVertexImpl> finSkin,
     IBoneWeights finBoneWeights) = GetSkinAndBoneWeights_(finModel, finBone);

    float right = left + geoTexture.Width * TEXTURE_PLANE_SCALE_;
    float height = geoTexture.Height * TEXTURE_PLANE_SCALE_;

    IReadOnlyVertex AddVertex(float x, float y, float u, float v)
      => AddVertex_(finSkin,
                    finBoneWeights,
                    new Vector3(x, y, 0),
                    new Vector3(0, 0, -1),
                    new Vector2(u, v));

    IMesh finMesh = finSkin.AddMesh();
    finMesh.Name = name;
    finMesh.AddQuads(AddVertex(left, height, 0, 0),
                     AddVertex(right, height, 1, 0),
                     AddVertex(right, 0, 1, 1),
                     AddVertex(left, 0, 0, 1))
           .SetMaterial(finMaterial);
  }

  private static (ISkin<NormalTangentMultiColorMultiUvVertexImpl>,
      IBoneWeights) GetSkinAndBoneWeights_(ModelImpl finModel,
                                           IReadOnlyBone finBone) {
    ISkin<NormalTangentMultiColorMultiUvVertexImpl> finSkin = finModel.Skin;
    return (finSkin,
            finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                           finBone));
  }

  private static NormalTangentMultiColorMultiUvVertexImpl AddVertex_(
      ISkin<NormalTangentMultiColorMultiUvVertexImpl> finSkin,
      IBoneWeights finBoneWeights,
      Vector3 position,
      Vector3? normal,
      Vector2 uv) {
    NormalTangentMultiColorMultiUvVertexImpl finVertex
        = finSkin.AddVertex(position);
    finVertex.SetBoneWeights(finBoneWeights);
    if (normal != null) {
      finVertex.SetLocalNormal(normal.Value);
    }

    finVertex.SetUv(0, uv);
    return finVertex;
  }

  private static Vector2 GetTexcoord_(Vector2[] geoTexcoords,
                                      ushort texcoordIndex)
    => texcoordIndex < geoTexcoords.Length
        ? geoTexcoords[texcoordIndex]
        : Vector2.Zero;

  /// <summary>
  ///   Transparent primitives within a model aren't depth-sorted, so they're
  ///   instead drawn in groups: overlays first (in pass order), since they're
  ///   part of the surfaces underneath them; then free-standing
  ///   alpha-blended surfaces like glass; then free-standing additive
  ///   glows/lightbeams last, since those look roughly correct regardless of
  ///   what they're drawn over. The few free-standing subtractive strips share
  ///   the first group; they still draw after opaque geometry, since
  ///   subtractive blending makes a material transparent.
  /// </summary>
  private static uint GetInversePriority_(GeoLayerFlags flags,
                                          byte overlayPass)
    => overlayPass > 0
        ? overlayPass - 1u
        : GetBlendMode_(flags) switch {
            GeoBlendMode.ADDITIVE => 0x101,
            GeoBlendMode.ALPHA_BLEND => 0x100,
            _ => 0,
        };
}
