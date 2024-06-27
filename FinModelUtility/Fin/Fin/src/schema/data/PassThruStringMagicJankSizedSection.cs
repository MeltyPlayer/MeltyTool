using schema.binary;
using schema.binary.attributes;

namespace fin.schema.data;

[BinarySchema]
public partial class PassThruStringMagicJankSizedSection<T>
    : IMagicSection<T> where T : IBinaryConvertible {
  private string MagicAsserter_ => this.Magic;

  [Skip]
  public string Magic { get; set; }

  private readonly PassThruJankSizedSection<T> impl_;

  public PassThruStringMagicJankSizedSection(string magic, T data) {
    this.Magic = magic;
    this.impl_ = new PassThruJankSizedSection<T>(data);
  }

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