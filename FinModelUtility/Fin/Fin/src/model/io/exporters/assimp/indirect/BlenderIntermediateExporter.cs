using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

using fin.data.indexable;
using fin.image;
using fin.image.util;
using fin.io;
using fin.model.accessor;
using fin.model.skeleton;
using fin.model.util;
using fin.util.enumerables;

namespace fin.model.io.exporters.assimp.indirect;

internal static class BlenderIntermediateExporter {
  public static ISystemFile ExportPackage(ISystemDirectory outputDirectory,
                                          IModelExporterParams modelExporterParams) {
    outputDirectory.Create();

    var texturesDirectory = new FinDirectory(Path.Combine(outputDirectory.FullPath,
                                                         "textures"));
    texturesDirectory.Create();

    var model = modelExporterParams.Model;
    var scale = modelExporterParams.Scale;

    var texturePathByTexture = ExportTextures_(model, texturesDirectory);
    var exportedBones = ExportBones_(model.Skeleton, scale);
    var boneNameByBone = exportedBones.ToDictionary(tuple => tuple.Bone,
                                                    tuple => tuple.Data.Name);

    var package = new BlenderIntermediatePackage {
        Name = modelExporterParams.OutputFile.NameWithoutExtension.ToString(),
        Bones = exportedBones.Select(tuple => tuple.Data).ToList(),
        Materials = ExportMaterials_(model, texturePathByTexture),
        Meshes = ExportMeshes_(model, scale),
        Animations = ExportAnimations_(model, scale, boneNameByBone),
    };

    var manifestFile = new FinFile(Path.Combine(outputDirectory.FullPath,
                                                "model.json"));
    var json = JsonSerializer.Serialize(
        package,
        new JsonSerializerOptions {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });
    manifestFile.WriteAllText(json);
    return manifestFile;
  }

  private static Dictionary<IReadOnlyTexture, string> ExportTextures_(
      IReadOnlyModel model,
      ISystemDirectory texturesDirectory) {
    var texturePathByTexture = new Dictionary<IReadOnlyTexture, string>();
    var textures = model.MaterialManager.All
                        .SelectMany(material => material.Textures)
                        .DistinctBy(texture => texture.ValidFileName)
                        .ToArray();

    for (var i = 0; i < textures.Length; ++i) {
      var texture = textures[i];
      var stem = Path.GetFileNameWithoutExtension(texture.ValidFileName);
      var fileName = $"{i:D4}_{SanitizeFileName_(stem)}.png";
      var relativePath = Path.Combine("textures", fileName).Replace('\\', '/');
      var file = new FinFile(Path.Combine(texturesDirectory.FullPath, fileName));

      using var stream = new MemoryStream();
      texture.Image.ExportToStream(stream, LocalImageFormat.PNG);
      file.WriteAllBytes(stream.ToArray());

      texturePathByTexture[texture] = relativePath;
    }

    return texturePathByTexture;
  }

  private static List<BlenderMaterialData> ExportMaterials_(
      IReadOnlyModel model,
      IReadOnlyDictionary<IReadOnlyTexture, string> texturePathByTexture) {
    var materials = new List<BlenderMaterialData>();

    foreach (var finMaterial in model.MaterialManager.All) {
      var material = new BlenderMaterialData {
          Name = finMaterial.Name ?? "material",
          DoubleSided = finMaterial.CullingMode switch {
              CullingMode.SHOW_FRONT_ONLY => false,
              CullingMode.SHOW_BACK_ONLY  => true,
              CullingMode.SHOW_BOTH       => true,
              CullingMode.SHOW_NEITHER    => false,
              _                           => false,
          },
      };

      var primaryTexture = PrimaryTextureFinder.GetFor(finMaterial);
      if (primaryTexture != null &&
          texturePathByTexture.TryGetValue(primaryTexture, out var primaryPath)) {
        material.PrimaryTexture = CreateTextureSlotData_(primaryTexture, primaryPath);
        material.AlphaMode = primaryTexture.TransparencyType.ToString();
      }

      IReadOnlyTexture? normalTexture = finMaterial switch {
          IStandardMaterial standardMaterial => standardMaterial.NormalTexture,
          IFixedFunctionMaterial fixedFunctionMaterial => fixedFunctionMaterial.NormalTexture,
          _ => null,
      };

      if (normalTexture != null &&
          texturePathByTexture.TryGetValue(normalTexture, out var normalPath)) {
        material.NormalTexture = CreateTextureSlotData_(normalTexture, normalPath);
      }

      materials.Add(material);
    }

    return materials;
  }

