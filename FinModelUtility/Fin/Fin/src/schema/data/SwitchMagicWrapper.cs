using System;
using System.Runtime.CompilerServices;

using schema.binary;

namespace fin.schema.data;

/// <summary>
///   Schema class that implements a uint32-sized section without needing to
///   worry about passing in an instance of the contained data. This should
///   be adequate for most cases, except when the data class needs to access
///   parent data.
/// </summary>
public class SwitchMagicWrapper<TMagic, TData> : IBinaryConvertible
    where TData : IBinaryConvertible {
  private readonly Func<IBinaryReader, TMagic> readMagicHandler_;
  private readonly Action<IBinaryWriter, TMagic> writeMagicHandler_;
  private readonly Func<TMagic, TData> createTypeHandler_;

  public SwitchMagicWrapper(
      Func<IBinaryReader, TMagic> readMagicHandler,
      Action<IBinaryWriter, TMagic> writeMagicHandler,
      Func<TMagic, TData> createTypeHandler) {
    this.readMagicHandler_ = readMagicHandler;
    this.writeMagicHandler_ = writeMagicHandler;
    this.createTypeHandler_ = createTypeHandler;
  }

  public TMagic Magic { get; private set; }

  public TData Data { get; private set; }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Read(IBinaryReader br) {
    this.Magic = this.readMagicHandler_(br);
    this.Data = this.createTypeHandler_(this.Magic);
    this.Data.Read(br);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Write(IBinaryWriter bw) {
    this.writeMagicHandler_(bw, this.Magic);
    this.Data.Write(bw);
  }

  public override string ToString() => $"[{this.Magic}]: {this.Data}";
}