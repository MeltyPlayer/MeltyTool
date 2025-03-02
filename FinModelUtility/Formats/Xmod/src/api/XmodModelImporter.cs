using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using xmod.schema.xmod;

using PrimitiveType = xmod.schema.xmod.PrimitiveType;


namespace xmod.api;

public class XmodModelImporter : IModelImporter<XmodModelFileBundle> {
  public IModel Import(XmodModelFileBundle modelFileBundle) {
    var files = modelFileBundle.XmodFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = files
    };

    this.ImportInto(modelFileBundle, finModel, files);

    return finModel;
  }

  public void ImportInto(
      XmodModelFileBundle modelFileBundle,
      ModelImpl finModel,
      ISet<IReadOnlyGenericFile> files) {
    var xmod = modelFileBundle.XmodFile.ReadNewFromText<Xmod>();

    var finMaterialManager = finModel.MaterialManager;

    var finSkin = finModel.Skin;
    var finMesh = finSkin.AddMesh();

    var finBones = finModel.Skeleton.Bones;

    var vertexBoneIndices = new List<int>(xmod.Positions.Count);
    for (var i = 0; i < xmod.Mtxv.Count; ++i) {
      for (var m = 0; m < xmod.Mtxv[i]; ++m) {
        vertexBoneIndices.Add(i);
      }
    }

    var packetIndex = 0;
    foreach (var material in xmod.Materials) {
      IMaterial finMaterial;

      var textureIds = material.TextureIds;
      if (textureIds.Count == 0) {
        finMaterial = finMaterialManager.AddNullMaterial();
      } else {
        var textureId = textureIds[0];
        var textureName = textureId.Name;

        var texFile =
            modelFileBundle.TextureDirectory.GetFilesWithNameRecursive(
                               $"{textureName}.tex")
                           .First();
        files.Add(texFile);
        var image = new TexImageReader().ReadImage(texFile);

        (finMaterial, _) = finMaterialManager
            .AddSimpleTextureMaterialFromImage(image, textureName);
      }

      for (var i = 0; i < material.NumPackets; ++i) {
        var packet = xmod.Packets[packetIndex];

        var packetVertices
            = packet.Adjuncts.Select(adjunct => {
                      var position = xmod.Positions[adjunct.PositionIndex];
                      var normal = xmod.Normals[adjunct.NormalIndex];
                      var color = xmod.Colors[adjunct.ColorIndex];
                      var uv1 = xmod.Uv1s[adjunct.Uv1Index];

                      var vertex = finSkin.AddVertex(position);
                      vertex.SetLocalNormal(normal);
                      vertex.SetColor(color);
                      vertex.SetUv(uv1);

                      if (finBones.Count > 1) {
                        /*var mappedMatrixIndex
                            = packet.MatrixTable[adjunct.MatrixIndex];*/
                        var mappedMatrixIndex
                            = vertexBoneIndices[adjunct.PositionIndex];

                        var finBone = finBones[1 + mappedMatrixIndex];
                        var boneWeights
                            = finSkin.GetOrCreateBoneWeights(
                                VertexSpace.RELATIVE_TO_BONE,
                                finBone);
                        vertex.SetBoneWeights(boneWeights);
                      }

                      return vertex;
                    })
                    .ToArray();

        foreach (var primitive in packet.Primitives) {
          var primitiveVertices =
              primitive.VertexIndices
                       .Skip(primitive.Type switch {
                           PrimitiveType.TRIANGLES => 0,
                           _                       => 1,
                       })
                       .Select(vertexIndex => packetVertices[vertexIndex])
                       .ToArray();
          var finPrimitive = primitive.Type switch {
              PrimitiveType.TRIANGLE_STRIP
                  => finMesh.AddTriangleStrip(primitiveVertices),
              PrimitiveType.TRIANGLE_STRIP_REVERSED
                  => finMesh.AddTriangleStrip(primitiveVertices),
              PrimitiveType.TRIANGLES
                  => finMesh.AddTriangles(primitiveVertices),
          };

          finPrimitive.SetMaterial(finMaterial);

          if (primitive.Type is PrimitiveType.TRIANGLES
                                or PrimitiveType.TRIANGLE_STRIP) {
            finPrimitive.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
          }
        }

        ++packetIndex;
      }
    }
  }
}