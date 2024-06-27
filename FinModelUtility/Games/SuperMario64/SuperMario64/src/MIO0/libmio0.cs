namespace LIBMIO0 {
  struct MIO0_Header {
    public uint dest_size;
    public uint comp_offset;
    public uint uncomp_offset;
    public bool big_endian;
  };

  class MIO0 {
    private const int MIO0_HEADER_LENGTH = 16;

    private static int GET_BIT(byte[] buf, int offset, int bit) {
      return buf[(bit / 8) + offset] & (1 << (7 - (bit % 8)));
    }

    private static bool
        compareByteArrays(byte[] buf1, byte[] buf2, int length) {
      for (int i = 0; i < length; ++i)
        if (buf1[i] != buf2[i])
          return false;
      return true;
    }

    private static uint read_u32_be(byte[] buf, int off) {
      return (uint) (((buf)[off + 0] << 24) + ((buf)[off + 1] << 16) +
                     ((buf)[off + 2] << 8) + ((buf)[off + 3]));
    }

    private static uint read_u32_le(byte[] buf, int off) {
      return (uint) (((buf)[off + 1] << 24) + ((buf)[off + 0] << 16) +
                     ((buf)[off + 3] << 8) + ((buf)[off + 2]));
    }

    private static void write_u32_be(byte[] buf, uint val, int off) {
      buf[off + 0] = (byte) ((val >> 24) & 0xFF);
      buf[off + 1] = (byte) ((val >> 16) & 0xFF);
      buf[off + 2] = (byte) ((val >> 8) & 0xFF);
      buf[off + 3] = (byte) (val & 0xFF);
    }

    ///<summary>
    /// decode MIO0 header<para/>
    /// returns true if valid header, false otherwise
    ///</summary>
    public static bool decode_header(byte[] buf, ref MIO0_Header head) {
      byte[] mio0_ascii_be = [0x4D, 0x49, 0x4F, 0x30];
      byte[] mio0_ascii_le = [0x49, 0x4D, 0x30, 0x4F];

      if (compareByteArrays(buf, mio0_ascii_be, 4)) {
        head.dest_size = read_u32_be(buf, 4);
        head.comp_offset = read_u32_be(buf, 8);
        head.uncomp_offset = read_u32_be(buf, 12);
        head.big_endian = true;
        return true;
      } else if (compareByteArrays(buf, mio0_ascii_le, 4)) {
        head.dest_size = read_u32_le(buf, 4);
        head.comp_offset = read_u32_le(buf, 8);
        head.uncomp_offset = read_u32_le(buf, 12);
        head.big_endian = false;
        return true;
      }

      return false;
    }

    ///<summary>
    /// encode MIO0 header from struct
    ///</summary>
    public static void encode_header(byte[] buf, ref MIO0_Header head) {
      write_u32_be(buf, 0x4D494F30, 0); // write "MIO0" at start of buffer
      write_u32_be(buf, head.dest_size, 4);
      write_u32_be(buf, head.comp_offset, 8);
      write_u32_be(buf, head.uncomp_offset, 12);
    }

    ///<summary>
    /// decode MIO0 data<para/>
    /// mio0_buf: buffer containing MIO0 data<para/>
    /// returns the raw data as a byte array
    ///</summary>
    public static byte[]? mio0_decode(byte[] mio0_buf) {
      MIO0_Header head = new MIO0_Header();
      uint bytes_written = 0;
      int bit_idx = 0;
      int comp_idx = 0;
      int uncomp_idx = 0;
      bool valid;

      // extract header
      valid = decode_header(mio0_buf, ref head);
      // verify MIO0 header
      if (!valid) {
        Console.WriteLine("Error: MIO0 Header is not valid.");
        return null;
      }

      if (!head.big_endian) {
        Console.WriteLine("Error: Sorry, only big endian supported right now.");
        return null;
      }

      byte[] decoded = new byte[head.dest_size];

      //Console.WriteLine("Decoded Length: 0x"+decoded.Length.ToString("X"));

      // decode data
      while (bytes_written < head.dest_size) {
        if (GET_BIT(mio0_buf, MIO0_HEADER_LENGTH, bit_idx) > 0) {
          // 1 - pull uncompressed data
          decoded[bytes_written] = mio0_buf[head.uncomp_offset + uncomp_idx];
          bytes_written++;
          uncomp_idx++;
        } else {
          // 0 - read compressed data
          byte a = mio0_buf[head.comp_offset + comp_idx + 0];
          byte b = mio0_buf[head.comp_offset + comp_idx + 1];
          comp_idx += 2;
          int length = ((a & 0xF0) >> 4) + 3;
          int idx = ((a & 0x0F) << 8) + b + 1;
          for (int i = 0; i < length; i++) {
            decoded[bytes_written] = decoded[bytes_written - idx];
            bytes_written++;
          }
        }
        bit_idx++;
      }
      return decoded;
    }
  }
}