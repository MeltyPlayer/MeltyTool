using System.Drawing;
using System.Numerics;

using f3dzex2.image;
using f3dzex2.io;

using fin.data.lazy;
using fin.io;
using fin.math;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using hm64.schema;
using hm64.schema.mesh;

using Microsoft.CodeAnalysis;

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
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.MainFile.AsFileSet(),
    };

    using var romBr
        = fileBundle.MainFile.OpenReadAsBinary(Endianness.BigEndian);

    var map = romBr.ReadNew<Map>();

    AddMap_(finModel, map, romBr);
    AddObjects_(finModel, map, romBr);

    return finModel;
  }

  private static void AddMap_(
      IModel<ISkin<NormalTangentMultiColorMultiUvVertexImpl>> finModel,
      Map map,
      IBinaryReader romBr) {
    var textures = ExtractTextures_(finModel.MaterialManager,
                                    romBr,
                                    map.Offsets.TileTexturesStart,
                                    map.Offsets.TilePalettesStart);

    var lazyMaterials = new LazyDictionary<int, IReadOnlyMaterial>(textureId
          => finModel.MaterialManager.AddTextureMaterial(textures[textureId]));

    var finSkin = finModel.Skin;
    var grid = map.Grid;
    for (var tileY = 0; tileY < grid.MapHeight; ++tileY) {
      for (var tileX = 0; tileX < grid.MapWidth; ++tileX) {
        var tileI = grid.TileIndices[tileY * grid.MapWidth + tileX].Value;
        if (tileI == 0) {
          continue;
        }

        var tile = map.Mesh.TileDefinitions[tileI - 1];

        var textureId = (tile.RawTexIndex & 0x7F);

        var texture = textures[textureId];
        var textureMaterial = lazyMaterials[textureId];

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

          if (face.TileUvs != null) {
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
                                      face.Color,
                                      isTextured ? tileUvs[i0] : null);
          var finVertex1 = AddVertex_(finSkin,
                                      baseOffset,
                                      tileVertex1,
                                      face.Color,
                                      isTextured ? tileUvs[i1] : null);
          var finVertex2 = AddVertex_(finSkin,
                                      baseOffset,
                                      tileVertex2,
                                      face.Color,
                                      isTextured ? tileUvs[i2] : null);

          var triangle
              = finMesh.AddTriangles(finVertex0, finVertex1, finVertex2);
          triangle.SetMaterial(isTextured ? textureMaterial : null);
        }
      }
    }
  }

  private static IReadOnlyVertex AddVertex_(
      ISkin<NormalTangentMultiColorMultiUvVertexImpl> skin,
      Vector3 baseOffset,
      Vertex tileVertex,
      Color color,
      Vector2? tileUv) {
    var finVertex = skin.AddVertex(
        baseOffset +
        new Vector3(tileVertex.X, tileVertex.Y, tileVertex.Z));

    finVertex.SetColor(color);

    if (tileUv != null) {
      finVertex.SetUv(tileUv);
    }

    return finVertex;
  }

  private static void AddObjects_(
      IModel<ISkin<NormalTangentMultiColorMultiUvVertexImpl>> finModel,
      Map map,
      IBinaryReader romBr) {
    var textures = ExtractTextures_(finModel.MaterialManager,
                                    romBr,
                                    map.Offsets.ObjectTexturesStart,
                                    map.Offsets.ObjectPalettesStart);

    // TODO: Hmmm... some sprites need to ignore depth?
    var lazyMaterials
        = new LazyDictionary<int, IReadOnlyTextureMaterial>(textureId
              => finModel.MaterialManager.AddTextureMaterial(
                  textures[textureId]));

    var tileOffset = new Vector3(map.Grid.TileSizeX, 0, map.Grid.TileSizeZ) / 2;
    var worldCenter = new Vector3(map.Grid.MapWidth, 0, map.Grid.MapHeight) *
                      tileOffset;

    var origin = worldCenter - tileOffset;

    var finSkin = finModel.Skin;
    var finMesh = finSkin.AddMesh();

    foreach (var group in map.Objects.Groups) {
      var spriteMaterial = lazyMaterials[group.SpriteIndex];

      var spriteImage = spriteMaterial.Texture.Image;
      var spriteWidthAndHeight
          = new Vector2(spriteImage.Width, spriteImage.Height);

      foreach (var instance in group.Instances) {
        var flags = instance.Flags;

        var scale = (flags & 0x0C) switch {
            0    => 1,
            0x04 => 2,
            0x08 => 4,
            _    => 8
        };

        var position = origin +
                       new Vector3(instance.X,
                                   instance.Y - scale * spriteImage.Height / 2f,
                                   instance.Z);
        var finBone = finModel.Skeleton.Root.AddChild(position);
        finBone.Transform.LocalScale = new Vector3(scale);

        var isBillboard = (flags & 0x80) == 0;
        if (isBillboard) {
          finBone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_AND_PITCH);

          AddSprite_(
              finModel,
              finBone,
              finMesh,
              spriteMaterial,
              spriteWidthAndHeight);
        } else {
          var rotationDegrees = (flags & 0x70) switch {
              0x70 => 45,
              0x60 => 90,
              0x50 => 135,
              0x40 => 180,
              0x30 => 225,
              0x20 => 270,
              0x10 => 315,
              _    => 0,
          };

          finBone.Transform.LocalEulerRadians
              = new Vector3(0, rotationDegrees * FinTrig.DEG_2_RAD, 0);

          AddSprite_(
              finModel,
              finBone,
              finMesh,
              spriteMaterial,
              spriteWidthAndHeight);
        }
      }
    }
  }

  private static void AddSprite_(
      IModel<ISkin<NormalTangentMultiColorMultiUvVertexImpl>> model,
      IReadOnlyBone bone,
      IMesh mesh,
      IReadOnlyMaterial material,
      Vector2 widthAndHeight) {
    var (width, height) = (widthAndHeight.X, widthAndHeight.Y);

    var ul = new Vector3(-width / 2, height, 0);
    var ur = new Vector3(width / 2, height, 0);
    var lr = new Vector3(width / 2, 0, 0);
    var ll = new Vector3(-width / 2, 0, 0);

    var skin = model.Skin;
    mesh.AddSimpleQuad(skin,
                       (ul, new Vector2(0, 0), null),
                       (ur, new Vector2(1, 0), null),
                       (lr, new Vector2(1, 1), null),
                       (ll, new Vector2(0, 1), null),
                       material,
                       bone);
  }

  private static IReadOnlyTexture[] ExtractTextures_(
      IMaterialManager finMaterialManager,
      IBinaryReader romBr,
      uint texturesStart,
      uint palettesStart) {
    romBr.Position = texturesStart;
    var textureCount = romBr.ReadUInt32() / 4;
    romBr.Position -= 4;
    var textureOffsets = romBr.ReadUInt32s(textureCount);
    while (textureOffsets[^1] == 0) {
      textureOffsets = textureOffsets[..^1];
    }

    romBr.Position = palettesStart;
    var paletteCount = romBr.ReadUInt32() / 4;
    romBr.Position -= 4;
    var paletteOffsets = romBr.ReadUInt32s(paletteCount).ToList();

    var textures = new IReadOnlyTexture[textureOffsets.Length - 1];

    for (var i = 0; i < textures.Length; ++i) {
      var textureStart = texturesStart + textureOffsets[i];
      var textureEnd = texturesStart + textureOffsets[i + 1];

      var paletteStart = palettesStart + paletteOffsets[i] + 4;
      var paletteEnd = palettesStart + paletteOffsets[i + 1];

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

      textures[i] = finMaterialManager.CreateTexture(image);
    }

    return textures;
  }
}