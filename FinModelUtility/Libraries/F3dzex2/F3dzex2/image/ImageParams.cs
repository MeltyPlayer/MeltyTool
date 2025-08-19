using fin.util.hash;

namespace f3dzex2.image;

public struct ImageParams {
  public ImageParams() { }

  public N64ColorFormat ColorFormat { get; set; } = N64ColorFormat.RGBA;
  public BitsPerTexel BitsPerTexel { get; set; } = BitsPerTexel._16BPT;

  public (ushort fullWidth, ushort uls, ushort ult)? LoadTileParams {
    get;
    set;
  }

  public ushort Width { get; set; }
  public ushort Height { get; set; }
  public uint SegmentedAddress { get; set; }

  public override int GetHashCode() => FluentHash.Start()
                                                 .With(this.ColorFormat)
                                                 .With(this.BitsPerTexel)
                                                 .With(this.Width)
                                                 .With(this.Height)
                                                 .With(this.SegmentedAddress);

  public bool IsInvalid => this.Width < 1 ||
                           this.Height < 1 ||
                           this.SegmentedAddress == 0;

  public override bool Equals(object? other) {
    if (ReferenceEquals(this, other)) {
      return true;
    }

    if (other is ImageParams otherImageParams) {
      return this.ColorFormat == otherImageParams.ColorFormat &&
             this.BitsPerTexel == otherImageParams.BitsPerTexel &&
             this.Width == otherImageParams.Width &&
             this.Height == otherImageParams.Height &&
             this.SegmentedAddress == otherImageParams.SegmentedAddress;
    }

    return false;
  }
}