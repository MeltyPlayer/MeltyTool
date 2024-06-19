using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.util.sets;

using schema.text.reader;

using xmod.schema.xmod;

using PrimitiveType = xmod.schema.xmod.PrimitiveType;

namespace xmod.api {
  public class XmodModelImporter : IModelImporter<XmodModelFileBundle> {
    public IModel Import(XmodModelFileBundle modelFileBundle) {
      using var tr = new SchemaTextReader(modelFileBundle.XmodFile.OpenRead());

      var xmod = new Xmod();
      xmod.Read(tr);

      var files = modelFileBundle.XmodFile.AsFileSet();
      var finModel = new ModelImpl {
          FileBundle = modelFileBundle,
          Files = files
      };

      var finMaterialManager = finModel.MaterialManager;

      var finSkin = finModel.Skin;
      var finMesh = finSkin.AddMesh();

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

          var finTexture = finMaterialManager.CreateTexture(image);
          finMaterial = finMaterialManager.AddTextureMaterial(finTexture);
        }

        for (var i = 0; i < material.NumPackets; ++i) {
          var packet = xmod.Packets[packetIndex];

          var packetVertices = packet.Adjuncts.Select(adjunct => {
                                       var position =
                                           xmod.Positions[
                                               adjunct.PositionIndex];
                                       var normal =
                                           xmod.Normals[adjunct.NormalIndex];
                                       var color =
                                           xmod.Colors[adjunct.ColorIndex];
                                       var uv1 = xmod.Uv1s[adjunct.Uv1Index];

                                       var vertex = finSkin.AddVertex(position);
                                       vertex.SetLocalNormal(normal);
                                       vertex.SetColor(color);
                                       vertex.SetUv(uv1);

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
                         .Select(vertexIndex => packetVertices[vertexIndex]);
            var finPrimitive = primitive.Type switch {
                PrimitiveType.TRIANGLE_STRIP => finMesh.AddTriangleStrip(
                    primitiveVertices.ToArray()),
                PrimitiveType.TRIANGLE_STRIP_REVERSED => finMesh
                    .AddTriangleStrip(
                        primitiveVertices.Reverse().ToArray()),
                PrimitiveType.TRIANGLES =>
                    finMesh.AddTriangles(primitiveVertices.ToArray()),
            };

            finPrimitive.SetMaterial(finMaterial);

            if (primitive.Type == PrimitiveType.TRIANGLES) {
              finPrimitive.SetVertexOrder(VertexOrder.NORMAL);
            }
          }

          ++packetIndex;
        }
      }

      return finModel;
    }
  }
}