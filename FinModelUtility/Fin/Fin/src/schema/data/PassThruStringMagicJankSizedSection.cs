using schema.binary;
using schema.binary.attributes;

namespace fin.schema.data;

[BinarySchema]
public partial class PassThruStringMagicJankSizedSection<T>(
    string magic,
    T data) : IMagicSection<T>
    where T : IBinaryConvertible {
  private string MagicAsserter_ => this.Magic;

  [Skip]
  public string Magic { get; set; } = magic;

  private readonly PassThruJankSizedSection<T> impl_ = new(data);

  [Skip]
  public int TweakReadSize {
    get => this.impl_.TweakReadSize;
    set => this.impl_.TweakReadSize = value;
  }

  [Skip]
  public uint Size => this.impl_.Size;

  [Skip]
  public T Data {
    get => this.impl_.Data;
    set => this.impl_.Data = value;
  }
}