using System.Drawing;
using System.Numerics;

using EarClipperLib;

using fin.color;
using fin.data.counters;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.math;
using fin.math.matrix.four;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enumerables;
using fin.util.image;
using fin.util.sets;

using vrml.schema;

namespace vrml.api;

using IndexedFaceGroup = (int coordIndex, int? texCoordIndex, int? colorIndex);

public class VrmlModelImporter : IModelImporter<VrmlModelFileBundle> {
  public IModel Import(VrmlModelFileBundle fileBundle) {
    var wrlFile = fileBundle.WrlFile.Impl;
    using var wrlFileStream = wrlFile.OpenRead();

    var vrmlScene = VrmlParser.Parse(wrlFileStream);

    var fileSet = fileBundle.WrlFile.AsFileSet();

    var finModel = new ModelImpl { FileBundle = fileBundle, Files = fileSet };

    var lazyTextureDictionary
        = new LazyCaseInvariantStringDictionary<IReadOnlyTexture>(name => {
          var wrlDirectory = wrlFile.AssertGetParent();
          var imageFile = wrlDirectory.AssertGetExistingFile(name);
          fileSet.Add(imageFile);

          var finTexture
              = finModel.MaterialManager.CreateTexture(
                  FinImage.FromFile(imageFile));
          finTexture.Name = imageFile.NameWithoutExtension;
          finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;

          return finTexture;
        });
    var lazyMaterialDictionary = new LazyDictionary<IAppearanceNode, IMaterial>(
        appearanceNode => {
          var vrmlMaterial = appearanceNode.Material;
          IMaterial finMaterial;

          var color = vrmlMaterial.DiffuseColor;
          var alpha = vrmlMaterial.Transparency ?? 1;

          var r = (byte) (color.X * 255);
          var g = (byte) (color.Y * 255);
          var b = (byte) (color.Z * 255);
          var a = (byte) (alpha * 255);
          var vrmlColor = Color.FromArgb(a, r, g, b);

          var vrmlTexture = appearanceNode.Texture;
          if (vrmlTexture != null) {
            var finTexture = lazyTextureDictionary[vrmlTexture.Url];
            var finTextureMaterial
                = finModel.MaterialManager.AddTextureMaterial(finTexture);
            finTextureMaterial.DiffuseColor = vrmlColor;
            finTextureMaterial.Name = finTexture.Name;
            finMaterial = finTextureMaterial;
          } else {
            finMaterial = finModel.MaterialManager.AddColorMaterial(
                vrmlColor);
          }

          finMaterial.TransparencyType = a < 255
              ? TransparencyType.TRANSPARENT
              : TransparencyType.OPAQUE;

          return finMaterial;
        });

    var finSkeleton = finModel.Skeleton;
    var nodeQueue
        = new FinTuple2Queue<INode, IBone>(
            vrmlScene.Children.Select(n => (n, finSkeleton.Root)));
    while (nodeQueue.TryDequeue(out var vrmlNode, out var finParentBone)) {
      var finBone = finParentBone;

      switch (vrmlNode) {
        case IShapeNode shapeNode: {
          var geometry = shapeNode.Geometry;
          if (geometry is not IIndexedFaceSetNode indexedFaceSetNode) {
            break;
          }

          var finMaterial = lazyMaterialDictionary[shapeNode.Appearance];
          var finMesh = finModel.Skin.AddMesh();
          foreach (var faceVertices in GetIndexFaceSetCoordGroups_(
                       indexedFaceSetNode)) {
            var finVertices = new LinkedList<INormalVertex>();
            foreach (var vrmlVertex in faceVertices) {
              var (coordIndex, texCoordIndex, colorIndex) = vrmlVertex;

              var coord = indexedFaceSetNode.Coord.Point[coordIndex];
              var texCoord = texCoordIndex != null
                  ? indexedFaceSetNode.TexCoord?.Point[texCoordIndex.Value]
                  : null;
              var color = texCoordIndex != null
                  ? indexedFaceSetNode.Color?.Color[colorIndex.Value]
                  : null;

              var finVertex = finModel.Skin.AddVertex(coord);
              finVertex.SetUv(texCoord);

              if (color != null) {
                var finColor = FinColor.FromRgbFloats(color.Value.X,
                  color.Value.Y,
                  color.Value.Z);
                finVertex.SetColor(finColor);
              }

              finVertex.SetBoneWeights(
                  finModel.Skin.GetOrCreateBoneWeights(
                      VertexSpace.RELATIVE_TO_BONE,
                      finBone));

              finVertices.AddLast(finVertex);
            }

            var finVerticesArray = finVertices.ToArray();
            if (finVerticesArray.Length >= 3) {
              AddFaceNormal_(finVerticesArray);

              IPrimitive? finPrimitive = null;
              if (finVertices.Count == 3) {
                finPrimitive = finMesh.AddTriangles(finVerticesArray);
              } else if (finVertices.Count == 4) {
                finPrimitive = finMesh.AddQuads(finVerticesArray);
              } else {
                /*finPrimitive
                    = finMesh.AddTriangles(
                        TriangulateVertices_(finVertices.ToArray()));*/
              }

              finPrimitive?.SetVertexOrder(VertexOrder.NORMAL);
              finPrimitive?.SetMaterial(finMaterial);
            }
          }

          break;
        }
        case ITransformNode transformNode: {
          // TODO: How to handle scale orientation??
          /*finBone = finParentBone.AddChild(transformNode.Translation);
          finBone.LocalTransform.Rotation
              = transformNode.Rotation ?? Quaternion.Identity;
          finBone.LocalTransform.Scale = transformNode.Scale;*/

          // T × C × R × SR × S × -SR × -C
          var translation = transformNode.Translation;
          if (!translation.IsRoughly0()) {
            finBone = finBone.AddChild(transformNode.Translation);
          }

          var center = transformNode.Center;
          if (center != null && !center.Value.IsRoughly0()) {
            finBone = finBone.AddChild(center.Value);
          }

          var rotation = transformNode.Rotation;
          if (rotation != null && rotation != Quaternion.Identity) {
            finBone = finBone.AddChild(
                SystemMatrix4x4Util.FromRotation(rotation.Value));
          }

          var scaleOrientation = transformNode.ScaleOrientation;
          if (scaleOrientation != null &&
              scaleOrientation != Quaternion.Identity) {
            finBone = finBone.AddChild(
                SystemMatrix4x4Util.FromRotation(scaleOrientation.Value));
          }

          var scale = transformNode.Scale;
          if (scale != null && !scale.Value.IsRoughly1()) {
            finBone = finBone.AddChild(
                SystemMatrix4x4Util.FromScale(scale.Value));
          }

          if (scaleOrientation != null &&
              scaleOrientation != Quaternion.Identity) {
            finBone = finBone.AddChild(
                FinMatrix4x4Util.FromRotation(scaleOrientation.Value)
                                .InvertInPlace());
          }

          if (center != null && !center.Value.IsRoughly0()) {
            finBone = finBone.AddChild(-center.Value);
          }

          break;
        }
      }

      if (vrmlNode is IGroupNode groupNode) {
        nodeQueue.Enqueue(groupNode.Children.Select(n => (n, finBone)));
      }
    }

    return finModel;
  }

