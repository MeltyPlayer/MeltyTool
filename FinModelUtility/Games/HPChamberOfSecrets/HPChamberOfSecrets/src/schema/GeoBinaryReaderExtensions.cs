using System.Numerics;

using schema.binary;

namespace hpcos.schema;

/// <summary>
///   Reader helpers for GEO/GDT files, which use self-relative pointers: an
///   int32 whose value is an offset from the position of the pointer itself.
/// </summary>
public static class GeoBinaryReaderExtensions {
  public static long ReadRelativePointer(this IBinaryReader br) {
    return br.Position + br.ReadInt32();
  }

  public static long ReadRelativePointerAt(this IBinaryReader br,
                                           long position) {
    br.Position = position;
    return br.ReadRelativePointer();
  }

  public static string ReadRelativeStringAt(this IBinaryReader br,
                                            long position) {
    long stringOffset = br.ReadRelativePointerAt(position);
    return br.SubreadAt(stringOffset, () => br.ReadStringNT());
  }

  public static int ReadInt32At(this IBinaryReader br, long position) {
    br.Position = position;
    return br.ReadInt32();
  }

  public static Vector3 ReadVector3(this IBinaryReader br)
    => new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

  /// <summary>
  ///   Reads consecutive records starting at the given offset. Each record's
  ///   Read() is expected to leave the reader at the start of the next one.
  /// </summary>
  public static T[] ReadNewsAt<T>(this IBinaryReader br,
                                  long offset,
                                  int count)
      where T : IBinaryDeserializable, new() {
    br.Position = offset;
    return br.ReadNews<T>(count);
  }
}
