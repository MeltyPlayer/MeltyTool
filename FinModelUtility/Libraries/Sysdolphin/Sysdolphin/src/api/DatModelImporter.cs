using System.Numerics;

using CommunityToolkit.HighPerformance;

using fin.data.lazy;
using fin.image;
using fin.image.formats;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.language.equations.fixedFunction.impl;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enums;
using fin.util.hex;

using gx;
using gx.displayList;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

using sysdolphin.schema;
using sysdolphin.schema.material;
using sysdolphin.schema.mesh;
using sysdolphin.schema.texture;

namespace sysdolphin.api;

public class DatModelImporter : IModelImporter<DatModelFileBundle> {
  public IModel Import(DatModelFileBundle modelFileBundle)
    => this.Import(modelFileBundle, out _, out _, out _, out _, out _);

  public unsafe IModel Import(
      DatModelFileBundle modelFileBundle,
      out HashSet<IReadOnlyGenericFile> files,
      out DatSubfile datSubfile,
      out IReadOnlyDictionary<JObj, IReadOnlyBone> outFinBoneByJObj,
      out IReadOnlyList<(JObj jObj,
          byte jObjIndex,
          DObj dObj,
          byte dObjIndex)> sortedJObjsAndDObjs,
      out IReadOnlyDictionary<DObj, IMesh> outFinMeshByDObj) {
    var dat = modelFileBundle.DatFile.ReadNew<Dat>(Endianness.BigEndian);
    datSubfile = dat.Subfiles.Single();

    files = modelFileBundle.Files.ToHashSet();
    var finModel = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = files
    };
    var finSkin = finModel.Skin;

    // Adds skeleton
    var jObjByOffset = datSubfile.JObjByOffset;
    var finBoneByJObj = new Dictionary<JObj, IReadOnlyBone>();
    outFinBoneByJObj = finBoneByJObj;
    var boneWeightsByJObj = new Dictionary<JObj, IBoneWeights>();
    var inverseBindMatrixByJObj =
        new Dictionary<JObj, IReadOnlyFinMatrix4x4>();
    var boneQueue = new Queue<(IBone finParentBone, JObj datBone)>();
    foreach (var datRootBone in datSubfile.RootJObjs) {
      boneQueue.Enqueue((finModel.Skeleton.Root, datRootBone));
    }

    Span<Matrix4x4> inverseBindMatrixBuffer = stackalloc Matrix4x4[1];
    Span<float> inverseBindMatrixFloatBuffer
        = inverseBindMatrixBuffer.Cast<Matrix4x4, float>();
    while (boneQueue.Count > 0) {
      var (finParentBone, jObj) = boneQueue.Dequeue();

      var finBone = finParentBone.AddChild(jObj.Position);
      finBone.LocalTransform.SetRotationRadians(jObj.RotationRadians);
      finBone.LocalTransform.SetScale(jObj.Scale);
      finBone.Name = jObj.Name;

      finBoneByJObj[jObj] = finBone;
      boneWeightsByJObj[jObj] =
          finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                         finBone);

      var inverseBindMatrixValues = jObj.InverseBindMatrixValues;
      if (inverseBindMatrixValues != null) {
        inverseBindMatrixValues.CopyTo(inverseBindMatrixFloatBuffer);
        inverseBindMatrixFloatBuffer[15] = 1;
      } else {
        inverseBindMatrixBuffer[0] = Matrix4x4.Identity;
      }

      inverseBindMatrixByJObj[jObj]
          = new FinMatrix4x4(inverseBindMatrixFloatBuffer).TransposeInPlace();

