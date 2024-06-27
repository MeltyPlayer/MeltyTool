namespace level5;

public class Decompress {
  /*
  public static byte[] YAY0(byte[] fileData) {
    DataReader code = new DataReader(new MemoryStream(fileData));
    DataReader count = new DataReader(new MemoryStream(fileData));
    DataReader data = new DataReader(new MemoryStream(fileData));
    code.BigEndian = true;
    count.BigEndian = true;
    data.BigEndian = true;

    code.Seek(0);
    Console.WriteLine(code.ReadString(4));
    code.Seek(4);
    int decompressedSize = code.ReadInt32();
    count.Seek(code.ReadUInt32());
    data.Seek(code.ReadUInt32());

    byte[] outputArray = new byte[decompressedSize];

    int outPosition = 0;
    uint validBitsCount = 0;
    byte currentCodeByte = 0;

    while (outPosition < decompressedSize) {
      if (validBitsCount <= 0) {
        currentCodeByte = (byte)code.ReadByte();
        validBitsCount = 8;
      }
      if ((currentCodeByte & 0x80) == 0x80) {
        outputArray[outPosition++] = (byte)data.ReadByte();
      } else {
        int c = count.ReadInt16();

        int distance = (c & 0xFFF);

        int startOffset = (outPosition - (distance + 1));

        int byteCount = ((c >> 12) & 0xF);

        if (byteCount == 0) {
          byteCount = (data.ReadByte() + 0x10);
        }

        // Take into consideration the two bytes for the count by adding two to the byte count.
        byteCount += 2;

        // Copy the run data.
        for (int j = 0; j < byteCount; j++) outputArray[outPosition++] = outputArray[startOffset++];
      }
      currentCodeByte <<= 1;
      validBitsCount--;
    }

    code.Dispose();
    data.Dispose();
    count.Dispose();

    return outputArray;
  }*/

  /*public static byte[] YAZ0(byte[] data) {
    DataReader f = new DataReader(new MemoryStream(data));

    f.BigEndian = true;
    f.Seek(4);
    int uncompressedSize = f.ReadInt32();
    f.Seek(0x10);

    byte[] src = f.ReadBytes(data.Length - 0x10);
    byte[] dst = new byte[uncompressedSize];

    int srcPlace = 0, dstPlace = 0; //current read/write positions

    uint validBitCount = 0; //number of valid bits left in "code" byte
    byte currCodeByte = 0;
    while (dstPlace < uncompressedSize) {
      //read new "code" byte if the current one is used up
      if (validBitCount == 0) {
        currCodeByte = src[srcPlace];
        ++srcPlace;
        validBitCount = 8;
      }

      if ((currCodeByte & 0x80) != 0) {
        //straight copy
        dst[dstPlace] = src[srcPlace];
        dstPlace++;
        srcPlace++;
      } else {
        //RLE part
        byte byte1 = src[srcPlace];
        byte byte2 = src[srcPlace + 1];
        srcPlace += 2;

        uint dist = (uint)(((byte1 & 0xF) << 8) | byte2);
        uint copySource = (uint)(dstPlace - (dist + 1));

        uint numBytes = (uint)(byte1 >> 4);
        if (numBytes == 0) {
          numBytes = (uint)(src[srcPlace] + 0x12);
          srcPlace++;
        } else
          numBytes += 2;

        //copy run
        for (int i = 0; i < numBytes; ++i) {
          dst[dstPlace] = dst[copySource];
          copySource++;
          dstPlace++;
        }
      }

      //use next bit from "code" byte
      currCodeByte <<= 1;
      validBitCount -= 1;
    }

    f.Dispose();

    return dst;
  }*/