  private static BlenderTextureSlotData CreateTextureSlotData_(IReadOnlyTexture texture,
                                                               string relativePath)
    => new() {
        Path = relativePath,
        UvIndex = texture.UvIndex,
        WrapModeU = texture.WrapModeU.ToString(),
        WrapModeV = texture.WrapModeV.ToString(),
      };

  private static List<BlenderMeshData> ExportMeshes_(IReadOnlyModel model,
                                                     float scale) {
    var boneTransformManager = new BoneTransformManager();
    boneTransformManager.CalculateStaticMatricesForManualProjection(model);

    var vertexAccessor = MaximalVertexAccessor.GetAccessorForModel(model);
    var meshes = new List<BlenderMeshData>();

    foreach (var finMesh in model.Skin.Meshes) {
      var verticesInMesh = finMesh.Primitives
                                  .SelectMany(primitive => primitive.Vertices)
                                  .Distinct()
                                  .ToArray();
      var vertexIndexByVertex = verticesInMesh
                                .Select((vertex, index) => (vertex, index))
                                .ToDictionary(tuple => tuple.vertex,
                                              tuple => tuple.index);

      var meshData = new BlenderMeshData {
          Name = finMesh.Name,
          Vertices = verticesInMesh
                     .Select(finVertex => ExportVertex_(finVertex,
                                                       vertexAccessor,
                                                       boneTransformManager,
                                                       scale))
                     .ToList(),
      };

      var materialNames = new HashSet<string>();
      foreach (var primitive in finMesh.Primitives) {
        var materialName = primitive.Material?.Name ?? "null";
        materialNames.Add(materialName);

        switch (primitive.Type) {
          case PrimitiveType.TRIANGLES:
          case PrimitiveType.TRIANGLE_STRIP:
          case PrimitiveType.TRIANGLE_FAN: {
            foreach (var (v1, v2, v3) in primitive
                                         .GetOrderedTriangleVertices()
                                         .SeparateTriplets()) {
              meshData.Faces.Add(new BlenderFaceData {
                  Indices = [
                      vertexIndexByVertex[v1],
                      vertexIndexByVertex[v2],
                      vertexIndexByVertex[v3],
                  ],
                  MaterialName = materialName,
              });
            }

            break;
          }
          case PrimitiveType.QUADS: {
            var vertices = primitive.Vertices;
            for (var i = 0; i < vertices.Count; i += 4) {
              var v0 = vertexIndexByVertex[vertices[i + 0]];
              var v1 = vertexIndexByVertex[vertices[i + 1]];
              var v2 = vertexIndexByVertex[vertices[i + 2]];
              var v3 = vertexIndexByVertex[vertices[i + 3]];

              meshData.Faces.Add(new BlenderFaceData {
                  Indices = [v0, v1, v2],
                  MaterialName = materialName,
              });
              meshData.Faces.Add(new BlenderFaceData {
                  Indices = [v0, v2, v3],
                  MaterialName = materialName,
              });
            }

            break;
          }
          case PrimitiveType.QUAD_STRIP: {
            var vertices = primitive.Vertices;
            var firstVertex = 0;
            var secondVertex = 1;
            for (var v = 3; v < vertices.Count; v += 2) {
              var a = firstVertex;
              var b = secondVertex;
              var c = v - 1;
              var d = v;

              meshData.Faces.Add(new BlenderFaceData {
                  Indices = [
                      vertexIndexByVertex[vertices[a]],
                      vertexIndexByVertex[vertices[b]],
                      vertexIndexByVertex[vertices[d]],
                  ],
                  MaterialName = materialName,
              });
              meshData.Faces.Add(new BlenderFaceData {
                  Indices = [
                      vertexIndexByVertex[vertices[a]],
                      vertexIndexByVertex[vertices[d]],
                      vertexIndexByVertex[vertices[c]],
                  ],
                  MaterialName = materialName,
              });

              firstVertex = c;
              secondVertex = d;
            }

            break;
          }
          case PrimitiveType.POINTS:
            break;
          default: throw new NotSupportedException();
        }
      }

      meshData.MaterialNames = materialNames.ToList();
      meshes.Add(meshData);
    }

    return meshes;
  }

