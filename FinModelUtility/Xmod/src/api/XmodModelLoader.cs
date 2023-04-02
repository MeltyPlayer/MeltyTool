using fin.color;
using fin.io;
using fin.model;
using fin.model.impl;

using xmod.schema.xmod;

using PrimitiveType = xmod.schema.xmod.PrimitiveType;


namespace xmod.api {
  public class XmodModelFileBundle : IModelFileBundle {
    public required string GameName { get; init; }
    public IFileHierarchyFile MainFile => this.XmodFile;
    public required IFileHierarchyFile XmodFile { get; init; }
  }

  public class XmodModelLoader : IModelLoader<XmodModelFileBundle> {
    public IModel LoadModel(XmodModelFileBundle modelFileBundle) {
      using var tr =
          new FinTextReader(modelFileBundle.XmodFile.OpenRead());

      var xmod = new Xmod();
      xmod.Read(tr);

      var finModel = new ModelImpl();

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

          var texFile = modelFileBundle
                        .XmodFile.Root.Impl.GetSubdir("texture_x")
                        .SearchForFiles($"{textureName}.tex", true)
                        .Single();
          var image = new TexImageLoader().LoadImage(texFile);

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

                                       return finSkin
                                              .AddVertex(position)
                                              .SetLocalNormal(normal)
                                              .SetColor(color)
                                              .SetUv(uv1);
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