using fin.util.asserts;

using schema.binary;

using sonicadventure.schema;

namespace sonicadventure.util;

public static class KeyedPointerUtil {
  public static T ReadAtPointer<T>(this IBinaryReader br,
                                      uint keyedPointer,
                                      uint key)
      where T : IKeyedInstance<T>
    => br.ReadAtPointerOrNull<T>(keyedPointer, key).AssertNonnull();

  public static T? ReadAtPointerOrNull<T>(this IBinaryReader br,
                                          uint keyedPointer,
                                          uint key)
      where T : IKeyedInstance<T> {
    if (keyedPointer == 0) {
      return default;
    }

    var pointer = keyedPointer - key;
    if (pointer == 0) {
      return default;
    }

    var instance = T.New(keyedPointer, key);
    br.SubreadAt(pointer, () => instance.Read(br));

    return instance;
  }

  public static T? ReadAtPointerOrNull<T>(this IBinaryReader br,
                                          uint keyedPointer,
                                          uint key,
                                          Func<T> handler) {
    if (keyedPointer == 0) {
      return default;
    }

    var pointer = keyedPointer - key;
    if (pointer == 0) {
      return default;
    }

    return br.SubreadAt(pointer, handler);
  }
}