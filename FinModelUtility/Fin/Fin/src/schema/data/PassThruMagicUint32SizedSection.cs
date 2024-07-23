using fin.util.asserts;

using schema.binary;

namespace fin.schema.data;

public class PassThruMagicUInt32SizedSection<TMagic, TData>(
    IMagicConfig<TMagic, TData> config,
    TData data)
    : IMagicSection<TMagic, TData>
    where TMagic : notnull
    where TData : IBinaryConvertible {
  private readonly PassThruUInt32SizedSection<TData> impl_ = new(data);

  public TMagic Magic { get; } = config.GetMagic(data);
  public uint Size => this.impl_.Size;

  public int TweakReadSize {
    get => this.impl_.TweakReadSize;
    set => this.impl_.TweakReadSize = value;
  }


  public TData Data {
    get => this.impl_.Data;
    set => this.impl_.Data = value;
  }

  public void Read(IBinaryReader br) {
    var actualMagic = config.ReadMagic(br);
    Asserts.Equal(this.Magic, actualMagic);
    this.impl_.Read(br);
  }

  public void Write(IBinaryWriter bw) {
    config.WriteMagic(bw, this.Magic);
    this.impl_.Write(bw);
  }
}