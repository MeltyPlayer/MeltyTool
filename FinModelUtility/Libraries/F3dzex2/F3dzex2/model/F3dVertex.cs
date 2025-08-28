using System.Numerics;
using System.Runtime.InteropServices;

using CommunityToolkit.HighPerformance;

using schema.binary;

namespace f3dzex2.model;

public record struct F3dVertex : IBinaryDeserializable {
  public short X { get; set; }
  public short Y { get; set; }
  public short Z { get; set; }
  public Vector3 GetPosition() => new(this.X, this.Y, this.Z);

  public short Flag { get; set; }

  public short U { get; set; }
  public short V { get; set; }

  public Vector2 GetUv(float scaleX, float scaleY)
    => new(this.U * scaleX, this.V * scaleY);

  public byte NormalXOrR { get; set; }
  public byte NormalYOrG { get; set; }
  public byte NormalZOrB { get; set; }
  public byte A { get; set; }

  public Vector3 GetNormal() => new Vector3(
      GetNormalChannel_(this.NormalXOrR),
      GetNormalChannel_(this.NormalYOrG),
      GetNormalChannel_(this.NormalZOrB));

  private static float GetNormalChannel_(byte value)
    => ((sbyte) value) / (byte.MaxValue * .5f);

  public Vector4 GetColor()
    => new(this.NormalXOrR / 255f,
           this.NormalYOrG / 255f,
           this.NormalZOrB / 255f,
           this.A / 255f);

  public void Read(IBinaryReader br) {
    var bytes = MemoryMarshal.CreateSpan(ref this, 1).AsBytes();
    br.ReadUInt16s(bytes.Cast<byte, ushort>()[..6]);
    br.ReadBytes(bytes[(6 * 2)..]);
  }
}