using schema.binary;

namespace granny3d {
  public static class GrannyUtils {
    public static void SubreadRef(
        IBinaryReader br,
        Action<IBinaryReader> subread) {
      var offset = br.ReadUInt64();
      if (offset == 0) {
        return;
      }

      br.SubreadAt((long) offset, subread);
    }

    public static void SubreadRefToArray(
        IBinaryReader br,
        Action<IBinaryReader, uint> subread) {
      var count = br.ReadUInt32();
      var offset = br.ReadUInt64();
      if (offset == 0) {
        return;
      }

      br.SubreadAt((long)offset, ser => {
        subread(ser, count);
      });
    }
  }
}