using fin.data;
using fin.schema;
using fin.schema.color;

using schema.binary;
using schema.binary.attributes;

namespace modl.schema.terrain;

public record BwSection(string Name, int Size, long Offset);

[BinarySchema]
public partial class BwHeightmapMaterial : IBinaryConvertible {
  [StringLengthSource(16)]
  public string Texture1 { get; private set; }

  [StringLengthSource(16)]
  public string Texture2 { get; private set; }

  [Unknown]
  public uint[] Unknown { get; } = new uint[4];
}

public interface IBwTerrain {
  IBwHeightmap Heightmap { get; }
  IList<BwHeightmapMaterial> Materials { get; }
}

public interface IBwHeightmap {
  Grid<IBwHeightmapChunk?> Chunks { get; }
  float GetHeightAtPosition(float x, float y);
}

public interface IBwHeightmapChunk {
  Grid<IBwHeightmapTile> Tiles { get; }
}

public interface IBwHeightmapTile {
  Grid<IBwHeightmapPoint> Points { get; }
  uint MatlIndex { get; }

  HeightmapParser.SchemaTile Schema { get; }
}

public interface IBwHeightmapPoint {
  float X { get; }
  float Y { get; }
  float Height { get; }

  Rgba32 LightColor { get; }
}