using System.Drawing;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.color;
using fin.data;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.image;
using fin.io;
using fin.math;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.asserts;
using fin.util.enumerables;
using fin.util.sets;

using modl.schema.res.texr;
using modl.schema.terrain;
using modl.schema.terrain.bw1;

using schema.binary;

namespace modl.api;

public class OutModelImporter : IModelImporter<OutModelFileBundle> {
  public IModel Import(OutModelFileBundle modelFileBundle)
    => modelFileBundle.TextureDirectories != null
        ? this.ImportModel(modelFileBundle,
                           modelFileBundle.OutFile,
                           modelFileBundle.TextureDirectories
                                          .Select(dir => dir),
                           modelFileBundle.GameVersion,
                           out _)
        : this.ImportModel(modelFileBundle,
                           modelFileBundle.OutFile,
                           modelFileBundle.GameVersion,
                           out _);

  public IModel ImportModel(
      OutModelFileBundle modelFileBundle,
      IReadOnlyTreeFile outFile,
      GameVersion gameVersion,
      out IBwTerrain bwTerrain,
      float terrainLightScale = 1) {
    var outName = outFile.Name.Replace(".out.gz", "")
                         .Replace(".out", "");
    var outDirectory =
        outFile.AssertGetParent()
               .GetExistingSubdirs()
               .Single(dir => dir.Name.StartsWith(outName + "_L"));
    var allMapsDirectory = outDirectory.AssertGetParent();

    return this.ImportModel(
        modelFileBundle,
        outFile,
        outDirectory.Yield().Concat(allMapsDirectory),
        gameVersion,
        out bwTerrain,
        terrainLightScale);
  }

