using schema.binary;

namespace fin.schema.data;

public interface ISwitchMagicConfig<TMagic, TData>
    : IMagicConfig<TMagic, TData>
    where TMagic : notnull
    where TData : IBinaryConvertible {
  TData CreateData(TMagic magic);
}

/// <summary>
///   Schema class that implements a uint32-sized section without needing to
///   worry about passing in an instance of the contained data. This should
///   be adequate for most cases, except when the data class needs to access
///   parent data.
/// </summary>
public class SwitchMagicUInt32SizedSection<TMagic, TData>
    : IMagicSection<TMagic, TData>
    where TMagic : notnull
    where TData : IBinaryConvertible {
  private readonly ISwitchMagicConfig<TMagic, TData> config_;
  private readonly PassThruUInt32SizedSection<TData> impl_ = new(default!);

  public SwitchMagicUInt32SizedSection(
      ISwitchMagicConfig<TMagic, TData> config) {
    this.config_ = config;
  }

  public int TweakReadSize {
    get => this.impl_.TweakReadSize;
    set => this.impl_.TweakReadSize = value;
  }

  public TMagic Magic { get; private set; }

  public TData Data => this.impl_.Data;

  public void Read(IBinaryReader br) {
    this.Magic = this.config_.ReadMagic(br);
    this.impl_.Data = this.config_.CreateData(this.Magic);
    this.impl_.Read(br);
  }

  public void Write(IBinaryWriter bw) {
    this.config_.WriteMagic(bw, this.Magic);
    this.impl_.Write(bw);
  }

  public override string ToString() => $"[{this.Magic}]: {this.Data}";
}