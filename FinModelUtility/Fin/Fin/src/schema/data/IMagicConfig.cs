using schema.binary;

namespace fin.schema.data;

public interface IMagicConfig<TMagic, in TData>
    where TMagic : notnull
    where TData : IBinaryConvertible {
  TMagic ReadMagic(IBinaryReader br);
  void WriteMagic(IBinaryWriter bw, TMagic magic);

  TMagic GetMagic(TData data);
}