  /*public static byte[] LZ11(byte[] instream, bool header = true) {
    int pointer = 0;

    if (header) {
      byte type = (byte)(instream[pointer++] & 0xFF);
      if (type != 0x11)
        throw new Exception("The provided stream is not a valid LZ-0x11 "
                + "compressed stream (invalid type 0x)");
      int decompressedSize = ((instream[pointer++] & 0xFF))
              | ((instream[pointer++] & 0xFF) << 8) | ((instream[pointer++] & 0xFF) << 16);

      if (decompressedSize == 0) {
        decompressedSize = ((instream[pointer++] & 0xFF))
                | ((instream[pointer++] & 0xFF) << 8) | ((instream[pointer++] & 0xFF) << 16)
                | ((instream[pointer++] & 0xFF) << 24);
        ;
      }
    }

    List<byte> outstream = new List<byte>();//[decompressedSize];
    int outpointer = 0;

    // the maximum 'DISP-1' is still 0xFFF.
    int bufferLength = 0x1000;
    int[] buffer = new int[bufferLength];
    int bufferOffset = 0;

    int currentOutSize = 0;
    int flags = 0, mask = 1;

    while (pointer < instream.Length) {
      // (throws when requested new flags byte is not available)
      // region Update the mask. If all flag bits have been read, get a
      // new set.
      // the current mask is the mask used in the previous run. So if it
      // masks the
      // last flag bit, get a new flags byte.
      if (mask == 1) {
        //				if (ReadBytes >= inLength)
        //					throw new Exception("Not enough data");
        flags = (instream[pointer++] & 0xFF);
        //				if (flags < 0)
        //					throw new Exception("Stream too short");
        mask = 0x80;
      } else {
        mask >>= 1;
      }

      // bit = 1 <=> compressed.
      if ((flags & mask) > 0) {
        // (throws when not enough bytes are available)
        // region Get length and displacement('disp') values from next
        // 2, 3 or 4 bytes

        // read the first byte first, which also signals the size of the
        // compressed block
        //				if (ReadBytes >= inLength)
        //					throw new Exception("Not enough data2");
        int byte1 = (instream[pointer++] & 0xFF);
        //				if (byte1 < 0)
        //					throw new Exception("Stream too short2");

        int length = byte1 >> 4;
        int disp = -1;

        if (length == 0) {
          // region case 0; 0(B C)(D EF) + (0x11)(0x1) = (LEN)(DISP)

          // case 0:
          // data = AB CD EF (with A=0)
          // LEN = ABC + 0x11 == BC + 0x11
          // DISP = DEF + 1

          // we need two more bytes available
          //					if (ReadBytes + 1 >= inLength)
          //						throw new Exception("Not enough data3");
          int byte2 = (instream[pointer++] & 0xFF);

          int byte3 = (instream[pointer++] & 0xFF);

          //					if (byte3 < 0)
          //						throw new Exception("Stream too short3");

          length = (((byte1 & 0x0F) << 4) | (byte2 >> 4)) + 0x11;
          disp = (((byte2 & 0x0F) << 8) | byte3) + 0x1;

          // endregion
        } else if (length == 1) {
          // region case 1: 1(B CD E)(F GH) + (0x111)(0x1) =
          // (LEN)(DISP)

          // case 1:
          // data = AB CD EF GH (with A=1)
          // LEN = BCDE + 0x111
          // DISP = FGH + 1

          // we need three more bytes available
          //					if (ReadBytes + 2 >= inLength)
          //						throw new Exception("Not enough data3");
          int byte2 = (instream[pointer++] & 0xFF);

          int byte3 = (instream[pointer++] & 0xFF);

          int byte4 = (instream[pointer++] & 0xFF);

          //					if (byte4 < 0)
          //						throw new Exception("Stream too short3");

          length = (((byte1 & 0x0F) << 12) | (byte2 << 4) | (byte3 >> 4)) + 0x111;
          disp = (((byte3 & 0x0F) << 8) | byte4) + 0x1;

          // endregion
        } else {
          // region case > 1: (A)(B CD) + (0x1)(0x1) = (LEN)(DISP)

          // case other:
          // data = AB CD
          // LEN = A + 1
          // DISP = BCD + 1

          // we need only one more byte available
          //					if (ReadBytes >= inLength)
          //						throw new Exception("Not enough data3");
          int byte2 = (instream[pointer++] & 0xFF);

          //					if (byte2 < 0)
          //						throw new Exception("Stream too short3");

          length = ((byte1 & 0xF0) >> 4) + 0x1;
          disp = (((byte1 & 0x0F) << 8) | byte2) + 0x1;

          // endregion
        }

        // endregion

        int bufIdx = bufferOffset + bufferLength - disp;
        for (int i = 0; i < length; i++) {
          int next = buffer[bufIdx % bufferLength];
          bufIdx++;
          outstream.Add((byte)(next & 0xFF));
          // outstream.WriteByte(next);
          buffer[bufferOffset] = next;
          bufferOffset = (bufferOffset + 1) % bufferLength;
        }
        currentOutSize += length;
      } else {
        // if (ReadBytes >= inLength)
        // throw new NotEnoughDataException(currentOutSize,
        // decompressedSize);
        int next = (instream[pointer++] & 0xFF);
        // if (next < 0)
        // throw new StreamTooShortException();

        outstream.Add((byte)(next & 0xFF));
        currentOutSize++;
        buffer[bufferOffset] = next;
        bufferOffset = (bufferOffset + 1) % bufferLength;
      }

    }
    return outstream.ToArray();

  }
  */

