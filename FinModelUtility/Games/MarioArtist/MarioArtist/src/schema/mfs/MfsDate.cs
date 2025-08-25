using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema.mfs;

[BinarySchema]
public partial class MfsDate : IBinaryDeserializable {
  private uint data_;

  [Skip]
  public uint Year => (this.data_ >> 25) + 1996;

  [Skip]
  public uint Month => (this.data_ >> 21) & 0xF;

  [Skip]
  public uint Day => (this.data_ >> 16) & 0x1F;

  [Skip]
  public uint Hour => (this.data_ >> 11) & 0x1F;

  [Skip]
  public uint Minute => (this.data_ >> 5) & 0x3F;

  [Skip]
  public uint Second => ((this.data_ >> 0) & 0x1F) * 2;

  public DateTime AsDateTime() => new((int) this.Year,
                                      (int) this.Month,
                                      (int) this.Day,
                                      (int) this.Hour,
                                      (int) this.Minute,
                                      (int) this.Second);

  public override string ToString() => this.AsDateTime().ToLongDateString();
}