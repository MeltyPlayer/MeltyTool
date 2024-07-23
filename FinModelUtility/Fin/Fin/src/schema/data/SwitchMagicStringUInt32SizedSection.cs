using System;

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
public partial class SwitchMagicStringUInt32SizedSection<T>(
    int magicLength,
    Func<string, T> createTypeHandler)
    : IMagicSection<T>
    where T : IBinaryConvertible {
  [Skip]
  private readonly int magicLength_ = magicLength;

  [Skip]
  private readonly Func<string, T> createTypeHandler_ = createTypeHandler;

  private readonly PassThruStringMagicUInt32SizedSection<T> impl_ =
      new(default, default!);

  [Skip]
  public int TweakReadSize {
    get => this.impl_.TweakReadSize;
    set => this.impl_.TweakReadSize = value;
  }

  [Skip]
  public string Magic => this.impl_.Magic;

  [Skip]
  public T Data => this.impl_.Data;

  public void Read(IBinaryReader br) {
    var baseOffset = br.Position;

    var magic = br.ReadString(this.magicLength_);
    this.impl_.Magic = magic;
    this.impl_.Data = this.createTypeHandler_(magic);

    br.Position = baseOffset;
    this.impl_.Read(br);
  }
}