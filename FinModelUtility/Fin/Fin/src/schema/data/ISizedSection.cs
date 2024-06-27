using schema.binary;

namespace fin.schema.data;

public interface ISizedSection<out T> : IBinaryConvertible
    where T : IBinaryConvertible {
  T Data { get; }

  int TweakReadSize { get; set; }
}