  private static BlenderVertexData ExportVertex_(IReadOnlyVertex finVertex,
                                                 IVertexAccessor vertexAccessor,
                                                 IReadOnlyBoneTransformManager boneTransformManager,
                                                 float scale) {
    vertexAccessor.Target(finVertex);

    boneTransformManager.ProjectVertexPositionNormalTangent(vertexAccessor,
                                                            out var outPosition,
                                                            out var outNormal,
                                                            out var outTangent);

    var vertex = new BlenderVertexData {
        Position = [outPosition.X * scale, outPosition.Y * scale, outPosition.Z * scale],
        BoneWeights = vertexAccessor.BoneWeights?.Weights
                                    .Select(weight => new BlenderBoneWeightData {
                                        BoneName = weight.Bone.Name,
                                        Weight = weight.Weight,
                                    })
                                    .ToList() ?? [],
    };

    if (vertexAccessor.LocalNormal != null) {
      vertex.Normal = [outNormal.X, outNormal.Y, outNormal.Z];
    }

    for (var i = 0; i < vertexAccessor.UvCount; ++i) {
      var uv = vertexAccessor.GetUv(i);
      if (uv != null) {
        vertex.Uvs.Add([uv.Value.X, uv.Value.Y]);
      }
    }

    return vertex;
  }

  private static List<BlenderAnimationData> ExportAnimations_(
      IReadOnlyModel model,
      float scale,
      IReadOnlyDictionary<IReadOnlyBone, string> boneNameByBone) {
    var animations = new List<BlenderAnimationData>();

    foreach (var animation in model.AnimationManager.Animations) {
      var animationData = new BlenderAnimationData {
          Name = animation.Name,
          FrameRate = animation.FrameRate,
          FrameCount = animation.FrameCount,
      };

      foreach (var (bone, boneName) in boneNameByBone) {
        if (!animation.BoneTracks.TryGetValue(bone, out var boneTracks)) {
          continue;
        }

        var boneAnimation = new BlenderBoneAnimationData {
            BoneName = boneName,
        };

        if (boneTracks.Translations?.HasAnyData ?? false) {
          var frames = new Vector3[animation.FrameCount];
          boneTracks.Translations.GetAllFrames(frames);
          boneAnimation.Translations = frames
                                       .Select(frame => new[] {
                                           frame.X * scale,
                                           frame.Y * scale,
                                           frame.Z * scale,
                                       })
                                       .ToList();
        }

        if (boneTracks.Rotations?.HasAnyData ?? false) {
          var frames = new Quaternion[animation.FrameCount];
          boneTracks.Rotations.GetAllFrames(frames);
          boneAnimation.Rotations = frames
                                    .Select(frame => new[] {
                                        frame.X,
                                        frame.Y,
                                        frame.Z,
                                        frame.W,
                                    })
                                    .ToList();
        }

        if (boneTracks.Scales?.HasAnyData ?? false) {
          var frames = new Vector3[animation.FrameCount];
          boneTracks.Scales.GetAllFrames(frames);
          boneAnimation.Scales = frames
                                 .Select(frame => new[] {
                                     frame.X,
                                     frame.Y,
                                     frame.Z,
                                 })
                                 .ToList();
        }

        if (boneAnimation.Translations != null ||
            boneAnimation.Rotations != null ||
            boneAnimation.Scales != null) {
          animationData.Bones.Add(boneAnimation);
        }
      }

      if (animationData.Bones.Count > 0) {
        animations.Add(animationData);
      }
    }

    return animations;
  }

  private static List<(IReadOnlyBone Bone, BlenderBoneData Data)> ExportBones_(
      IReadOnlySkeleton skeleton,
      float scale) {
    var bones = new List<(IReadOnlyBone, BlenderBoneData)>();
    foreach (var child in skeleton.Root.Children) {
      ExportBone_(child, null, scale, bones);
    }

    return bones;
  }

