using System.Numerics;

using fin.data.dictionaries;
using fin.data.lazy;
using fin.image;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using rollingMadness.schema;

namespace rollingMadness.api;

public record AseMeshModelFileBundle(
    IReadOnlyTreeFile AseMeshFile,
    IReadOnlyTreeDirectory TextureDirectory,
    IReadOnlyList<AseAnimationMetadata>? AnimationMetadata = null,
    float? Specular = null,
    IReadOnlyList<IReadOnlyTreeFile>? MetadataFiles = null) : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.AseMeshFile;
}

public sealed class AseMeshModelImporter
    : IModelImporter<AseMeshModelFileBundle> {
  public IModel Import(AseMeshModelFileBundle fileBundle) {
    var aseMesh = fileBundle.AseMeshFile.ReadNew<AseMesh>();

    var frameCount = checked((int) aseMesh.AdditionalFrameCount + 1);
    if (aseMesh.Vertices.Length % frameCount != 0) {
      throw new InvalidDataException(
          $"Vertex count {aseMesh.Vertices.Length} is not divisible by " +
          $"the ASE mesh frame count {frameCount}.");
    }

    var verticesPerFrame = aseMesh.Vertices.Length / frameCount;
    if (verticesPerFrame == 0 && aseMesh.Triangles.Length > 0) {
      throw new InvalidDataException(
          "ASE mesh contains triangles but has no vertices.");
    }
    if (aseMesh.UvDatas.Length != verticesPerFrame) {
      throw new InvalidDataException(
          $"UV count {aseMesh.UvDatas.Length} does not match the " +
          $"per-frame vertex count {verticesPerFrame}.");
    }

    foreach (var triangle in aseMesh.Triangles) {
      if (triangle.MainTextureIndex < 0 ||
          triangle.MainTextureIndex >= aseMesh.ImageNames.Length) {
        throw new InvalidDataException(
            $"Triangle main texture index {triangle.MainTextureIndex} is " +
            $"outside the image table (count: {aseMesh.ImageNames.Length}).");
      }
      if (triangle.DecalTextureIndex < -1 ||
          triangle.DecalTextureIndex >= aseMesh.ImageNames.Length) {
        throw new InvalidDataException(
            $"Triangle decal texture index {triangle.DecalTextureIndex} is " +
            $"outside the image table (count: {aseMesh.ImageNames.Length}).");
      }
      if (triangle.LightmapIndex < -1 ||
          triangle.LightmapIndex >= aseMesh.LightmapNames.Length) {
        throw new InvalidDataException(
            $"Triangle lightmap index {triangle.LightmapIndex} is outside " +
            $"the lightmap table (count: {aseMesh.LightmapNames.Length}).");
      }
    }

    var fileSet = fileBundle.AseMeshFile.AsFileSet();
    if (fileBundle.MetadataFiles != null) {
      fileSet.UnionWith(fileBundle.MetadataFiles);
    }
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileSet,
    };

    var finMaterialManager = finModel.MaterialManager;
    var lazyTextureMap
        = new LazyDictionary<(string aseImageName, int uvIndex, bool isLightmap),
            ITexture>(tuple => {
          var (aseImageName, uvIndex, isLightmap) = tuple;

          var imageFile = fileBundle.TextureDirectory.AssertGetExistingFile(
              aseImageName);
          fileSet.Add(imageFile);
          if (fileBundle.TextureDirectory.TryToGetExistingFile(
                  aseImageName + ".txt", out var textureSidecar)) {
            fileSet.Add(textureSidecar);
          }
          var finImage = FinImage.FromFile(imageFile);

          var finTexture = finMaterialManager.CreateTexture(finImage);
          // NameWithoutExtension truncates at the first dot, collapsing
          // lev1.ase.0.png through lev1.ase.5.png to the same name ("lev1").
          // Remove only the final extension so exporters retain six identities.
          finTexture.Name = System.IO.Path.GetFileNameWithoutExtension(
              imageFile.Name.ToString());
          finTexture.MinFilter = TextureMinFilter.LINEAR;

          finTexture.UvIndex = uvIndex;

          if (!isLightmap) {
            finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;
          }

          return finTexture;
        });
    var lazyMaterialMap
        = new LazyDictionary<(int aseMainTextureIndex, int aseDecalTextureIndex,
            int aseLightmapIndex, int renderFlags), IReadOnlyMaterial>(tuple => {
          var (aseMainTextureIndex, aseDecalTextureIndex, aseLightmapIndex,
              renderFlags) = tuple;

          var finMaterial = finMaterialManager.AddFixedFunctionMaterial();

          var finMainTexture = lazyTextureMap[
              (aseMesh.ImageNames[aseMainTextureIndex].Value, 0, false)];
          finMaterial.Name = finMainTexture.Name;

          var (diffuseColor, diffuseAlpha)
              = finMaterial.AddTextureSourceColorAlpha(finMainTexture);

          var equations = finMaterial.Equations;
          if (aseDecalTextureIndex >= 0) {
            var finDecalTexture = lazyTextureMap[
                (aseMesh.ImageNames[aseDecalTextureIndex].Value,
                 1, false)];

            finMaterial.Name += $"/{finDecalTexture.Name}";

            var (decalColor, decalAlpha)
                = finMaterial.AddTextureSourceColorAlpha(finDecalTexture);

            diffuseColor = equations.ColorOps.MixWithScalar(
                    diffuseColor,
                    decalColor,
                    decalAlpha);
          }

          IColorValue ambientColor = equations.CreateOrGetColorInput(
                  FixedFunctionSource.LIGHT_AMBIENT_COLOR);
          if (aseLightmapIndex >= 0) {
            var aseLightmapName
                = aseMesh.LightmapNames[aseLightmapIndex].Value;
            var finLightmapTexture = lazyTextureMap[(aseLightmapName, 2, true)];
            finMaterial.Name += $"/{finLightmapTexture.Name}";
            ambientColor = ambientColor.Multiply(
                finMaterial.AddTextureSourceColor(finLightmapTexture));
          }

          // Flags are part of the face material identity even where their
          // individual bit meanings are not yet known. Keeping them separate
          // prevents faces with different original render state from being
          // irreversibly merged by exporters.
          if (renderFlags != 0) {
            finMaterial.Name += $"/flags_0x{renderFlags:X8}";
          }

          var outputColorAlpha
              = equations.GenerateLighting(
                  (diffuseColor, diffuseAlpha), ambientColor,
                  fileBundle.Specular is { } specular
                      ? equations.CreateColorConstant(specular)
                      : equations.ColorOps.Zero,
                  equations.ColorOps.Zero);

          equations.SetOutputColorAlpha(outputColorAlpha);

          return finMaterial;
        });

    var finSkin = finModel.Skin;
    var finMesh = finSkin.AddMesh();

    // UVs and triangle indices address one frame. ASE decal wrapping is
    // triangle-local, so vertices must be split at triangle boundaries. This
    // matches the game loader and prevents a decal crossing a repeat seam from
    // being interpolated across the entire texture.
    var baseFrameVertices = aseMesh.Vertices.AsSpan(0, verticesPerFrame)
                                           .ToArray();
    var importedTriangles = new List<(
        Triangle triangle,
        (IReadOnlyVertex vertex, int sourceIndex)[] corners)>();

    foreach (var triangle in aseMesh.Triangles) {
      var sourceIndices = new[] {
          checked((int) triangle.Vertex1),
          checked((int) triangle.Vertex2),
          checked((int) triangle.Vertex3),
      };
      if (sourceIndices.Any(index => index < 0 || index >= verticesPerFrame)) {
        throw new InvalidDataException(
            $"Triangle references a vertex outside the base frame " +
            $"(vertex count: {verticesPerFrame}).");
      }

      var decalCenter = sourceIndices.Select(index => aseMesh.UvDatas[index].Uv1)
                                     .Aggregate(Vector2.Zero,
                                                (sum, uv) => sum +
                                                    new Vector2(uv.X,
                                                                1 - uv.Y)) /
                        3;
      var decalShift = new Vector2(-MathF.Floor(decalCenter.X),
                                   -MathF.Floor(decalCenter.Y));

      var corners = sourceIndices.Select(sourceIndex => {
        var aseVertex = baseFrameVertices[sourceIndex];
        var aseUvData = aseMesh.UvDatas[sourceIndex];
        var finVertex = finSkin.AddVertex(aseVertex.Position);
        if (aseVertex.Normal.LengthSquared() > 0) {
          finVertex.SetLocalNormal(Vector3.Normalize(aseVertex.Normal));
        }

        finVertex.SetUv(0, new Vector2(aseUvData.Uv0.X, 1 - aseUvData.Uv0.Y));
        finVertex.SetUv(1, new Vector2(aseUvData.Uv1.X,
                                      1 - aseUvData.Uv1.Y) + decalShift);
        finVertex.SetUv(2, new Vector2(aseUvData.LightmapUv.X,
                                      1 - aseUvData.LightmapUv.Y));
        return ((IReadOnlyVertex) finVertex, sourceIndex);
      }).ToArray();

      importedTriangles.Add((triangle, corners));
    }

    var importedVertices = importedTriangles.SelectMany(entry => entry.corners)
                                            .ToArray();

    var morphTargetFrames = new IMorphTarget?[frameCount];
    for (var frameIndex = 1; frameIndex < frameCount; ++frameIndex) {
      var morphTarget = finModel.AnimationManager.AddMorphTarget();
      morphTarget.Name = $"frame_{frameIndex}";
      morphTargetFrames[frameIndex] = morphTarget;

      var frameOffset = frameIndex * verticesPerFrame;
      foreach (var (finVertex, sourceIndex) in importedVertices) {
        var aseVertex = aseMesh.Vertices[frameOffset + sourceIndex];
        morphTarget.SetNewLocalPosition(finVertex, aseVertex.Position);

        if (aseVertex.Normal.LengthSquared() > 0) {
          morphTarget.SetNewLocalNormal(finVertex,
                                        Vector3.Normalize(aseVertex.Normal));
        }
      }
    }

    // The file stores a total duration but no looping flag. Do not invent
    // timing or looping behavior when duration is absent.
    if (frameCount > 1 && aseMesh.AnimationDuration > 0) {
      var animationMetadata = fileBundle.AnimationMetadata?.Count > 0
                                  ? fileBundle.AnimationMetadata
                                  : [new AseAnimationMetadata(
                                      "vertex_animation",
                                      fileBundle.AseMeshFile.Name.ToString(),
                                      false, false, false, null)];
      foreach (var metadata in animationMetadata) {
        var animation = finModel.AnimationManager.AddAnimation();
        animation.Name = metadata.Name;
        animation.FrameCount = frameCount;
        animation.FrameRate = (frameCount - 1) / aseMesh.AnimationDuration;
        animation.UseLoopingInterpolation = metadata.Loop;
        animation.AnimationInterpolationMagFilter = metadata.NoInterpolation
            ? fin.animation.AnimationInterpolationMagFilter
                 .ORIGINAL_FRAME_RATE_NEAREST
            : fin.animation.AnimationInterpolationMagFilter
                 .ORIGINAL_FRAME_RATE_LINEAR;
        animation.SetMorphTargetFrames(metadata.Reverse
            ? morphTargetFrames.Reverse().ToArray()
            : morphTargetFrames);
      }
    }

    var trianglesByMaterialIndex
        = importedTriangles.ToListDictionary(entry => (
            entry.triangle.MainTextureIndex,
            entry.triangle.DecalTextureIndex,
            entry.triangle.LightmapIndex,
            entry.triangle.RenderFlags));

    var mainAndDecalAndLightmapIndexes = trianglesByMaterialIndex.Keys.OrderBy(k => k.MainTextureIndex)
                                       .ThenBy(k => k.DecalTextureIndex)
                                       .ThenBy(k => k.LightmapIndex)
                                       .ThenBy(k => k.RenderFlags);

    foreach (var mainAndDecalAndLightmapIndex in mainAndDecalAndLightmapIndexes) {
      var finMaterial = lazyMaterialMap[mainAndDecalAndLightmapIndex];

      var aseTriangles = trianglesByMaterialIndex[mainAndDecalAndLightmapIndex];
      var triangleVertices = aseTriangles.Select(entry => (
          entry.corners[0].vertex,
          entry.corners[1].vertex,
          entry.corners[2].vertex)).ToArray();

      finMesh.AddTriangles(triangleVertices)
             .SetMaterial(finMaterial);
    }


    return finModel;
  }
}
