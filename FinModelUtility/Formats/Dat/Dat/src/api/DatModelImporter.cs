using System.Numerics;

using dat.schema;
using dat.schema.animation;
using dat.schema.material;
using dat.schema.melee;
using dat.schema.mesh;
using dat.schema.texture;

using fin.data.lazy;
using fin.image;
using fin.image.formats;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.language.equations.fixedFunction.impl;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enums;
using fin.util.hex;
using fin.util.strings;

using gx;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

namespace dat.api;

// TODO: Split out this importer based on the game
public class DatModelImporter : IModelImporter<DatModelFileBundle> {
  public unsafe IModel Import(DatModelFileBundle modelFileBundle) {
    var primaryDat =
        modelFileBundle.PrimaryDatFile.ReadNew<Dat>(Endianness.BigEndian);
    var primaryDatSubfile = primaryDat.Subfiles.Single();

    var animationDat =
        modelFileBundle.AnimationDatFile?.ReadNew<Dat>(Endianness.BigEndian);
    var fighterDatSubfile =
        modelFileBundle
            .FighterDatFile?
            .ReadNew<Dat>(Endianness.BigEndian)
            .Subfiles
            .Single();

    var finModel = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = modelFileBundle.Files.ToHashSet()
    };
    var finSkin = finModel.Skin;

    // Adds skeleton
    var jObjByOffset = primaryDatSubfile.JObjByOffset;
    var finBoneByJObj = new Dictionary<JObj, IBone>();
    var boneWeightsByJObj = new Dictionary<JObj, IBoneWeights>();
    var inverseBindMatrixByJObj =
        new Dictionary<JObj, IReadOnlyFinMatrix4x4>();
    var boneQueue = new Queue<(IBone finParentBone, JObj datBone)>();
    foreach (var datRootBone in primaryDatSubfile.RootJObjs) {
      boneQueue.Enqueue((finModel.Skeleton.Root, datRootBone));
    }

    Span<float> inverseBindMatrixBuffer = stackalloc float[4 * 4];
    while (boneQueue.Count > 0) {
      var (finParentBone, jObj) = boneQueue.Dequeue();

      var finBone =
          finParentBone.AddChild(jObj.Position)
                       .SetLocalRotationRadians(jObj.RotationRadians)
                       .SetLocalScale(jObj.Scale);
      finBone.Name = jObj.Name;

      finBoneByJObj[jObj] = finBone;
      boneWeightsByJObj[jObj] =
          finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, finBone);

      var inverseBindMatrixValues = jObj.InverseBindMatrixValues;
      inverseBindMatrixValues.CopyTo(inverseBindMatrixBuffer);
      inverseBindMatrixBuffer[15] = 1;
      inverseBindMatrixByJObj[jObj] =
          new FinMatrix4x4(inverseBindMatrixBuffer).TransposeInPlace();

