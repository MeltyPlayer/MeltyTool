using System.Numerics;

using schema.binary;

namespace hpcos.schema.geo;

/// <summary>
///   An instance of a mesh placed within a level.
/// </summary>
public sealed class GeoLayoutObject : IBinaryDeserializable {
  public const int SIZE = 0xa0;

  public Vector3 Position { get; private set; }
  public Vector3 RotationRadians { get; private set; }
  public Vector3 Scale { get; private set; }

  /// <summary>
  ///   Index of the mesh this object places, or -1 if it doesn't place one.
  /// </summary>
  public int MeshIndex { get; private set; }

  public void Read(IBinaryReader br) {
    long offset = br.Position;

    br.Position = offset + 0x04;
    this.Position = br.ReadVector3();

    br.Position = offset + 0x10;
    this.RotationRadians = br.ReadVector3();

    br.Position = offset + 0x30;
    this.Scale = br.ReadVector3();

    this.MeshIndex = br.ReadInt32At(offset + 0x48);

    br.Position = offset + SIZE;
  }
}
