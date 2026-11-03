using schema.binary;

namespace mp64.schema.mainfs;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/master/packages/lib/utils/compression.ts
/// </summary>
public static class DecompressionUtil {
  public const int WINDOW_START = 0x3be;
  public const int WINDOW_SIZE = 1024;

  public const int MIN_MATCH_LEN  = 3;

  /// <summary>
  ///   Mario Party N64 compression type 01
  ///
  ///   This type of compression was used in all three titles, though was the
  ///   exclusive type available in the original title. It is similar to LZSS,
  ///   and uses a 1KB circular buffer when referencing offsets. Like yaz0
  ///   compression, there are code bytes for every 8 "chunks," though they are
  ///   read from right to left (0x01 to 0x80).
  ///
  ///   When a '0' code bit is encountered, the next two bytes in the input are
  ///   read as offset and length pairs into the sliding window.
  ///
  ///         Byte 1    Byte 2
  ///       [oooooooo][oollllll]
  ///
  ///   The two offset bits within Byte 2 are masked and shifted such that they
  ///   become the most significant bits. The length is found by adding the
  ///   value of MIN_MATCH_LEN, in this case, 3.
  ///
  ///   Notably, the circular buffer begins filling not at zero, but at a
  ///   position roughly 90% of the way through, WINDOW_START.
  /// </summary>
  public static void Decompress01(IBinaryReader src, Span<byte> dst) {
    // Current positions in dst buffer and sliding window.
    var dstPlace = 0;
    var winPlace = WINDOW_START;

    // Create the sliding window buffer. Must be initialized to zero as some
    // files look into the window right away for starting zeros.
    Span<byte> winBuffer = stackalloc byte[WINDOW_SIZE];

    // The code byte's bits are interpreted to mean straight copy (1) or read
    // from sliding window (0).
    var curCodeByte = 0;
    while (dstPlace < dst.Length) {
      // Read a new "code" byte if the current one has expired. By placing an
      // 0xFF prior to the code bits, we can tell when we have consumed the
      // code byte by observing that the value will be become 0 only when we
      // have shifted away all of the code bits.
      if ((curCodeByte & 0x100) == 0) {
        curCodeByte = src.ReadByte();
        curCodeByte |= 0xff00;
      }

      if ((curCodeByte & 0x01) == 1) {
        // Copy the next byte from the source to the destination.
        var srcByte = src.ReadByte();
        dst[dstPlace] = srcByte;
        winBuffer[winPlace] = srcByte;
        ++dstPlace;
        winPlace = (winPlace + 1) % WINDOW_SIZE;
      } else {
        // Interpret the next two bytes as an offset into the sliding window and
        // a length to read.
        var byte1 = src.ReadByte();
        var byte2 = src.ReadByte();

        var offset = ((byte2 & 0xc0) << 2) | byte1;
        var count = (byte2 & 0x3f) + MIN_MATCH_LEN;

        // Within the sliding window, locate the offset and copy count bytes.
        for (var i = 0; i < count; ++i) {
          var val = winBuffer[offset % WINDOW_SIZE];
          winBuffer[winPlace] = val;
          winPlace = (winPlace + 1) % WINDOW_SIZE;
          dst[dstPlace] = val;
          ++dstPlace;
          ++offset;
        }
      }

      // Consider the code bit consumed.
      curCodeByte >>= 1;
    }
  }

  /// <summary>
  ///   Mario Party N64 compression type 02
  /// 
  ///   This compression algorithm was introduced in Mario Party 2, and is very
  ///   similar to yaz0. It uses word sized groups of code bits rather than a
  ///   single byte, and reads them in the same left to right order as yaz0.
  ///   The interpretation of the bytes when a code bit is 0 is also the same
  ///   as yaz0.
  /// 
  ///         Byte 1    Byte 2     [Byte 3 when l = 0]
  ///       [lllldddd][dddddddd]   [llllllll]
  /// 
  ///   The values of length and lookback distance are simply split apart and
  ///   used. The special case when length == 0 in yaz0 also applies, where a
  ///   third byte is read as the value of length and 0x12 is added. When the
  ///   lookback distance is farther than the start (out of bounds), 0s are
  ///   inserted.
  /// 
  ///   This is also the implementation used for decompression types 03 and 04.
  ///   03 and 04 are considered the same by the game engine.
  ///   02 differs from 03/04 in that it is the only implementation that checks
  ///   whether lookback goes out of bounds, and handles it. As far as I know,
  ///   it is OK to use that handling for 03/04 as well, since the behavior
  ///   would otherwise be undefined.
  /// </summary>
  public static void Decompress02(IBinaryReader src, Span<byte> dst) {
    // Current positions in dst buffer.
    var dstPlace = 0;

    // The code word's bits are interpreted to mean straight copy (1)
    // or lookback and read (0).
    uint curCodeWord = 0;
    var codeWordBitsRemaining = 0;
    while (dstPlace < dst.Length) {
      // Read a new code word if the current one has expired.
      if (codeWordBitsRemaining == 0) {
        curCodeWord = src.ReadUInt32();
        codeWordBitsRemaining = 32;
      }

      if ((curCodeWord & 0x80000000) != 0) {
        // Copy the next byte from the source to the destination.
        dst[dstPlace] = src.ReadByte();
        ++dstPlace;
      } else {
        // Interpret the next two bytes as a distance to travel backwards and a
        // a length to read.
        var byte1 = src.ReadByte();
        var byte2 = src.ReadByte();

        var back = (((byte1 & 0x0f) << 8) | byte2) + 1;
        var count = ((byte1 & 0xf0) >> 4) + 2;

        if (count == 2) {
          // Special case where 0xF0 masked byte 1 is zero.
          count = src.ReadByte() + 0x12;
        }

        // Step back and copy count bytes into the dst.
        for (var i = 0; i < count && dstPlace < dst.Length; ++i) {
          byte val;
          if (back > dstPlace) val = 0;
          else val = dst[dstPlace - back];

          dst[dstPlace] = val;
          ++dstPlace;
        }
      }

      // Consider the code bit consumed.
      curCodeWord <<= 1;
      --codeWordBitsRemaining;
    }
  }

  /// <summary>
  ///   Mario Party N64 compression type 05
  /// 
  ///   This is mostly a run-length encoding. The algorithm can be pretty
  ///   simply described:
  ///   1. Read a byte
  ///   2. If the byte has the sign bit (byte & 0x80), then get rid of the sign
  ///      bit and use that to count and copy in that many bytes.
  ///   3. If there is no sign bit, use the byte to count and repeat the next
  ///      byte for that many times.
  ///   4. Repeat until decompressed size reached.
  /// 
  ///   Typically this is used when there are a lot of zeroes or repeated
  ///   single values, like in some images.
  /// </summary>
  public static void Decompress05(IBinaryReader src, Span<byte> dst) {
    // Current positions in dst buffer.
    var dstPlace = 0;

    while (dstPlace < dst.Length) {
      var curCodeByte = src.ReadByte();

      var count = curCodeByte & 0x7f;
      if ((curCodeByte & 0x80) != 0) {
        // Having the sign bit means we read the next n bytes from the input.
        for (var i = 0; i < count; i++) {
          var nextByte = src.ReadByte();
          dst[dstPlace] = nextByte;
          dstPlace++;
        }
      } else {
        // No sign bit means we repeat the next byte n times.
        var repeatedByte = src.ReadByte();

        for (var i = 0; i < count; i++) {
          dst[dstPlace] = repeatedByte;
          dstPlace++;
        }
      }
    }
  }
}