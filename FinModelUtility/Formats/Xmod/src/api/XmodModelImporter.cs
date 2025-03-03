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

    this.ImportInto(modelFileBundle, finModel, files, null);

    return finModel;
  }

  public void ImportInto(
      XmodModelFileBundle modelFileBundle,
      ModelImpl finModel,
      ISet<IReadOnlyGenericFile> files,
      IReadOnlyDictionary<int, IReadOnlyBone>? finBoneById) {
    var xmod = modelFileBundle.XmodFile.ReadNewFromText<Xmod>();

    var finMaterialManager = finModel.MaterialManager;

    var finSkin = finModel.Skin;
    var finMesh = finSkin.AddMesh();

    var vertexBoneIndices = new List<int>(xmod.Positions.Count);
    for (var i = 0; i < xmod.Mtxv.Count; ++i) {
      for (var m = 0; m < xmod.Mtxv[i]; ++m) {
        vertexBoneIndices.Add(i);
      }
    }

    var packetIndex = 0;
    foreach (var xmodMaterial in xmod.Materials) {
      IMaterial finMaterial;

      IReadOnlyTreeFile? textureFile = null;
      var textureIds = xmodMaterial.TextureIds;
      if (textureIds.Count > 0) {
        var textureId = textureIds[0];
        var textureName = textureId.Name;
        textureFile = modelFileBundle
                      .TextureDirectory.GetFilesWithNameRecursive(
                          $"{textureName}.tex")
                      .FirstOrDefault();
      }

      if (textureFile == null) {
        finMaterial = finMaterialManager.AddNullMaterial();
      } else {
        files.Add(textureFile);
        var image = new TexImageReader().ReadImage(textureFile);

        (finMaterial, var finTexture) = finMaterialManager
            .AddSimpleTextureMaterialFromImage(
                image,
                textureFile.NameWithoutExtension.ToString());

        finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;

        // finMaterial.Shininess = xmodMaterial.Shininess;
      }

      for (var i = 0; i < xmodMaterial.NumPackets; ++i) {
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

                      if (finBoneById != null) {
                        var mappedMatrixIndex
                            = vertexBoneIndices[adjunct.PositionIndex];

                        var finBone = finBoneById[mappedMatrixIndex];
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

          finPrimitive.SetVertexOrder(
              primitive.Type is PrimitiveType.TRIANGLES
                                or PrimitiveType.TRIANGLE_STRIP
                  ? VertexOrder.CLOCKWISE
                  : VertexOrder.COUNTER_CLOCKWISE);
        }

        ++packetIndex;
      }
    }
  }
}