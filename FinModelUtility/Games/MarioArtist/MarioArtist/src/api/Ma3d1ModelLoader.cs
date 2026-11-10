using System.Numerics;

using fin.image.util;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using marioartist.schema.polygon_studio;

public sealed record Ma3d1ModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle<Ma3d1ModelFileBundle, Ma3d1ModelLoader>;

public sealed class Ma3d1ModelLoader : IModelImporter<Ma3d1ModelLoader, Ma3d1ModelFileBundle> {
  public IModel Import(Ma3d1ModelFileBundle fileBundle) {
    var ma3d1 = fileBundle.MainFile.ReadNew<Ma3d1>();

    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.MainFile.AsFileSet(),
    };

    var finMaterialManager = finModel.MaterialManager;
    var finSkin = finModel.Skin;

    var ma3d1Meshes = ma3d1.MeshData.Meshes;
    for (var i = 0; i < ma3d1Meshes.Count; ++i) {
      var ma3d1Mesh = ma3d1Meshes[i];

      var (finMaterial, finTexture)
          = finMaterialManager.AddSimpleTextureMaterialFromImage(
              ma3d1Mesh.Texture.ToImage());
      if (finTexture.TransparencyType != TransparencyType.OPAQUE) {
        finMaterial.CullingMode = CullingMode.SHOW_BOTH;
      }

      finTexture.ThreePointFiltering = true;

      var finMesh = finSkin.AddMesh();

      var ma3d1Vertices = ma3d1Mesh.Vertices ?? [];

      var getFinVertex = (int indexInTriangle, int vertexI, int textureI) => {
        var ma3d1Vertex = ma3d1Vertices[vertexI];
        var finVertex = finSkin.AddVertex(ma3d1Vertex.X,
                                          ma3d1Vertex.Y,
                                          ma3d1Vertex.Z);

        var normal = Vector3.Normalize(
            new Vector3(ma3d1Vertex.NormalX,
                        ma3d1Vertex.NormalY,
                        ma3d1Vertex.NormalZ));
        finVertex.SetLocalNormal(normal);

        var heightScaling = ma3d1Mesh.TriangleDefinitionsSize / 28f;
        var uv = (textureI % 2) switch {
            0 => indexInTriangle switch {
                0 => (0.03125f, (textureI / 2 + 0.96875f) / heightScaling),
                1 => (0.90625f, (textureI / 2 + 0.96875f) / heightScaling),
                2 => (0.03125f, (textureI / 2 + 0.09375f) / heightScaling),
            },
            1 => indexInTriangle switch {
                0 => (0.96875f, (textureI / 2 + 0.03125f) / heightScaling),
                1 => (0.09375f, (textureI / 2 + 0.03125f) / heightScaling),
                2 => (0.96875f, (textureI / 2 + 0.90625f) / heightScaling),
            },
        };
        finVertex.SetUv(uv.Item1, uv.Item2);

        return (IReadOnlyVertex) finVertex;
      };

      var finTriangles
          = (ma3d1Mesh.Triangles ?? [])
            .Select((ma3d1Triangle, i) => {
              var v0 = ma3d1Triangle.VertexIndex0;
              var v1 = ma3d1Triangle.VertexIndex1;
              var v2 = ma3d1Triangle.VertexIndex2;

              return (getFinVertex(0, v0, i),
                      getFinVertex(1, v1, i),
                      getFinVertex(2, v2, i));
            })
            .ToArray();

      finMesh.AddTriangles(finTriangles)
             .SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE)
             .SetMaterial(finMaterial);
    }

    return finModel;
  }
}