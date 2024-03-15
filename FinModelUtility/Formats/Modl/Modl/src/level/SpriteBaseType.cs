using System.Numerics;

using fin.color;

namespace modl.level {
  public class SpriteBaseType : BLevelObject {
    public TextureResource Texture { get; set; }
    public uint Properties { get; set; }
    public IColor Color { get; set; }
    public Vector2 TlUv { get; set; }
    public Vector2 BrUv { get; set; }
    public byte TotalFrames { get; set; }
    public float FrameTime { get; set; }
    public byte NumRows { get; set; }
    public byte NumColumns { get; set; }
    public byte StartLoop { get; set; }
    public byte EndLoop { get; set; }
  }
}
