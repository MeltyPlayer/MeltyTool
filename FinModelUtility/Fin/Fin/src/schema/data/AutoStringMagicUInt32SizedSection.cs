using schema.binary;
using schema.binary.attributes;

namespace fin.schema.data;

/// <summary>
///   Schema class that implements a uint32-sized section without needing to
///   worry about passing in an instance of the contained data. This should
///   be adequate for most cases, except when the data class needs to access
///   parent data.
/// </summary>
[BinarySchema]
public partial class AutoStringMagicUInt32SizedSection<T> : IMagicSection<T>
    where T : IBinaryConvertible, new() {
  private readonly PassThruStringMagicUInt32SizedSection<T> impl_;

  public AutoStringMagicUInt32SizedSection(string magic) {
    this.impl_ = new(magic, new T());
  }

  [Skip]
  public int TweakReadSize {
    get => this.impl_.TweakReadSize;
    set => this.impl_.TweakReadSize = value;
  }

  [Skip]
  public string Magic => this.impl_.Magic;

  [Skip]
  public uint Size => this.impl_.Size;

  [Skip]
  public T Data => this.impl_.Data;
}