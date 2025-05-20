using CommunityToolkit.Diagnostics;

using fin.math;
using fin.picross;

using schema.binary;
using schema.binary.attributes;

namespace MariosPicross;

/// <summary>
///   Shamelessly stolen from:
///   https://www.zophar.net/fileuploads/3/21546xutra/picrossleveldata.txt
///   https://github.com/sopoforic/cgrr-mariospicross/blob/master/mariospicross.py
/// </summary>
[BinarySchema]
public partial class GameBoyPicrossDefinition 
    : IPicrossDefinition, IBinaryConvertible {
  [Skip]
  public string Name { get; set; }

  [SequenceLengthSource(15)]
  public ushort[] Rows { get; set; }

  private byte width_;
  private byte height_;

  [Skip]
  public int Width => this.width_;

  [Skip]
  public int Height => this.height_;

  public bool this[int x, int y] {
    get {
      Guard.IsLessThan(x, this.Width);
      Guard.IsLessThan(y, this.Height);
      return this.Rows[y].GetBit(15 - x);
    }
    set {
      Guard.IsLessThan(x, this.Width);
      Guard.IsLessThan(y, this.Height);
      this.Rows[y]
          = value ? this.Rows[y].SetBitTo1(x) : this.Rows[y].SetBitTo0(x);
    }
  }
}