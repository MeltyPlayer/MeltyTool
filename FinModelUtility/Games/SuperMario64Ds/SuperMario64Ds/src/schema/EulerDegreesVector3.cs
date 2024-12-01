using fin.math.xyz;

using schema.binary;

namespace sm64ds.schema;

public struct EulerDegreesVector3 : IBinaryConvertible, IXyz {
  public float X { get; set; }
  public float Y { get; set; }
  public float Z { get; set; }

  public void Read(IBinaryReader br) {
    this.X = ReadAngle_(br);
    this.Y = ReadAngle_(br);
    this.Z = ReadAngle_(br);
  }

  public void Write(IBinaryWriter bw) {
    WriteAngle_(bw, this.X);
    WriteAngle_(bw, this.Y);
    WriteAngle_(bw, this.Z);
  }

  private static float ReadAngle_(IBinaryReader br)
    => br.ReadInt16() * 90f / 0x0400;

  private static void WriteAngle_(IBinaryWriter bw, float value)
    => bw.WriteInt16((short) (value / 90 * 0x0400));
}