  public IModel ImportModel(
      OutModelFileBundle modelFileBundle,
      IReadOnlyGenericFile outFile,
      IEnumerable<IReadOnlyTreeDirectory> textureDirectoriesEnumerable,
      GameVersion gameVersion,
      out IBwTerrain bwTerrain,
      float terrainLightScale = 1) {
    var isBw2 = gameVersion == GameVersion.BW2;

    Stream stream;
    if (isBw2) {
      using var gZipStream =
          new GZipStream(outFile.OpenRead(),
                         CompressionMode.Decompress);

      stream = new MemoryStream();
      gZipStream.CopyTo(stream);
      stream.Position = 0;
    } else {
      stream = outFile.OpenRead();
    }

    using var br = new SchemaBinaryReader(stream, Endianness.LittleEndian);

    var terrain = bwTerrain =
        isBw2 ? br.ReadNew<Bw2Terrain>() : br.ReadNew<Bw1Terrain>();

    var files = outFile.AsSet();
    var finModel = new ModelImpl<OneColor2UvVertexImpl>(
        (index, position) => new OneColor2UvVertexImpl(index, position)) {
        FileBundle = modelFileBundle,
        Files = files
    };

    var textureDirectories = textureDirectoriesEnumerable.ToArray();
    var lazyImageDictionary = new LazyDictionary<string, IImage>(
        imageName => {
          if (imageName == "Dummy") {
            return FinImage.Create1x1FromColor(Color.Magenta);
          }

          var textureFile =
              textureDirectories
                  .SelectMany(
                      dir => dir.GetFilesWithNameRecursive(
                          $"{imageName}.texr"))
                  .FirstOrDefault();

          if (textureFile == null) {
            return FinImage.Create1x1FromColor(Color.Magenta);
          }

          files.Add(textureFile);
          var texr = isBw2
              ? (ITexr) textureFile.ReadNew<Gtxd>()
              : textureFile.ReadNew<Text>();
          return texr.Image;
        });

    var textureDictionary = new LazyDictionary<(int, string), ITexture?>(
        uvIndexAndTextureName => {
          var (uvIndex, textureName) = uvIndexAndTextureName;

          if (textureName == "Dummy") {
            return null;
          }

          var image = lazyImageDictionary[textureName];

          var finTexture =
              finModel.MaterialManager.CreateTexture(image);
          finTexture.Name = textureName;
          finTexture.UvIndex = uvIndex;

          // TODO: Need to handle wrapping
          finTexture.WrapModeU = WrapMode.REPEAT;
          finTexture.WrapModeV = WrapMode.REPEAT;

          return finTexture;
        });
    var materialDictionary = new LazyArray<IMaterial>(
        terrain.Materials.Count,
        matlIndex => {
          var matl = terrain.Materials[matlIndex];

          var texture1 = textureDictionary[(0, matl.Texture1)];
          var texture2 = textureDictionary[(1, matl.Texture2)];

          if (texture1 == null && texture2 == null) {
            return finModel.MaterialManager.AddNullMaterial();
          }

          var finMaterial =
              finModel.MaterialManager.AddFixedFunctionMaterial();
          finMaterial.Name = texture2 == null
              ? texture1.Name
              : $"{texture1.Name}/{texture2.Name}";

          finMaterial.SetTextureSource(0, Asserts.CastNonnull(texture1));
          finMaterial.SetTextureSource(1, Asserts.CastNonnull(texture2));

          var equations = finMaterial.Equations;
          var scalar1 = equations.CreateScalarConstant(1);

          var vertexColor0 = equations.CreateOrGetColorInput(
              FixedFunctionSource.VERTEX_COLOR_0);

          var textureColor1 = equations.CreateOrGetColorInput(
              FixedFunctionSource.TEXTURE_COLOR_0);
          var textureColor2 = equations.CreateOrGetColorInput(
              FixedFunctionSource.TEXTURE_COLOR_1);

          var vertexAlpha0 =
              equations.CreateOrGetScalarInput(
                  FixedFunctionSource.VERTEX_ALPHA_0);
          var inverseVertexAlpha0 = scalar1.Subtract(vertexAlpha0);

          var blendedTextureColor =
              textureColor1.Multiply(inverseVertexAlpha0)
                           .Add(textureColor2.Multiply(vertexAlpha0));

          equations.CreateColorOutput(
              FixedFunctionSource.OUTPUT_COLOR,
              vertexColor0.Multiply(blendedTextureColor));
          equations.CreateScalarOutput(FixedFunctionSource.OUTPUT_ALPHA,
                                       scalar1);

          return finMaterial;
        });

    var finSkin = finModel.Skin;
    var finMesh = finSkin.AddMesh();

    var bwHeightmap = terrain.Heightmap;
    var chunks = bwHeightmap.Chunks;

    var chunkCountX = chunks.Width;
    var chunkCountY = chunks.Height;

    var tileCountX = chunkCountX * 4;
    var tileCountY = chunkCountY * 4;

    var tileGrid = new Grid<IBwHeightmapTile?>(tileCountX, tileCountY);

    var heightmapSizeX = tileCountX * 4;
    var heightmapSizeY = tileCountY * 4;
    var chunkFinVertices = new Grid<IVertex?>(heightmapSizeX, heightmapSizeY);

    var trianglesByMaterial =
        new ListDictionary<uint, (IReadOnlyVertex, IReadOnlyVertex,
            IReadOnlyVertex)>();

    Span<(float, float)> surfaceTextureUvsInRow =
        stackalloc (float, float)[4];
    for (var chunkY = 0; chunkY < chunks.Height; ++chunkY) {
      for (var chunkX = 0; chunkX < chunks.Width; ++chunkX) {
        var tiles = chunks[chunkX, chunkY]?.Tiles;
        if (tiles == null) {
          continue;
        }

        for (var tileY = 0; tileY < tiles.Height; ++tileY) {
          for (var tileX = 0; tileX < tiles.Width; ++tileX) {
            var tile = tiles[tileX, tileY];
            tileGrid[4 * chunkX + tileX, 4 * chunkY + tileY] = tile;

            var surfaceTextureUvsFromFirstRow = tile.Schema
                                                    .SurfaceTextureUvsFromFirstRow
                                                    .Select(weirdUv => {
                                                      var u = LoadUOrV_(weirdUv.U);
                                                      var v = LoadUOrV_(weirdUv.V);
                                                      return (u, v);
                                                    })
                                                    .ToArray();

            var points = tile.Points;
            for (var pointY = 0; pointY < points.Height; ++pointY) {
              var oneThird = .33333f;
              var twoThirds = 2 * oneThird;
              var currentMultipleOfThird = pointY * oneThird;

              surfaceTextureUvsInRow[0] =
                  (Lerp_(surfaceTextureUvsFromFirstRow[0].u,
                         surfaceTextureUvsFromFirstRow[2].u,
                         currentMultipleOfThird),
                   Lerp_(surfaceTextureUvsFromFirstRow[0].v,
                         surfaceTextureUvsFromFirstRow[2].v,
                         currentMultipleOfThird));
              surfaceTextureUvsInRow[1] =
                  (Lerp_(
                       Lerp_(surfaceTextureUvsFromFirstRow[0].u,
                             surfaceTextureUvsFromFirstRow[1].u,
                             oneThird),
                       Lerp_(surfaceTextureUvsFromFirstRow[2].u,
                             surfaceTextureUvsFromFirstRow[3].u,
                             oneThird),
                       currentMultipleOfThird
                   ),
                   Lerp_(
                       Lerp_(surfaceTextureUvsFromFirstRow[0].v,
                             surfaceTextureUvsFromFirstRow[1].v,
                             oneThird),
                       Lerp_(surfaceTextureUvsFromFirstRow[2].v,
                             surfaceTextureUvsFromFirstRow[3].v,
                             oneThird),
                       currentMultipleOfThird
                   )
                  );
              surfaceTextureUvsInRow[2] =
                  (Lerp_(
                       Lerp_(surfaceTextureUvsFromFirstRow[0].u,
                             surfaceTextureUvsFromFirstRow[1].u,
                             twoThirds),
                       Lerp_(surfaceTextureUvsFromFirstRow[2].u,
                             surfaceTextureUvsFromFirstRow[3].u,
                             twoThirds),
                       currentMultipleOfThird
                   ),
                   Lerp_(
                       Lerp_(surfaceTextureUvsFromFirstRow[0].v,
                             surfaceTextureUvsFromFirstRow[1].v,
                             twoThirds),
                       Lerp_(surfaceTextureUvsFromFirstRow[2].v,
                             surfaceTextureUvsFromFirstRow[3].v,
                             twoThirds),
                       currentMultipleOfThird
                   )
                  );
              surfaceTextureUvsInRow[3] =
                  (Lerp_(surfaceTextureUvsFromFirstRow[1].u,
                         surfaceTextureUvsFromFirstRow[3].u,
                         currentMultipleOfThird),
                   Lerp_(surfaceTextureUvsFromFirstRow[1].v,
                         surfaceTextureUvsFromFirstRow[3].v,
                         currentMultipleOfThird));

              for (var pointX = 0; pointX < points.Width; ++pointX) {
                var point = points[pointX, pointY];

                var heightmapX = 16 * chunkX + 4 * tileX + pointX;
                var heightmapY = 16 * chunkY + 4 * tileY + pointY;

                var lightColor = point.LightColor;

                var scaledLightColor =
                    FinColor.FromRgbaFloats(
                        ApplyTerrainLightScale_(
                            lightColor.Rf,
                            terrainLightScale),
                        ApplyTerrainLightScale_(
                            lightColor.Gf,
                            terrainLightScale),
                        ApplyTerrainLightScale_(
                            lightColor.Bf,
                            terrainLightScale),
                        lightColor.Af);

                var detailTextureUvs =
                    tile.Schema.DetailTextureUvs[4 * pointY + pointX];

                var (u0, v0) = surfaceTextureUvsInRow[pointX];
                var uv0 = new Vector2(u0, v0);

                var u1 = LoadUOrV_(detailTextureUvs.U);
                var v1 = LoadUOrV_(detailTextureUvs.V);
                var uv1 = new Vector2(u1, v1);

                var finVertex =
                    finSkin.AddVertex(point.X, point.Height, point.Y);
                finVertex.SetColor(scaledLightColor);
                finVertex.SetUv(0, uv0);
                finVertex.SetUv(1, uv1);

                chunkFinVertices[heightmapX, heightmapY] = finVertex;
              }
            }

            for (var pointY = 0; pointY < points.Height - 1; ++pointY) {
              for (var pointX = 0; pointX < points.Width - 1; ++pointX) {
                var vX = 16 * chunkX + 4 * tileX + pointX;
                var vY = 16 * chunkY + 4 * tileY + pointY;

                var a = chunkFinVertices[vX, vY];
                var b = chunkFinVertices[vX + 1, vY];
                var c = chunkFinVertices[vX, vY + 1];
                var d = chunkFinVertices[vX + 1, vY + 1];

                if (a != null && b != null && c != null && d != null) {
                  trianglesByMaterial.Add(tile.MatlIndex, (a, b, c));
                  trianglesByMaterial.Add(tile.MatlIndex, (d, c, b));
                }
              }
            }
          }
        }
      }
    }

    foreach (var materialIndex in trianglesByMaterial.Keys.OrderBy(k => k)) {
      var material = materialDictionary[(int) materialIndex];
      var triangles = trianglesByMaterial[materialIndex];
      finMesh.AddTriangles(triangles.ToArray()).SetMaterial(material);
    }

    return finModel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ApplyTerrainLightScale_(
      float channel,
      float terrainLightScale)
    => channel * FinMath.MapRange(terrainLightScale, 0f, 1f, .5f, 1.5f);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Lerp_(float from, float to, float frac)
    => from + (to - from) * frac;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float LoadUOrV_(float value) => value / 4096;
}