      foreach (var datChildBone in jObj.GetChildren()) {
        boneQueue.Enqueue((finBone, datChildBone));
      }
    }

    // Adds animations
    {
      var rootJointAnims
          = datSubfile
            .GetRootNodesOfType<SObj>()
            .SelectMany(sObj => sObj.JObjDescs?.Values ?? [])
            .SelectMany(jObjDesc => jObjDesc.JointAnimations?.Values ?? []);

      var i = 0;
      var jObjs = datSubfile.JObjs.ToArray();
      foreach (var rootJointAnim in rootJointAnims) {
        var finAnimation = finModel.AnimationManager.AddAnimation();
        finAnimation.Name = $"animation {i++}";
        finAnimation.FrameRate = 30;

        foreach (var (jObj, jointAnim) in jObjs.Zip(
                     rootJointAnim.GetSelfAndChildrenAndSiblings())) {
          var aObj = jointAnim.AObj;
          if (aObj == null) {
            continue;
          }

          finAnimation.FrameCount
              = Math.Max(finAnimation.FrameCount, (int) aObj.EndFrame);

          var finBone = finBoneByJObj[jObj];
          var boneTracks = finAnimation.GetOrCreateBoneTracks(finBone);

          DatBoneTracksHelper.AddDatKeyframesToBoneTracks(
              aObj.FObjs,
              boneTracks);
        }
      }
    }

    // Adds mesh and materials
    var mObjByOffset = new Dictionary<uint, MObj>();
    var tObjByOffset = new Dictionary<uint, TObj>();
    foreach (var jObj in datSubfile.JObjs) {
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

          FinMatrix4x4Util.FromTrs(tObj.Translation,
                                   tObj.RotationRadians.CreateZyxRadians(),
                                   tObj.Scale)
                          .InvertInPlace()
                          .Decompose(out var tObjTranslation,
                                     out var tObjRotation,
                                     out var tObjScale);
          finTexture.SetTranslation3d(tObjTranslation)
                    .SetRotationRadians3d(tObjRotation.ToEulerRadians())
                    .SetScale3d(tObjScale *
                                new Vector3(
                                    tObj.RepeatS,
                                    tObj.RepeatT,
                                    1));

          return finTexture;
        });
    var finMaterialsAndTextureMatricesByMObjOffset =
        new LazyDictionary<(uint, CullingMode), IMaterial?>(
            (mObjOffsetAndCullingMode => {
              var (mObjOffset, cullingMode) = mObjOffsetAndCullingMode;
              if (mObjOffset == 0) {
                return null;
              }

              var mObj = mObjByOffset[mObjOffset];
              var tObjsAndOffsets = mObj.TObjsAndOffsets.ToArray();

              var tObjsAndFinTextures =
                  new (TObj, ITexture)[tObjsAndOffsets.Length];
              for (var i = 0; i < tObjsAndOffsets.Length; i++) {
                var (tObjOffset, tObj) = tObjsAndOffsets[i];
                tObjsAndFinTextures[i] = (
                    tObj, finTexturesByTObjOffset[tObjOffset]);
              }

              var fixedFunctionMaterial =
                  finMaterialManager.AddFixedFunctionMaterial();
              fixedFunctionMaterial.Name = mObj.Name ?? mObjOffset.ToHex();
              fixedFunctionMaterial.CullingMode = cullingMode;

              var mObjMaterial = mObj.Material;
              fixedFunctionMaterial.Shininess = mObjMaterial.Shininess;
              // TODO: This results in some issues with sorting
              if (mObj.RenderMode.CheckFlag(RenderMode.NO_ZUPDATE)) {
                fixedFunctionMaterial.DepthMode = DepthMode.READ_ONLY;
              }

              this.PopulateFixedFunctionMaterial_(mObj,
                                                  tObjsAndFinTextures,
                                                  fixedFunctionMaterial);

              var peDesc = mObj.PeDesc;
              if (peDesc == null) {
                fixedFunctionMaterial.SetAlphaCompare(
                    AlphaCompareType.Greater,
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

              return fixedFunctionMaterial;
            }));

    // Sorts all dObjs so that the opaque ones are rendered first, and then the translucent (XLU) ones
    LinkedList<(JObj jObj, byte jObjIndex, DObj dObj, byte dObjIndex)>
        allJObjsAndDObjs = [];
    {
      byte jObjIndex = 0;
      foreach (var rootJObj in datSubfile.RootJObjs) {
        byte dObjIndex = 0;
        foreach (var jObj in rootJObj.GetSelfAndChildrenAndSiblings()) {
          foreach (var dObj in jObj.DObjs) {
            allJObjsAndDObjs.AddLast((jObj, jObjIndex, dObj, dObjIndex++));
          }
        }

        jObjIndex++;
      }
    }

    sortedJObjsAndDObjs
        = allJObjsAndDObjs
          .OrderBy(
              tuple => tuple.dObj.MObj?.RenderMode.CheckFlag(RenderMode.XLU) ??
                       false)
          .ToArray();

    var finMeshByDObj = new Dictionary<DObj, IMesh>();
    outFinMeshByDObj = finMeshByDObj;

    finSkin.AllowMaterialRendererMerging = false;
    foreach (var (jObj, _, dObj, _) in sortedJObjsAndDObjs) {
      var defaultBoneWeights = boneWeightsByJObj[jObj];
      var mObjOffset = dObj.MObjOffset;

      var finMesh = finSkin.AddMesh();
      finMeshByDObj[dObj] = finMesh;

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

        var finMaterial =
            finMaterialsAndTextureMatricesByMObjOffset[
                (mObjOffset, cullingMode)];

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
                      var uv0 = datVertex.Uv0.Value;
                      finVertex.SetUv(0, uv0.X, uv0.Y);
                    }

                    if (datVertex.Uv1 != null) {
                      var uv1 = datVertex.Uv1.Value;
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
              GxPrimitiveType.GX_TRIANGLES =>
                  finMesh.AddTriangles(finVertices),
              GxPrimitiveType.GX_QUADS => finMesh.AddQuads(finVertices),
              GxPrimitiveType.GX_TRIANGLE_STRIP => finMesh.AddTriangleStrip(
                  finVertices),
              _ => throw new ArgumentOutOfRangeException()
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

    var colorOps = equations.ColorOps;
    var scalarOps = equations.ScalarOps;

    for (var i = 0; i < tObjsAndFinTextures.Count; ++i) {
      var (_, finTexture) = tObjsAndFinTextures[i];
      fixedFunctionMaterial.SetTextureSource(i, finTexture);
    }

    var renderMode = mObj.RenderMode;
    var material = mObj.Material;

    var hasConstantRenderMode = renderMode.CheckFlag(RenderMode.CONSTANT);
    var hasDiffuseRenderMode = renderMode.CheckFlag(RenderMode.DIFFUSE);
    var hasSpecularRenderMode = renderMode.CheckFlag(RenderMode.SPECULAR);

    // Diffuse
    var diffuseRgba = hasConstantRenderMode || hasDiffuseRenderMode
        ? material.DiffuseColor
        : fin.schema.color.Rgba32.WHITE;
    var hasVertexColorAlpha = renderMode.CheckFlag(RenderMode.VERTEX); 
    var (diffuseSurfaceColor, diffuseSurfaceAlpha)
        = fixedFunctionMaterial.GenerateDiffuse(
            (equations.CreateColorConstant(diffuseRgba.Rf, diffuseRgba.Gf, diffuseRgba.Bf),
             equations.CreateScalarConstant(
                 material.DiffuseColor.Af * material!.Alpha)),
            null,
            (hasVertexColorAlpha, hasVertexColorAlpha));

    // Ambient
    IColorValue? ambientSurfaceColor = equations.CreateColorConstant(
        material.AmbientColor.Rf,
        material.AmbientColor.Gf,
        material.AmbientColor.Bf);

    IScalarValue? ambientSurfaceAlpha = equations.CreateScalarConstant(
        material.AmbientColor.Af);

    // Specular
    IColorValue? specularSurfaceColor = equations.CreateColorConstant(
        material.SpecularColor.Rf,
        material.SpecularColor.Gf,
        material.SpecularColor.Bf);

    IScalarValue? specularSurfaceAlpha = equations.CreateScalarConstant(
        material.SpecularColor.Af);

    // Lighting passes
    IColorValue ambientLightColor = ColorConstant.ZERO;
    IColorValue diffuseLightColor = ColorConstant.ONE;
    IColorValue specularLightColor = ColorConstant.ZERO;

    var lightingPasses = new LinkedList<TObjFlags>();
    lightingPasses.AddLast(TObjFlags.LIGHTMAP_DIFFUSE);

    // Shamelessly stolen from:
    // https://github.com/Ploaj/HSDLib/blob/93a906444f34951c6eed4d8c6172bba43d4ada98/HSDRawViewer/Shader/gx_material.frag#L81
    if (!(hasConstantRenderMode && !hasDiffuseRenderMode)) {
      lightingPasses.AddFirst(TObjFlags.LIGHTMAP_AMBIENT);
      ambientLightColor = equations.CreateOrGetColorInput(
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
    if (hasDiffuseRenderMode) {
      ambientAndDiffuseLightingColor =
          colorOps.MultiplyWithConstant(ambientAndDiffuseLightingColor, 2);
    }

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

    equations.SetOutputColorAlpha((outputColor, outputAlpha));
  }

  private void PerformTextureLightingPass_(
      TObj tObj,
      int textureIndex,
      IFixedFunctionEquations<FixedFunctionSource> equations,
      IColorOps colorOps,
      IScalarOps scalarOps,
      ref IColorValue color,
      ref IScalarValue alpha
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