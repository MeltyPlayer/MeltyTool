using schema.binary;
using schema.binary.attributes;

namespace fin.schema.data;

[BinarySchema]
public partial class AutoMagicUInt32SizedSection<TMagic, TData>
    : IMagicSection<TMagic, TData>
    where TMagic : notnull
    where TData : IBinaryConvertible {
  private readonly PassThruMagicUInt32SizedSection<TMagic, TData> impl_;

  public AutoMagicUInt32SizedSection(ISwitchMagicConfig<TMagic, TData> config,
                                     TMagic magic) {
    var data = config.CreateData(magic);
    this.impl_ = new(config, data);
  }

  [Skip]
  public int TweakReadSize {
    get => this.impl_.TweakReadSize;
    set => this.impl_.TweakReadSize = value;
  }

  [Skip]
  public TMagic Magic => this.impl_.Magic;

  [Skip]
  public uint Size => this.impl_.Size;

  [Skip]
  public TData Data => this.impl_.Data;
}