  private static void ExportBone_(IReadOnlyBone bone,
                                  string? parentName,
                                  float scale,
                                  IList<(IReadOnlyBone, BlenderBoneData)> bones) {
    Matrix4x4.Decompose(bone.Transform.LocalMatrix,
                        out var boneScale,
                        out var boneRotation,
                        out var boneTranslation);

    var length = bone.Children.Count > 0
        ? bone.Children.Select(child => child.Transform.LocalMatrix.Translation.Length())
                       .Where(value => value > 0)
                       .DefaultIfEmpty(0.05f)
                       .First()
        : 0.05f;

    bones.Add((bone, new BlenderBoneData {
        Name = bone.Name,
        ParentName = parentName,
        Translation = [
            boneTranslation.X * scale,
            boneTranslation.Y * scale,
            boneTranslation.Z * scale,
        ],
        Rotation = [
            boneRotation.X,
            boneRotation.Y,
            boneRotation.Z,
            boneRotation.W,
        ],
        Scale = [boneScale.X, boneScale.Y, boneScale.Z],
        Length = Math.Max(length * scale, 0.01f),
    }));

    foreach (var child in bone.Children) {
      ExportBone_(child, bone.Name, scale, bones);
    }
  }

  private static string SanitizeFileName_(string value) {
    var invalidChars = Path.GetInvalidFileNameChars();
    return string.Concat(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch));
  }

  private sealed class BlenderIntermediatePackage {
    public string Name { get; set; } = "model";
    public List<BlenderBoneData> Bones { get; set; } = [];
    public List<BlenderMaterialData> Materials { get; set; } = [];
    public List<BlenderMeshData> Meshes { get; set; } = [];
    public List<BlenderAnimationData> Animations { get; set; } = [];
  }

  private sealed class BlenderBoneData {
    public string Name { get; set; } = "";
    public string? ParentName { get; set; }
    public float[] Translation { get; set; } = [0, 0, 0];
    public float[] Rotation { get; set; } = [0, 0, 0, 1];
    public float[] Scale { get; set; } = [1, 1, 1];
    public float Length { get; set; } = 0.05f;
  }

  private sealed class BlenderTextureSlotData {
    public string Path { get; set; } = "";
    public int UvIndex { get; set; }
    public string WrapModeU { get; set; } = "";
    public string WrapModeV { get; set; } = "";
  }

  private sealed class BlenderMaterialData {
    public string Name { get; set; } = "";
    public bool DoubleSided { get; set; } = true;
    public string? AlphaMode { get; set; }
    public BlenderTextureSlotData? PrimaryTexture { get; set; }
    public BlenderTextureSlotData? NormalTexture { get; set; }
  }

  private sealed class BlenderMeshData {
    public string Name { get; set; } = "";
    public List<BlenderVertexData> Vertices { get; set; } = [];
    public List<BlenderFaceData> Faces { get; set; } = [];
    public List<string> MaterialNames { get; set; } = [];
  }

  private sealed class BlenderVertexData {
    public float[] Position { get; set; } = [0, 0, 0];
    public float[]? Normal { get; set; }
    public List<float[]> Uvs { get; set; } = [];
    public List<BlenderBoneWeightData> BoneWeights { get; set; } = [];
  }

  private sealed class BlenderBoneWeightData {
    public string BoneName { get; set; } = "";
    public float Weight { get; set; }
  }

  private sealed class BlenderFaceData {
    public int[] Indices { get; set; } = [];
    public string MaterialName { get; set; } = "";
  }

  private sealed class BlenderAnimationData {
    public string Name { get; set; } = "";
    public float FrameRate { get; set; }
    public int FrameCount { get; set; }
    public List<BlenderBoneAnimationData> Bones { get; set; } = [];
  }

  private sealed class BlenderBoneAnimationData {
    public string BoneName { get; set; } = "";
    public List<float[]>? Translations { get; set; }
    public List<float[]>? Rotations { get; set; }
    public List<float[]>? Scales { get; set; }
  }
}
