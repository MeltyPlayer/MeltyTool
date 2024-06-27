using fin.util.asserts;

using schema.binary;

namespace fin.schema.data;

public class PassThruMagicUInt32SizedSection<TMagic, TData>
    : IMagicSection<TMagic, TData>
    where TMagic : notnull
    where TData : IBinaryConvertible {
  private IMagicConfig<TMagic, TData> config_;
  private readonly PassThruUInt32SizedSection<TData> impl_;

  public PassThruMagicUInt32SizedSection(IMagicConfig<TMagic, TData> config,
                                         TData data) {
    this.config_ = config;
    this.impl_ = new PassThruUInt32SizedSection<TData>(data);
    this.Magic = config.GetMagic(data);
  }

  public TMagic Magic { get; }
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
    var actualMagic = this.config_.ReadMagic(br);
    Asserts.Equal(this.Magic, actualMagic);
    this.impl_.Read(br);
  }

  public void Write(IBinaryWriter bw) {
    this.config_.WriteMagic(bw, this.Magic);
    this.impl_.Write(bw);
  }
}