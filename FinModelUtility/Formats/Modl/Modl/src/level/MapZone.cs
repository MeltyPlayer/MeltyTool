using System.Numerics;

namespace modl.level;

public enum ZoneType {
  MISSION_BOUNDARY
}

public class MapZone : BLevelObject {
  public ZoneType ZoneType { get; set; }
  public float Radius { get; set; }
  public Vector4 Size { get; set; }
  public uint Flags { get; set; }
  public Matrix4x4 Matrix { get; set; }
  public uint SystemFlags { get; set; }
}