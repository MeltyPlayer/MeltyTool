using System.Drawing;
using System.Numerics;

using f3dzex2.image;
using f3dzex2.io;

using fin.color;
using fin.data.lazy;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using hm64.schema;
using hm64.schema.mesh;

using schema.binary;

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

    using var romBr
        = fileBundle.MainFile.OpenReadAsBinary(Endianness.BigEndian);

    var map = romBr.ReadNew<Map>();

    IReadOnlyTexture[] textures;
    {
      romBr.Position = map.Offsets.TileTexturesStart;
      var textureCount = romBr.ReadUInt32() / 4;
      var textureOffsets = romBr.ReadUInt32s(textureCount)[..^1];
      while (textureOffsets[^1] == 0) {
        textureOffsets = textureOffsets[..^1];
      }

      romBr.Position = map.Offsets.TilePalettesStart;
      var paletteCount = romBr.ReadUInt32() / 4;
      var paletteOffsets = romBr.ReadUInt32s(paletteCount)[..^1];

      textures = new IReadOnlyTexture[textureOffsets.Length - 1];

      for (var i = 0; i < textures.Length; ++i) {
        var textureStart = map.Offsets.TileTexturesStart + textureOffsets[i];
        var textureEnd = map.Offsets.TileTexturesStart + textureOffsets[i + 1];

        var paletteStart
            = map.Offsets.TilePalettesStart + paletteOffsets[i] + 4;
        var paletteEnd = map.Offsets.TilePalettesStart + paletteOffsets[i + 1];

        romBr.Position = textureStart + 3;
        var textureFlags = romBr.ReadByte();
        var imageFormat = textureFlags == 16
            ? N64ImageFormat.CI4
            : N64ImageFormat.CI8;
        romBr.PushMemberEndianness(Endianness.LittleEndian);
        var width = romBr.ReadUInt16();
        var height = romBr.ReadUInt16();
        romBr.PopEndianness();

        var textureData = romBr.ReadBytes(textureEnd - romBr.Position);

        romBr.Position = paletteStart;
        var paletteData = romBr.ReadBytes(paletteEnd - paletteStart);

        var n64Hardware = new N64Hardware<SeparateN64Memory>();
        n64Hardware.Memory = new();
        n64Hardware.Rdp = new Rdp {
            PaletteSegmentedAddress = 0,
        };

        n64Hardware.Memory.AddSegment(0, 0, paletteData);

        var image = new N64ImageParser(n64Hardware)
            .Parse(imageFormat, textureData, width, height);

        textures[i] = model.MaterialManager.CreateTexture(image);
      }
    }

    var lazyMaterials = new LazyDictionary<int, IReadOnlyMaterial>(textureId
          => model.MaterialManager.AddTextureMaterial(textures[textureId]));

    var grid = map.Grid;
    for (var tileY = 0; tileY < grid.MapHeight; ++tileY) {
      for (var tileX = 0; tileX < grid.MapWidth; ++tileX) {
        var tileI = grid.TileIndices[tileY * grid.MapWidth + tileX].Value;
        if (tileI == 0) {
          continue;
        }

        var tile = map.Mesh.TileDefinitions[tileI - 1];

        var textureId = (tile.RawTexIndex & 0x7F) - 1;
        var isValid = textureId >= 0 && textureId < textures.Length;

        var texture = isValid ? textures[textureId] : null;
        var textureMaterial = isValid ? lazyMaterials[textureId] : null;

        var baseOffset = new Vector3(
            tileX * grid.TileSizeX,
            tile.YOffset,
            tileY * grid.TileSizeZ);

        var finMesh = finSkin.AddMesh();

        var tileUvs = new Vector2[32];

        foreach (var face in tile.Faces) {
          var i0 = face.Vertices.Item1;
          var i1 = face.Vertices.Item2;
          var i2 = face.Vertices.Item3;

          if (isValid && face.TileUvs != null) {
            foreach (var tileUv in face.TileUvs) {
              tileUvs[tileUv.VertexIndex] = new Vector2(
                  tileUv.S * 2f / texture.Image.Width,
                  tileUv.T * 2f / texture.Image.Height);
            }
          }

          var tileVertex0 = tile.Vertices[i0];
          var tileVertex1 = tile.Vertices[i1];
          var tileVertex2 = tile.Vertices[i2];

          var isTextured = face.IsTextured;

          var finVertex0 = AddVertex_(finSkin,
                                      baseOffset,
                                      tileVertex0,
                                      isTextured ? tileUvs[i0] : null);
          var finVertex1 = AddVertex_(finSkin,
                                      baseOffset,
                                      tileVertex1,
                                      isTextured ? tileUvs[i1] : null);
          var finVertex2 = AddVertex_(finSkin,
                                      baseOffset,
                                      tileVertex2,
                                      isTextured ? tileUvs[i2] : null);

          var triangle = finMesh.AddTriangles(finVertex0, finVertex1, finVertex2);
          triangle.SetMaterial(isValid && isTextured ? textureMaterial : null);
        }
      }
    }

    return model;
  }

  private static IReadOnlyVertex AddVertex_(
      ISkin<NormalTangentMultiColorMultiUvVertexImpl> skin,
      Vector3 baseOffset,
      Vertex tileVertex,
      Vector2? tileUv) {
    var finVertex = skin.AddVertex(
        baseOffset +
        new Vector3(tileVertex.X, tileVertex.Y, tileVertex.Z));

    if (tileUv != null) {
      finVertex.SetUv(tileUv);
    }

    return finVertex;
  }
}