using System.Drawing;
using System.Numerics;

using CommunityToolkit.HighPerformance;

using DanDare.schema;

using fin.color;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using schema.binary;
using schema.binary.attributes;

namespace DanDare.api;

public sealed record A3dModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle<A3dModelFileBundle, A3dModelImporter>;

public sealed partial class A3dModelImporter
    : IModelImporter<A3dModelImporter, A3dModelFileBundle> {
  public IModel Import(A3dModelFileBundle fileBundle) {
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.MainFile.AsFileSet(),
    };

    using var fs = fileBundle.MainFile.OpenRead();
    var a3dScene = new A3dReader().Load(fs);

    var lazyFinTextures
        = new LazyDictionary<string, IReadOnlyTexture>(imageFileName => {
          using var ms
              = new MemoryStream(a3dScene.EmbeddedFiles[imageFileName]);
          var img = FinImage.FromStream(ms);

          var finTexture = finModel.MaterialManager.CreateTexture(img);
          finTexture.Name = imageFileName;

          finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;

          return finTexture;
        });

    var lazyFinMaterials
        = new LazyDictionary<(A3dMaterialNode?, A3dTextureNode?), IReadOnlyMaterial?>(tuple
                  => {
                var (a3dMaterialNode, a3dTextureNode) = tuple;
                if (a3dMaterialNode == null && a3dTextureNode == null) {
                  return null;
                }

                var color = new Vector4(
                        a3dMaterialNode?.Diffuse ?? Vector3.One,
                        (a3dMaterialNode?.Alpha ?? 1) *
                        (1 - (a3dMaterialNode?.Transparency ?? 0)))
                    .AsSystemColor();

                if (a3dTextureNode == null) {
                  var colorMaterial = finModel.MaterialManager.AddColorMaterial(color);
                  colorMaterial.Name = a3dMaterialNode!.Name;
                  return colorMaterial;
                }

                var textureMaterial
                    = finModel.MaterialManager.AddTextureMaterial(
                        lazyFinTextures[a3dTextureNode.Image]);
                textureMaterial.Name = a3dMaterialNode?.Name ?? a3dTextureNode.Name;
                textureMaterial.DiffuseColor = color;

                return textureMaterial;
              });

    if (a3dScene.Root != null) {
      var nodeQueue = new FinTuple2Queue<BA3dNode, IBone>(
          (a3dScene.Root, finModel.Skeleton.Root));
      while (nodeQueue.TryDequeue(out var a3dNode, out var parentFinBone)) {
        switch (a3dNode) {
          case A3dTransformNode a3dTransformNode: {
            var finBone = parentFinBone.AddChild(a3dTransformNode.Position);
            finBone.Transform
                   .SetScale(a3dTransformNode.Scale)
                   .SetRotation(a3dTransformNode.Rotation);

            nodeQueue.Enqueue(
                a3dTransformNode.Children.Select(c => (c, finBone)));
            break;
          }
          case A3dMeshNode a3dMeshNode: {
            var mesh = finModel.Skin.AddMesh();
            mesh.Name = a3dMeshNode.Name;

            var boneWeights
                = finModel.Skin.GetOrCreateBoneWeights(
                    VertexSpace.RELATIVE_TO_BONE,
                    parentFinBone);

            var a3dPositions = a3dMeshNode.Positions.Cast<float, Vector3>();
            var a3dNormals = a3dMeshNode.Normals.Cast<float, Vector3>();
            var a3dUvs = a3dMeshNode.Uvs.Cast<float, Vector2>();

            var triangleVertices = new List<IReadOnlyVertex>();

            if (a3dMeshNode.PositionIndices.Length == 0) {
              for (var i = 0; i < a3dPositions.Length; ++i) {
                var finVertex = finModel.Skin.AddVertex(a3dPositions[i]);

                if (a3dNormals.Length > 0) {
                  finVertex.SetLocalNormal(a3dNormals[i]);
                }

                if (a3dUvs.Length > 0) {
                  finVertex.SetUv(a3dUvs[i]);
                }

                finVertex.SetBoneWeights(boneWeights);

                triangleVertices.Add(finVertex);
              }
            } else {
              for (var i = 0; i < a3dMeshNode.PositionIndices.Length; ++i) {
                var a3dPositionIndex = a3dMeshNode.PositionIndices[i];
                if (a3dPositionIndex == -1) {
                  continue;
                }

                var finVertex
                    = finModel.Skin.AddVertex(a3dPositions[a3dPositionIndex]);

                if (a3dMeshNode.NormalIndices.Length > 0) {
                  finVertex.SetLocalNormal(a3dNormals[a3dMeshNode.NormalIndices[i]]);
                }

                if (a3dMeshNode.UvIndices.Length > 0) {
                  finVertex.SetUv(a3dUvs[a3dMeshNode.UvIndices[i]]);
                }

                finVertex.SetBoneWeights(boneWeights);

                triangleVertices.Add(finVertex);
              }
            }

            var triangles = mesh.AddTriangles(triangleVertices);
            triangles.SetMaterial(
                lazyFinMaterials[(a3dMeshNode.Material, a3dMeshNode.Material?.Texture ?? a3dMeshNode.Texture)]);
            triangles.SetVertexOrder(
                a3dMeshNode.Ccw
                    ? VertexOrder.COUNTER_CLOCKWISE
                    : VertexOrder.CLOCKWISE);

            break;
          }
          case A3dTimerNode or A3dCameraNode or A3dLightNode: {
            break;
          }
          case A3dRotationLinearNode or A3dVector3LinearNode: {
            break;
          }
          default: {
            break;
          }
        }
      }
    }

    return finModel;
  }

  [BinarySchema]
  private partial class FileHeader : IBinaryConvertible {
    [NullTerminatedString]
    public string Name { get; set; }

    public uint Size { get; set; }
  }
}