  private static IReadOnlyVertex[] TriangulateVertices_(
      IReadOnlyVertex[] finVertices) {
    var inputPoints = EarClipping.GetCoplanarMapping(
        finVertices.Select(v => new Vector3m(
                               v.LocalPosition.X,
                               v.LocalPosition.Y,
                               v.LocalPosition.Z))
                   .Reverse()
                   .ToList(),
        out var reverseMapping);

    var earClipping = new EarClipping();
    earClipping.SetPoints(inputPoints);
    earClipping.Triangulate();

    var result
        = EarClipping.RevertCoplanarityMapping(earClipping.Result,
                                               reverseMapping);

    return result.Select(v => FindClosest_(v, finVertices))
                 .ToArray();
  }

  private static IEnumerable<IndexedFaceGroup[]> GetIndexFaceSetCoordGroups_(
      IIndexedFaceSetNode indexedFaceSetNode) {
    var indexCounts = new CounterSet<int>();
    var list = new LinkedList<IndexedFaceGroup>();

    foreach (var indexedFaceSetGroup in GetIndexFaceSetCoords_(
                     indexedFaceSetNode)
                 .SplitByNull()) {
      if (indexedFaceSetGroup.Length == 0) {
        continue;
      }

      indexCounts.Clear();

      foreach (var tuple in indexedFaceSetGroup) {
        indexCounts.Increment(tuple.coordIndex);
      }

      if (!indexCounts.Any(kvp => kvp.Value > 1)) {
        yield return indexedFaceSetGroup;
        continue;
      }

      indexCounts.Clear();
      list.Clear();
      foreach (var tuple in indexedFaceSetGroup) {
        if (indexCounts.GetCount(tuple.coordIndex) == 1) {
          yield return list.ToArray();

          indexCounts.Clear();
          list.Clear();
          continue;
        }

        list.AddLast(tuple);
        indexCounts.Increment(tuple.coordIndex);
      }

      if (list.Count >= 3) {
        yield return list.ToArray();
      }
    }
  }

  private static IEnumerable<IndexedFaceGroup?> GetIndexFaceSetCoords_(
      IIndexedFaceSetNode indexedFaceSetNode) {
    var colorPerVertex = indexedFaceSetNode.ColorPerVertex ?? true;
    var coordIndex = indexedFaceSetNode.CoordIndex;
    var texCoordIndex = indexedFaceSetNode.TexCoordIndex;

    var primitiveIndex = 0;
    for (var i = 0; i < indexedFaceSetNode.CoordIndex.Count; ++i) {
      if (coordIndex[i] == -1) {
        primitiveIndex++;
        yield return null;
      } else {
        yield return (coordIndex[i],
                      texCoordIndex?[i],
                      !colorPerVertex ? primitiveIndex : null);
      }
    }
  }

  private static IReadOnlyVertex FindClosest_(
      Vector3m vec3,
      IReadOnlyVertex[] finVertices) {
    IReadOnlyVertex closestVertex = finVertices[0];
    var closestDistance = GetDistance_(vec3, closestVertex);
    foreach (var vertex in finVertices.Skip(1)) {
      var distance = GetDistance_(vec3, vertex);
      if (distance < closestDistance) {
        closestVertex = vertex;
        closestDistance = distance;
      }
    }

    return closestVertex;
  }

  private static float GetDistance_(Vector3m lhs, IReadOnlyVertex rhs)
    => MathF.Sqrt(MathF.Pow((float) lhs.X - rhs.LocalPosition.X, 2) +
                  MathF.Pow((float) lhs.Y - rhs.LocalPosition.Y, 2) +
                  MathF.Pow((float) lhs.Z - rhs.LocalPosition.Z, 2));
    
  private static void AddFaceNormal_(IReadOnlyList<INormalVertex> vertices) {
    var a = vertices[0].LocalPosition;
    var b = vertices[1].LocalPosition;
    var c = vertices[2].LocalPosition;

    var normal = Vector3.Cross(b - a, c - a);
    normal = Vector3.Normalize(normal);

    foreach (var vertex in vertices) {
      vertex.SetLocalNormal(normal);
    }
  }
}