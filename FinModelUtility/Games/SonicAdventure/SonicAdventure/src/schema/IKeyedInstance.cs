using schema.binary;

namespace sonicadventure.schema;

public interface IKeyedInstance<out TThis> : IBinaryDeserializable
    where TThis : IKeyedInstance<TThis>, IBinaryDeserializable {
  static abstract TThis New(uint key);
}