      foreach (var datChildBone in jObj.GetChildren()) {
        boneQueue.Enqueue((finBone, datChildBone));
      }
    }

    // Adds animations
    if (animationDat != null) {
      var lazyFinAnimations = new LazyList<IModelAnimation>(i => {
        var finAnimation = finModel.AnimationManager.AddAnimation();
        finAnimation.Name = $"Animation {i}";

        finAnimation.FrameRate = 60;

        return finAnimation;
      });

      var i = 0;
      foreach (var animationDatSubfile in animationDat.Subfiles) {
        foreach (var (figaTree, figaTreeName) in animationDatSubfile
                     .GetRootNodesWithNamesOfType<FigaTree>()) {
          var finAnimation = lazyFinAnimations[i++];
          finAnimation.Name = figaTreeName.SubstringAfter("Share_ACTION_")
                                          .SubstringUpTo("_figatree");
          finAnimation.FrameCount = (int) figaTree.FrameCount;

          foreach (var (jObj, trackNode) in primaryDatSubfile.JObjs.Zip(
                       figaTree.TrackNodes)) {
            var finBone = finBoneByJObj[jObj];
            var boneTracks = finAnimation.AddBoneTracks(finBone);
            DatBoneTracksHelper.AddDatKeyframesToBoneTracks(
                trackNode,
                boneTracks);
          }
        }
      }
    }

    // Adds mesh and materials
    var mObjByOffset = new Dictionary<uint, MObj>();
    var tObjByOffset = new Dictionary<uint, TObj>();
    foreach (var jObj in primaryDatSubfile.JObjs) {
      foreach (var dObj in jObj.DObjs) {
        var mObj = dObj.MObj;
        if (mObj != null) {
          mObjByOffset[dObj.MObjOffset] = mObj;
          foreach (var (tObjOffset, tObj) in mObj.TObjsAndOffsets) {
            tObjByOffset[tObjOffset] = tObj;
          }
        }
      }
    }

    List<HashSet<byte>>? lowPolyDObjs = null;
    if (fighterDatSubfile != null) {
      var fighterData =
          fighterDatSubfile.GetRootNodesOfType<MeleeFighterData>()
                           .Single();

      var lowPoly = fighterData.ModelLookupTables
                               .CostumeVisibilityLookupTable
                               ?.LowPoly;
      if (lowPoly != null) {
        lowPolyDObjs = [];
        foreach (var lookupEntry in lowPoly.LookupEntries) {
          var set = new HashSet<byte>();
          lowPolyDObjs.Add(set);

          foreach (var byteEntry in lookupEntry.ByteEntries) {
            set.Add(byteEntry);
          }
        }
      }
    }

    var finMaterialManager = finModel.MaterialManager;
    var finTexturesByTObjOffset =
        new LazyDictionary<uint, ITexture>(tObjOffset => {
          var tObj = tObjByOffset[tObjOffset];

          IImage image = tObj.Image;
          if (tObj.WrapT == GxWrapMode.GX_MIRROR) {
            var width = image.Width;
            var height = image.Height;

            var flippedImage =
                new Rgba32Image(image.PixelFormat, width, height);
            image.Access(getHandler => {
              using var flippedImageLock = flippedImage.Lock();
              var flippedImageScan0 = flippedImageLock.Pixels;
              for (var y = 0; y < height; ++y) {
                for (var x = 0; x < width; ++x) {
                  getHandler(x,
                             height - 1 - y,
                             out var r,
                             out var g,
                             out var b,
                             out var a);

                  flippedImageScan0[y * width + x] = new Rgba32(r, g, b, a);
                }
              }
            });

            image = flippedImage;
          }

          var finTexture = finMaterialManager.CreateTexture(image);
          finTexture.Name = tObj.Name ?? tObjOffset.ToHex();

          finTexture.MagFilter = tObj.MagFilter.ToFinMagFilter();

          var lod = tObj.Lod;
          if (lod != null) {
            finTexture.MinFilter = lod.MinFilter.ToFinMinFilter();
            finTexture.LodBias = lod.Bias;
          } else {
            finTexture.MinFilter = TextureMinFilter.LINEAR;
          }

          finTexture.WrapModeU = tObj.WrapS.ToFinWrapMode();
          finTexture.WrapModeV = tObj.WrapT.ToFinWrapMode();

          finTexture.UvIndex = tObj.TexGenSrc switch {
              >= GxTexGenSrc.Tex0 and <= GxTexGenSrc.Tex7
                  => tObj.TexGenSrc - GxTexGenSrc.Tex0
          };
          finTexture.UvType = tObj.Flags.GetCoord() switch {
              Coord.UV         => UvType.STANDARD,
              Coord.REFLECTION => UvType.SPHERICAL,
              _                => UvType.STANDARD
          };

          return finTexture;
        });
    var finTextureScalerByTObjOffset
        = new LazyDictionary<uint, Func<Vector2, Vector2>>(
            tObjOffset => {
              var tObj = tObjByOffset[tObjOffset];

              // Why tf does Melee have 3D texture transforms......
              // https://github.com/Ploaj/HSDLib/blob/93a906444f34951c6eed4d8c6172bba43d4ada98/HSDRawViewer/Converters/ModelExporter.cs#L526
              var tObjTranslation = tObj.Translation;
              var tObjRotationRadians = tObj.RotationRadians;
              var tObjScale = tObj.Scale;

              var rawTranslation = new Vector3(
                  tObjTranslation.X,
                  tObjTranslation.Y,
                  tObjTranslation.Z);
              var rawQuaternion = QuaternionUtil.CreateZyx(
                  tObjRotationRadians.X,
                  tObjRotationRadians.Y,
                  tObjRotationRadians.Z);
              var rawScale =
                  new Vector3(tObjScale.X, tObjScale.Y, tObjScale.Z);

              // This is an absolute nightmare, but it works.
              var uvMatrix = FinMatrix4x4Util
                             .FromTrs(rawTranslation, rawQuaternion, rawScale)
                             .InvertInPlace();

              return uv => uvMatrix.TransformPosition(uv) *
                           new Vector2(tObj.RepeatS, tObj.RepeatT);
            });
    var finMaterialsAndTextureMatricesByMObjOffset =
        new LazyDictionary<(uint, CullingMode), (IMaterial, Func<Vector2, Vector2>[])?>(
            (mObjOffsetAndCullingMode => {
              var (mObjOffset, cullingMode) = mObjOffsetAndCullingMode;
              if (mObjOffset == 0) {
                return null;
              }

              var mObj = mObjByOffset[mObjOffset];
              var tObjsAndOffsets = mObj.TObjsAndOffsets.ToArray();

              var tObjsAndFinTextures =
                  new (TObj, ITexture)[tObjsAndOffsets.Length];
              var textureScalers = new Func<Vector2, Vector2>[tObjsAndFinTextures.Length];
              for (var i = 0; i < tObjsAndOffsets.Length; i++) {
                var (tObjOffset, tObj) = tObjsAndOffsets[i];
                tObjsAndFinTextures[i] = (
                    tObj, finTexturesByTObjOffset[tObjOffset]);
                textureScalers[i] = finTextureScalerByTObjOffset[tObjOffset];
              }

              var fixedFunctionMaterial =
                  finMaterialManager.AddFixedFunctionMaterial();
              fixedFunctionMaterial.CullingMode = cullingMode;

              var mObjMaterial = mObj.Material;
              fixedFunctionMaterial.Shininess = mObjMaterial.Shininess;
              // TODO: This results in some issues with sorting
              if (mObj.RenderMode.CheckFlag(RenderMode.NO_ZUPDATE)) {
                fixedFunctionMaterial.DepthMode =
                    DepthMode.SKIP_WRITE_TO_DEPTH_BUFFER;
              }

              this.PopulateFixedFunctionMaterial_(mObj,
                                                  tObjsAndFinTextures,
                                                  fixedFunctionMaterial);

              var peDesc = mObj.PeDesc;
              if (peDesc == null) {
                fixedFunctionMaterial.SetAlphaCompare(
                    AlphaOp.Or,
                    AlphaCompareType.Greater,
                    0,
                    AlphaCompareType.Never,
                    0);
              } else {
                fixedFunctionMaterial.DepthCompareType =
                    peDesc.DepthFunction.ToFinDepthCompareType();

                new GxFixedFunctionBlending().ApplyBlending(
                    fixedFunctionMaterial,
                    peDesc.BlendMode,
                    peDesc.SrcFactor,
                    peDesc.DstFactor,
                    peDesc.BlendOp);
                fixedFunctionMaterial.SetAlphaCompare(
                    peDesc.AlphaOp.ToFinAlphaOp(),
                    peDesc.AlphaComp0.ToFinAlphaCompareType(),
                    peDesc.AlphaRef0,
                    peDesc.AlphaComp1.ToFinAlphaCompareType(),
                    peDesc.AlphaRef1);
              }

              fixedFunctionMaterial.Name = mObj.Name ?? mObjOffset.ToHex();
              return (fixedFunctionMaterial, textureScalers);
            }));

    // Sorts all dObjs so that the opaque ones are rendered first, and then the translucent (XLU) ones
    LinkedList<(JObj jObj, byte jObjIndex, DObj dObj, byte dObjIndex)>
        allJObjsAndDObjs = [];
    {
      byte jObjIndex = 0;
      foreach (var rootJObj in primaryDatSubfile.RootJObjs) {
        byte dObjIndex = 0;
        foreach (var jObj in rootJObj.GetSelfAndChildrenAndSiblings()) {
          foreach (var dObj in jObj.DObjs) {
            allJObjsAndDObjs.AddLast((jObj, jObjIndex, dObj, dObjIndex++));
          }
        }

        jObjIndex++;
      }
    }

    var sortedJObjsAndDObjs =
        allJObjsAndDObjs
            .Where(
                tuple => !(
                    tuple.dObj.MObj?.RenderMode.CheckFlag(RenderMode.XLU) ??
                    false))
            .Concat(
                allJObjsAndDObjs.Where(
                    tuple
                        => tuple.dObj.MObj?.RenderMode
                                .CheckFlag(RenderMode.XLU) ??
                           false));

    finSkin.AllowMaterialRendererMerging = false;
    var finMesh = finSkin.AddMesh();
    foreach (var (jObj, jObjIndex, dObj, dObjIndex) in sortedJObjsAndDObjs) {
      if (lowPolyDObjs != null && jObjIndex < lowPolyDObjs.Count) {
        var lowPolyDObjsSet = lowPolyDObjs[jObjIndex];
        if (lowPolyDObjsSet.Contains(dObjIndex)) {
          continue;
        }
      }

      var defaultBoneWeights = boneWeightsByJObj[jObj];
      var mObjOffset = dObj.MObjOffset;

      // TODO: Use fighter file to choose only high-poly meshes

      // Adds polygons
      foreach (var pObj in dObj.PObjs) {
        var pObjFlags = pObj.Header.Flags;
        var cullingMode = (pObjFlags.CheckFlag(PObjFlags.CULLFRONT),
                           pObjFlags.CheckFlag(PObjFlags.CULLBACK))
            switch {
                (true, true)  => CullingMode.SHOW_BOTH,
                (true, false) => CullingMode.SHOW_FRONT_ONLY,
                (false, true) => CullingMode.SHOW_BACK_ONLY,
                _             => CullingMode.SHOW_BOTH
            };

        var tuple = 
            finMaterialsAndTextureMatricesByMObjOffset[(mObjOffset, cullingMode)];
        var finMaterial = tuple?.Item1;
        var textureScalers = tuple?.Item2;

        var vertexSpace = pObj.VertexSpace;
        var finWeights =
            pObj.Weights
                ?.Select(
                    pObjWeights => finSkin.GetOrCreateBoneWeights(
                        vertexSpace,
                        pObjWeights
                            .Select(
                                pObjWeight => {
                                  var jObj =
                                      jObjByOffset[pObjWeight.JObjOffset];
                                  return new BoneWeight(
                                      finBoneByJObj[jObj],
                                      inverseBindMatrixByJObj[jObj],
                                      pObjWeight.Weight
                                  );
                                })
                            .ToArray()))
                .ToArray();

        foreach (var datPrimitive in pObj.Primitives) {
          var finVertices =
              datPrimitive
                  .Vertices
                  .Select(datVertex => {
                    var finVertex = finSkin.AddVertex(datVertex.Position);
                    finVertex.SetLocalNormal(datVertex.Normal);
                    // TODO: Add support for binormal/tangents for bump-mapping

                    finVertex.SetColor(datVertex.Color);

                    if (datVertex.Uv0 != null) {
                      var uv0 = textureScalers[0](datVertex.Uv0.Value);
                      finVertex.SetUv(0, uv0.X, uv0.Y);
                    }

                    if (datVertex.Uv1 != null) {
                      var uv1 = textureScalers[1](datVertex.Uv1.Value);
                      finVertex.SetUv(1, uv1.X, uv1.Y);
                    }

                    // TODO: Is this right???
                    if (datVertex.WeightId != null) {
                      if (finWeights != null) {
                        finVertex.SetBoneWeights(
                            finWeights[datVertex.WeightId.Value]);
                      }
                    } else {
                      finVertex.SetBoneWeights(defaultBoneWeights);
                    }

                    return finVertex;
                  })
                  .ToArray();

          var finPrimitive = datPrimitive.Type switch {
              GxOpcode.DRAW_TRIANGLES =>
                  finMesh.AddTriangles(finVertices),
              GxOpcode.DRAW_QUADS => finMesh.AddQuads(finVertices),
              GxOpcode.DRAW_TRIANGLE_STRIP => finMesh.AddTriangleStrip(
                  finVertices)
          };

          if (finMaterial != null) {
            finPrimitive.SetMaterial(finMaterial);
          }
        }
      }
    }

    return finModel;
  }

  /// <summary>
  ///   Shamelessly copied from:
  ///   https://github.com/Ploaj/HSDLib/blob/93a906444f34951c6eed4d8c6172bba43d4ada98/HSDRawViewer/Shader/gx_lightmap.frag
  /// </summary>
  private void PopulateFixedFunctionMaterial_(
      MObj mObj,
      IReadOnlyList<(TObj, ITexture)> tObjsAndFinTextures,
      IFixedFunctionMaterial fixedFunctionMaterial) {
    var equations = fixedFunctionMaterial.Equations;

    var colorOps = new ColorFixedFunctionOps(equations);
    var scalarOps = new ScalarFixedFunctionOps(equations);

    for (var i = 0; i < tObjsAndFinTextures.Count; ++i) {
      var (_, finTexture) = tObjsAndFinTextures[i];
      fixedFunctionMaterial.SetTextureSource(i, finTexture);
    }

    var vertexColor = equations.CreateOrGetColorInput(
        FixedFunctionSource.VERTEX_COLOR_0);
    var vertexAlpha = equations.CreateOrGetScalarInput(
        FixedFunctionSource.VERTEX_ALPHA_0);

    var renderMode = mObj.RenderMode;
    var material = mObj.Material;

    IColorValue? diffuseSurfaceColor = colorOps.One;
    IScalarValue? diffuseSurfaceAlpha = scalarOps.One;

    // Constant color
    var diffuseRgba = material.DiffuseColor;
    diffuseSurfaceColor = equations.CreateColorConstant(
        diffuseRgba.Rf,
        diffuseRgba.Gf,
        diffuseRgba.Bf);
    diffuseSurfaceAlpha = scalarOps.MultiplyWithConstant(
        diffuseSurfaceAlpha,
        material.DiffuseColor.Af * material!.Alpha);

    // Vertex color
    if (renderMode.CheckFlag(RenderMode.VERTEX)) {
      diffuseSurfaceColor =
          colorOps.Multiply(diffuseSurfaceColor, vertexColor);
      diffuseSurfaceAlpha =
          scalarOps.Multiply(diffuseSurfaceAlpha, vertexAlpha);
    }

    IColorValue? ambientSurfaceColor = equations.CreateColorConstant(
        material.AmbientColor.Rf,
        material.AmbientColor.Gf,
        material.AmbientColor.Bf);

    IScalarValue? ambientSurfaceAlpha = equations.CreateScalarConstant(
        material.AmbientColor.Af);

    IColorValue? specularSurfaceColor = equations.CreateColorConstant(
        material.SpecularColor.Rf,
        material.SpecularColor.Gf,
        material.SpecularColor.Bf);

    IScalarValue? specularSurfaceAlpha = equations.CreateScalarConstant(
        material.SpecularColor.Af);


    // Lighting passes
    var hasConstantRenderMode = renderMode.CheckFlag(RenderMode.CONSTANT);
    var hasDiffuseRenderMode = renderMode.CheckFlag(RenderMode.DIFFUSE);
    var hasSpecularRenderMode = renderMode.CheckFlag(RenderMode.SPECULAR);

    IColorValue? ambientLightColor = null;
    IColorValue diffuseLightColor = colorOps.One;
    IColorValue? specularLightColor = null;

    var lightingPasses = new LinkedList<TObjFlags>();
    lightingPasses.AddLast(TObjFlags.LIGHTMAP_DIFFUSE);

    // Shamelessly stolen from:
    // https://github.com/Ploaj/HSDLib/blob/93a906444f34951c6eed4d8c6172bba43d4ada98/HSDRawViewer/Shader/gx_material.frag#L81
    if (!(hasConstantRenderMode && !hasDiffuseRenderMode)) {
      lightingPasses.AddFirst(TObjFlags.LIGHTMAP_AMBIENT);
      ambientSurfaceColor = equations.CreateOrGetColorInput(
          FixedFunctionSource.LIGHT_AMBIENT_COLOR);

      if (hasDiffuseRenderMode) {
        diffuseLightColor = equations.GetMergedLightDiffuseColor();
      }

      if (hasSpecularRenderMode) {
        lightingPasses.AddLast(TObjFlags.LIGHTMAP_SPECULAR);
        specularLightColor = equations.GetMergedLightSpecularColor();
      }
    }

    foreach (var lightingPass in lightingPasses) {
      IColorValue? color;
      IScalarValue? alpha;

      switch (lightingPass) {
        case TObjFlags.LIGHTMAP_DIFFUSE: {
          color = diffuseSurfaceColor;
          alpha = diffuseSurfaceAlpha;
          break;
        }
        case TObjFlags.LIGHTMAP_AMBIENT: {
          color = ambientSurfaceColor;
          alpha = ambientSurfaceAlpha;
          break;
        }
        case TObjFlags.LIGHTMAP_SPECULAR: {
          color = specularSurfaceColor;
          alpha = specularSurfaceAlpha;
          break;
        }
        default: throw new NotImplementedException();
      }

      for (var i = 0; i < tObjsAndFinTextures.Count; ++i) {
        var (tObj, _) = tObjsAndFinTextures[i];
        if (!tObj.Flags.CheckFlag(lightingPass)) {
          continue;
        }

        this.PerformTextureLightingPass_(
            tObj,
            i,
            equations,
            colorOps,
            scalarOps,
            ref color,
            ref alpha);

        switch (lightingPass) {
          case TObjFlags.LIGHTMAP_DIFFUSE: {
            diffuseSurfaceColor = color;
            diffuseSurfaceAlpha = alpha;
            break;
          }
          case TObjFlags.LIGHTMAP_AMBIENT: {
            ambientSurfaceColor = color;
            ambientSurfaceAlpha = alpha;
            break;
          }
          case TObjFlags.LIGHTMAP_SPECULAR: {
            specularSurfaceColor = color;
            specularSurfaceAlpha = alpha;
            break;
          }
        }
      }
    }

    var ambientAndDiffuseLightingColor = colorOps.Add(
        colorOps.Multiply(ambientSurfaceColor, ambientLightColor),
        diffuseLightColor);

    // We double it because all the other kids do. (Other fixed-function games.)
    ambientAndDiffuseLightingColor =
        colorOps.MultiplyWithConstant(ambientAndDiffuseLightingColor, 2);

    var ambientAndDiffuseComponent = colorOps.Multiply(
        ambientAndDiffuseLightingColor,
        diffuseSurfaceColor);

    var specularComponent =
        colorOps.Multiply(specularSurfaceColor, specularLightColor);

    // Performs ext lighting pass
    var extLightingColor = colorOps.Add(
        ambientAndDiffuseComponent,
        specularComponent);
    var extLightingAlpha = diffuseSurfaceAlpha;

    for (var i = 0; i < tObjsAndFinTextures.Count; ++i) {
      var (tObj, _) = tObjsAndFinTextures[i];
      if (!tObj.Flags.CheckFlag(TObjFlags.LIGHTMAP_EXT)) {
        continue;
      }

      this.PerformTextureLightingPass_(
          tObj,
          i,
          equations,
          colorOps,
          scalarOps,
          ref extLightingColor,
          ref extLightingAlpha);
    }

    // Sets up output colors
    var outputColor = extLightingColor;
    var outputAlpha = diffuseSurfaceAlpha;

    equations.CreateColorOutput(FixedFunctionSource.OUTPUT_COLOR,
                                outputColor ?? colorOps.Zero);
    equations.CreateScalarOutput(FixedFunctionSource.OUTPUT_ALPHA,
                                 outputAlpha ?? scalarOps.Zero);
  }

  private void PerformTextureLightingPass_(
      TObj tObj,
      int textureIndex,
      IFixedFunctionEquations<FixedFunctionSource> equations,
      ColorFixedFunctionOps colorOps,
      ScalarFixedFunctionOps scalarOps,
      ref IColorValue? color,
      ref IScalarValue? alpha
  ) {
    var textureColor = equations.CreateOrGetColorInput(
        FixedFunctionSource.TEXTURE_COLOR_0 + textureIndex);
    var textureAlpha = equations.CreateOrGetScalarInput(
        FixedFunctionSource.TEXTURE_ALPHA_0 + textureIndex);

    switch (tObj.Flags.GetColorMap()) {
      case ColorMap.NONE:
      case ColorMap.PASS: {
        // As you might guess from the name, does nothing.
        break;
      }
      case ColorMap.ALPHA_MASK: {
        // TODO: Is this right?
        color = colorOps.MixWithScalar(color, textureColor, textureAlpha);
        break;
      }
      case ColorMap.RGB_MASK: {
        // TODO: What should this do?
        break;
      }
      case ColorMap.BLEND: {
        color = colorOps.MixWithConstant(color,
                                         textureColor,
                                         tObj.Blending);
        break;
      }
      case ColorMap.MODULATE: {
        color = colorOps.Multiply(color, textureColor);
        break;
      }
      case ColorMap.REPLACE: {
        color = textureColor;
        break;
      }
      case ColorMap.ADD: {
        color = colorOps.Add(
            color,
            colorOps.MultiplyWithScalar(textureColor, textureAlpha));
        break;
      }
      case ColorMap.SUB: {
        color = colorOps.Subtract(
            color,
            colorOps.MultiplyWithScalar(textureColor, textureAlpha));
        break;
      }
    }

    switch (tObj.Flags.GetAlphaMap()) {
      case AlphaMap.NONE:
      case AlphaMap.PASS: {
        // As you might guess from the name, does nothing.
        break;
      }
      case AlphaMap.ALPHA_MASK: {
        // TODO: What should this do?
        break;
      }
      case AlphaMap.BLEND: {
        alpha = scalarOps.MixWithConstant(
            alpha,
            textureAlpha,
            tObj.Blending);
        break;
      }
      case AlphaMap.MODULATE: {
        alpha = scalarOps.Multiply(alpha, textureAlpha);
        break;
      }
      case AlphaMap.REPLACE: {
        alpha = textureAlpha;
        break;
      }
      case AlphaMap.ADD: {
        alpha = scalarOps.Add(alpha, textureAlpha);
        break;
      }
      case AlphaMap.SUB: {
        alpha = scalarOps.Subtract(alpha, textureAlpha);
        break;
      }
    }
  }
}