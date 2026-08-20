using System.Numerics;

using fin.color;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using hm64.schema;
using hm64.schema.mesh;

namespace hm64.api;

public sealed record Hm64MapModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/harvestwhisperer/hm64-decomp/blob/master/tools/modding/map/blender_import.py
/// </summary>
public sealed class Hm64MapModelImporter
    : IModelImporter<Hm64MapModelFileBundle> {
  public IModel Import(Hm64MapModelFileBundle fileBundle) {
    var model = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.MainFile.AsFileSet(),
    };

    var finSkin = model.Skin;

    var map = fileBundle.MainFile.ReadNew<Map>();

    var grid = map.Grid;
    for (var tileY = 0; tileY < grid.MapHeight; ++tileY) {
      for (var tileX = 0; tileX < grid.MapWidth; ++tileX) {
        var tileI = grid.TileIndices[tileY * grid.MapWidth + tileX].Value;
        if (tileI == 0) {
          continue;
        }

        var tile = map.Mesh.TileDefinitions[tileI - 1];

        var baseOffset = new Vector3(
            tileX * grid.TileSizeX,
            tile.YOffset,
            tileY * grid.TileSizeZ);

        var finMesh = finSkin.AddMesh();

        foreach (var face in tile.Faces) {
          var tileVertex0 = tile.Vertices[face.Vertices.Item1];
          var tileVertex1 = tile.Vertices[face.Vertices.Item2];
          var tileVertex2 = tile.Vertices[face.Vertices.Item3];

          var finVertex0
              = AddVertex_(finSkin, baseOffset, tileVertex0, face.Color);
          var finVertex1
              = AddVertex_(finSkin, baseOffset, tileVertex1, face.Color);
          var finVertex2
              = AddVertex_(finSkin, baseOffset, tileVertex2, face.Color);

          finMesh.AddTriangles(finVertex0, finVertex1, finVertex2);
        }
      }
    }

    return model;
  }

  private static IReadOnlyVertex AddVertex_(
      ISkin<NormalTangentMultiColorMultiUvVertexImpl> skin,
      Vector3 baseOffset,
      Vertex tileVertex,
      IColor color) {
    var finVertex = skin.AddVertex(
        baseOffset +
        new Vector3(tileVertex.X, tileVertex.Y, tileVertex.Z));

    finVertex.SetColor(color);

    return finVertex;
  }
}