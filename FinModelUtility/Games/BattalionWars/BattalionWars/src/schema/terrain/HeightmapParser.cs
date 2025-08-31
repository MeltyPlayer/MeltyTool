using CommunityToolkit.HighPerformance;

using fin.data;
using fin.schema.color;

using schema.binary;
using schema.binary.attributes;

namespace modl.schema.terrain;

public partial class HeightmapParser : IBwHeightmap {
  // TODO: Write this in a more schema way instead

  private readonly TerrData terrData_;

  public const int tilesPerChunkAxis = 4;
  public const int tilesPerChunk = tilesPerChunkAxis * tilesPerChunkAxis;

  public const int pointsPerTileAxis = 4;
  public const int pointsPerTile = pointsPerTileAxis * pointsPerTileAxis;

  public const float chunkSize = 12;
  public const float overallScale = 5.25f;
  public const float xyScale = overallScale;
  public const float zScale = 1 / 64f * overallScale;

  public Grid<IBwHeightmapChunk?> Chunks { get; }

  public HeightmapParser(TerrData terrData,
                         byte[] tilemapBytes,
                         byte[] tilesBytes) {
    this.terrData_ = terrData;
    var chunkCountX = terrData.ChunkCountX;
    var chunkCountY = terrData.ChunkCountY;

    this.Chunks = new(chunkCountX, chunkCountY);

    SchemaTilemapDefinition[] tilemapDefinitions;
    {
      using var tilemapEr =
          new SchemaBinaryReader(new MemoryStream(tilemapBytes),
                                 Endianness.BigEndian);
      tilemapDefinitions =
          tilemapEr.ReadNews<SchemaTilemapDefinition>(
              chunkCountX * chunkCountY);
    }

    SchemaTile[] schemaTiles;
    {
      using var tilesEr =
          new SchemaBinaryReader(new MemoryStream(tilesBytes),
                                 Endianness.BigEndian);
      var schemaTileCount = tilesBytes.Length / 180;
      schemaTiles = tilesEr.ReadNews<SchemaTile>(schemaTileCount);
    }

    var totalWidth = chunkSize * chunkCountX;
    var totalHeight = chunkSize * chunkCountY;

    for (var chunkY = 0; chunkY < chunkCountY; ++chunkY) {
      for (var chunkX = 0; chunkX < chunkCountX; ++chunkX) {
        var tilemapDefinition =
            tilemapDefinitions[chunkY * chunkCountX + chunkX];
        if (tilemapDefinition.Unknown != 1) {
          continue;
        }

        var offset = tilemapDefinition.Offset;

        var chunk = new BwHeightmapChunk();
        this.Chunks[chunkX, chunkY] = chunk;

        for (var tileY = 0; tileY < tilesPerChunkAxis; ++tileY) {
          for (var tileX = 0; tileX < tilesPerChunkAxis; ++tileX) {
            var tile = new BwHeightmapTile();
            chunk.Tiles[tileX, tileY] = tile;

            var tileOffset = 4 * tileY + tileX;
            var schemaTile = schemaTiles[16 * offset + tileOffset];

            tile.Schema = schemaTile;
            tile.MatlIndex = schemaTile.MatlIndex;

            for (var pointY = 0; pointY < pointsPerTileAxis; ++pointY) {
              for (var pointX = 0; pointX < pointsPerTileAxis; ++pointX) {
                var point = new BwHeightmapPoint();

                GetWorldPosition(terrData.ChunkCountX, terrData.ChunkCountY, chunkX, chunkY, tileX, tileY, pointX, pointY, out var worldX, out var worldY);
                point.X = worldX;
                point.Y = worldY;

                var pointOffset = pointY * 4 + pointX;

                point.Height = zScale * schemaTile.Heights[pointOffset];
                point.LightColor = schemaTile.LightColors[pointOffset];

                tile.Points[pointX, pointY] = point;
              }
            }
          }
        }
      }
    }
  }

  public static void GetWorldPosition(
      int chunkCountX, int chunkCountY,
      int chunkX, int chunkY,
      int tileX, int tileY,
      int pointX, int pointY,
      out float worldX, out float worldY) {
    worldX = xyScale *
             (4 * 3 * chunkX + 3 * tileX + pointX -
              chunkSize * chunkCountX / 2);
    worldY = xyScale *
             (4 * 3 * chunkY + 3 * tileY + pointY -
              chunkSize * chunkCountY / 2);
  }

  public static void GetIndices(
      float worldX, float worldY,
      int chunkCountX, int chunkCountY,
      out int chunkX, out int chunkY,
      out int tileX, out int tileY,
      out int pointX, out int pointY) {
    var localX = (int)(worldX / xyScale + chunkSize * chunkCountX / 2);
    var localY = (int)(worldY / xyScale + chunkSize * chunkCountY / 2);

    pointX = localX % 3;
    pointY = localY % 3;

    tileX = ((localX - pointX) / 3) % 4;
    tileY = ((localY - pointY) / 3) % 4;

    chunkX = (localX - 3 * tileX - pointX) / 12;
    chunkY = (localY - 3 * tileY - pointY) / 12;
  }

  public float GetHeightAtPosition(float worldX, float worldY) {
    GetIndices(worldX, worldY,
               this.terrData_.ChunkCountX, this.terrData_.ChunkCountY, 
               out var chunkX, out var chunkY,
               out var tileX, out var tileY,
               out var pointX, out var pointY);

    try {
      return this.Chunks[chunkX, chunkY]?.Tiles[tileX, tileY].Points[pointX, pointY].Height ?? 0;
    } catch {
      return 0;
    }
  }

  [BinarySchema]
  private partial struct SchemaTilemapDefinition : IBinaryConvertible {
    private readonly byte padding_ = 0;

    public SchemaTilemapDefinition() {}

    public byte Unknown { get; private set; }

    public ushort Offset { get; private set; }
  }

  [BinarySchema]
  public partial class SchemaTile : IBinaryConvertible {
    public ushort[] Heights { get; } = new ushort[16];
    public Rgba32[] LightColors { get; } = new Rgba32[16];
    public TileUvs[] SurfaceTextureUvsFromFirstRow { get; } = new TileUvs[4];
    public TileUvs[] DetailTextureUvs { get; } = new TileUvs[16];
    public uint MatlIndex { get; private set; }

    public void Read(IBinaryReader br) {
      br.ReadUInt16s(this.Heights);
      br.ReadBytes(this.LightColors.AsSpan().Cast<Rgba32, byte>());
      br.ReadUInt16s(this.SurfaceTextureUvsFromFirstRow.AsSpan()
                         .Cast<TileUvs, ushort>());
      br.ReadUInt16s(this.DetailTextureUvs.AsSpan()
                         .Cast<TileUvs, ushort>());
      this.MatlIndex = br.ReadUInt32();
    }
  }

  [BinarySchema]
  public partial struct TileUvs : IBinaryConvertible {
    public ushort U { get; private set; }
    public ushort V { get; private set; }
  }

  private class BwHeightmapChunk : IBwHeightmapChunk {
    public Grid<IBwHeightmapTile> Tiles { get; } = new(4, 4);
  }

  private class BwHeightmapTile : IBwHeightmapTile {
    public Grid<IBwHeightmapPoint> Points { get; } = new(4, 4);
    public uint MatlIndex { get; set; }

    public SchemaTile Schema { get; set; }
  }

  private struct BwHeightmapPoint : IBwHeightmapPoint {
    public float X { get; set; }
    public float Y { get; set; }
    public float Height { get; set; }
    public Rgba32 LightColor { get; set; }
  }
}