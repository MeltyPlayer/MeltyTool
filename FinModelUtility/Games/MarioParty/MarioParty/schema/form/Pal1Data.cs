using fin.color;
using fin.image;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/master/packages/lib/models/FORM.ts#L540
/// </summary>
[BinarySchema]
public sealed partial class Pal1Data : IFormChunkData {
  public ushort GlobalIndex { get; set; }
  private ushort colorCount_;

  [Skip]
  public PixelFormat Format { get; set; }

  [Skip]
  public IColor[] Colors { get; private set; }

  [ReadLogic]
  public void Read_(IBinaryReader br) {
    this.Colors = new IColor[this.colorCount_];

    var remainingLength = br.Length - br.Position;
    var bpp = (uint) Math.Floor(1f * remainingLength / this.colorCount_);
    this.Format = bpp switch {
        1 => PixelFormat.L8,
        2 => PixelFormat.LA88,
        4 => PixelFormat.RGBA8888,
    };

    for (var i = 0; i < this.colorCount_; ++i) {
      switch (bpp) {
        case 1: {
          this.Colors[i] = FinColor.FromIntensityByte(br.ReadByte());
          break;
        }
        case 2: {
          var intensity = br.ReadByte();
          var alpha = br.ReadByte();

          this.Colors[i] = FinColor.FromRgbaBytes(
              intensity,
              intensity,
              intensity,
              alpha);
          break;
        }
        case 4: {
          this.Colors[i] = FinColor.FromRgbaBytes(
              br.ReadByte(),
              br.ReadByte(),
              br.ReadByte(),
              br.ReadByte());
          break;
        }
      }
    }
  }
}