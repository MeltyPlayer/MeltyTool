using Newtonsoft.Json;

namespace sm64.JSON {
  [JsonObject]
  public sealed class ObjectData {
    public byte ModelId { get; set; }
    public uint Behaviour { get; set; }
    public byte[] BehaviourArgs { get; set; } = new byte[4];

    public bool AllActs { get; set; }
    public bool[] Acts { get; set; } = new bool[6];

    public short X { get; set; }
    public short Y { get; set; }
    public short Z { get; set; }

    public short RX { get; set; }
    public short RY { get; set; }
    public short RZ { get; set; }
  }
}