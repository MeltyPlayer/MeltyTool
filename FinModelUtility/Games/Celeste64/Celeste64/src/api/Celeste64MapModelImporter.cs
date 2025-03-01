using System.Numerics;
using System.Runtime.CompilerServices;

using Celeste64.map;

using fin.color;
using fin.data.lazy;
using fin.io;
using fin.math.rotations;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using SledgeFace = Sledge.Formats.Map.Objects.Face;

namespace Celeste64.api;

public class Celeste64MapModelFileBundle : IModelFileBundle {
  public required IReadOnlyTreeFile MapFile { get; init; }
  public required IReadOnlyTreeDirectory TextureDirectory { get; init; }

  public IReadOnlyTreeFile MainFile => this.MapFile;
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/EXOK/Celeste64/blob/bf7b209b7c56dad0e86a225f4d591ae3bccff455/Source/Data/Map.cs#L26
/// </summary>
public class Celeste64MapModelImporter
    : IModelImporter<Celeste64MapModelFileBundle> {
  public IModel Import(Celeste64MapModelFileBundle fileBundle) {
    using var s = fileBundle.MapFile.OpenRead();
    var celeste64Map = new Map(s);
    return this.Import(fileBundle, celeste64Map);
  }

  public IModel Import(Celeste64MapModelFileBundle fileBundle,
                       Map celeste64Map) {
    var fileSet = fileBundle.MapFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileSet,
    };

    var lazyMaterialAndTextureMap
        = new LazyCaseInvariantStringDictionary<(IMaterial, ITexture?)>(
            textureName => {
              if (textureName is "TB_empty" or "__TB_empty" or "invisible") {
                var hiddenMaterial
                    = finModel.MaterialManager.AddHiddenMaterial();
                hiddenMaterial.Name = textureName;
                return (hiddenMaterial, null);
              }

              if (fileBundle.TextureDirectory.TryToGetExistingFile(
                      $"{textureName}.png",
                      out var textureFile)) {
                var (textureMaterial, finTexture) = finModel.MaterialManager
                    .AddSimpleTextureMaterialFromFile(
                        textureFile);

                finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;
                finTexture.MinFilter = TextureMinFilter.NEAR;
                finTexture.MagFilter = TextureMagFilter.NEAR;

                return (textureMaterial, finTexture);
              }

              var nullMaterial = finModel.MaterialManager.AddNullMaterial();
              nullMaterial.Name = textureName;
              return (nullMaterial, null);
            });

    var finSkin = finModel.Skin;

    var finRootBone = finModel.Skeleton.Root.AddRoot(0, 0, 0);
    finRootBone.LocalTransform.SetRotationDegrees(-90, 180, 0);

    var finBoneWeights = finSkin.GetOrCreateBoneWeights(
        VertexSpace.RELATIVE_TO_BONE,
        finRootBone);

    foreach (var solid in celeste64Map.Solids) {
      var finMesh = finSkin.AddMesh();

      var color = FinColor.FromSystemColor(solid.Color);

      foreach (var face in solid.Faces) {
        var (finMaterial, finTexture)
            = lazyMaterialAndTextureMap[face.TextureName];

        var plane = face.Plane;
        CalculateRotatedUv_(face, out var rotatedUAxis, out var rotatedVAxis);

        var finVertices = face.Vertices.Select(position => {
                                var finVertex = finSkin.AddVertex(position);
                                finVertex.SetLocalNormal(plane.Normal);
                                finVertex.SetUv(CalculateUv_(face,
                                                  position,
                                                  finTexture,
                                                  rotatedUAxis,
                                                  rotatedVAxis));
                                finVertex.SetColor(color);
                                finVertex.SetBoneWeights(finBoneWeights);
                                return finVertex;
                              })
                              .ToArray();

        finMesh.AddTriangleFan(finVertices)
               .SetMaterial(finMaterial)
               .SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
      }
    }

    return finModel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void CalculateRotatedUv_(in SledgeFace face,
                                          out Vector3 rotatedUAxis,
                                          out Vector3 rotatedVAxis) {
    // Determine the dominant axis of the normal vector
    static Vector3 GetRotationAxis(Vector3 normal) {
      var abs = Vector3.Abs(normal);
      if (abs.X > abs.Y && abs.X > abs.Z) {
        return Vector3.UnitX;
      }

      if (abs.Y > abs.Z) {
        return Vector3.UnitY;
      }

      return Vector3.UnitZ;
    }

    // Apply scaling to the axes
    var scaledUAxis = face.UAxis / face.XScale;
    var scaledVAxis = face.VAxis / face.YScale;

    // Determine the rotation axis based on the face normal
    var rotationAxis = GetRotationAxis(face.Plane.Normal);
    var rotationMatrix
        = Matrix4x4.CreateFromAxisAngle(rotationAxis,
                                        face.Rotation * FinTrig.DEG_2_RAD);
    rotatedUAxis = Vector3.Transform(scaledUAxis, rotationMatrix);
    rotatedVAxis = Vector3.Transform(scaledVAxis, rotationMatrix);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Vector2 CalculateUv_(in SledgeFace face,
                                      in Vector3 vertex,
                                      IReadOnlyTexture? finTexture,
                                      in Vector3 rotatedUAxis,
                                      in Vector3 rotatedVAxis) {
    Vector2 uv;
    uv.X = vertex.X * rotatedUAxis.X +
           vertex.Y * rotatedUAxis.Y +
           vertex.Z * rotatedUAxis.Z;
    uv.Y = vertex.X * rotatedVAxis.X +
           vertex.Y * rotatedVAxis.Y +
           vertex.Z * rotatedVAxis.Z;
    uv.X += face.XShift;
    uv.Y += face.YShift;
    uv.X /= finTexture?.Image.Width ?? 1;
    uv.Y /= finTexture?.Image.Height ?? 1;
    return uv;
  }
}