  /*private static int fbuf = 0;
  public static byte[] PRS_Mod(byte[] compData, int decompSize, int compSize) {
    fbuf = 0;
    return PRS_8ing(decompSize, compData, compSize);
  }

  private static int prs_8ing_get_bits(int n, byte[] sbuf, ref int sptr, ref int blen) {
    int retv;

    retv = 0;
    while (n != 0) {
      retv <<= 1;
      if (blen == 0) {
        fbuf = sbuf[sptr];
        //if(*sptr<256)
        //{ fprintf(stderr, "[%02x] ", fbuf&0xff); fflush(0); }
        sptr++;
        blen = 8;
      }

      if ((fbuf & 0x80) != 0) {
        retv |= 1;
      }

      fbuf <<= 1;
      blen--;
      n--;
    }

    return retv;
  }

  private static byte[] PRS_8ing(int dlen, byte[] sbuf, int slen) {
    byte[] dbuf = new byte[dlen];
    int sptr;
    int dptr;
    int i;
    int flag;
    int len;
    int pos;

    int blen = 0;

    sptr = 0;
    dptr = 0;
    while (sptr < slen) {
      flag = prs_8ing_get_bits(1, sbuf, ref sptr, ref blen);
      if (flag == 1) {
        //if(sptr<256)
        //{ fprintf(stderr, "%02x ", (u8)sbuf[sptr]); fflush(0); }
        if (dptr < dlen) {
          dbuf[dptr++] = sbuf[sptr++];
        }
      } else {
        flag = prs_8ing_get_bits(1, sbuf, ref sptr, ref blen);
        if (flag == 0) {
          len = prs_8ing_get_bits(2, sbuf, ref sptr, ref blen) + 2;
          pos = (int)(sbuf[sptr++] | 0xffffff00);
        } else {
          pos = (int)((sbuf[sptr++] << 8) | 0xffff0000);
          pos |= sbuf[sptr++] & 0xff;
          len = pos & 0x07;
          pos >>= 3;
          if (len == 0) {
            len = (sbuf[sptr++] & 0xff) + 1;
          } else {
            len += 2;
          }
        }
        //if(sptr<256)
        //{ fprintf(stderr, "<%08x(%08x): %08x %d> \n", dptr, dlen, pos, len); fflush(0); }
        pos += dptr;
        for (i = 0; i < len; i++) {
          if (dptr < dlen) {
            dbuf[dptr++] = dbuf[(uint)pos++];
          }
        }
      }
    }

    return dbuf;
  }



  public static byte[] SRD_Decomp(byte[] data) {
    List<byte> o = new List<byte>();

    using (DataReader r = new DataReader(data)) {
      r.BigEndian = true;

      if (r.ReadString(4) != "$CMP")
        throw new InvalidDataException();

      r.Seek(0x10);
      var decompSize = r.ReadInt32();
      var compSize = r.ReadInt32();
      r.Skip(0x10);

      while (true) {
        var cmp_mode = r.ReadString(4);
        if (!cmp_mode.StartsWith("$CL") && !cmp_mode.Equals("$CR0"))
          break;

        var chunk_dec_size = r.ReadInt32();
        var chunk_cmp_size = r.ReadInt32();
        r.ReadInt32();

        var chunk = r.ReadBytes(chunk_cmp_size - 0x10);

        if (!cmp_mode.Equals("$CR0"))
          chunk = SRC_DEC_CHUNK(chunk, cmp_mode);

        o.AddRange(chunk);
      }
    }

    return o.ToArray();
  }

  public static byte[] SRC_DEC_CHUNK(byte[] chunk, string cmp_mode) {
    List<byte> o = new List<byte>();

    using (DataReader r = new DataReader(chunk)) {
      r.BigEndian = true;


    }

    return o.ToArray();
  }*/
}