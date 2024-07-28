using fin.decompression;

namespace level5.decompression;

public class HuffmanArrayDecompressor(byte aType) : ISpanDecompressor {
  public bool TryToGetLength(ReadOnlySpan<byte> src, out int length) {
      DecompressionUtils.GetLengthAndType(src,
                                          out length,
                                          out var decompressionType);
      return (decompressionType == DecompressionType.HUFFMAN_ARRAY_24 &&
              aType == 0x24) ||
             (decompressionType == DecompressionType.HUFFMAN_ARRAY_28 &&
              aType == 0x28);
    }

  public bool TryToDecompressInto(ReadOnlySpan<byte> src, Span<byte> dst) {
      HuffStream instream = new HuffStream();

      instream.ReadInt32(src);

      var dstIndex = 0;

      int treeSize = instream.ReadByte(src);
      treeSize = (treeSize + 1) * 2;

      long treeEnd = (instream.p - 1) + treeSize;

      // the relative offset may be 4 more (when the initial decompressed size is 0), but
      // since it's relative that doesn't matter, especially when it only matters if
      // the given value is odd or even.
      HuffTreeNode rootNode =
          new HuffTreeNode(instream, src, false, 5, treeEnd);

      // re-position the stream after the tree (the stream is currently positioned after the root
      // node, which is located at the start of the tree definition)
      instream.p = (int) treeEnd;

      // the current u32 we are reading bits from.
      int data = 0;
      // the amount of bits left to read from <data>
      byte bitsLeft = 0;

      // a cache used for writing when the block size is four bits
      int cachedByte = -1;

      // the current output size
      HuffTreeNode currentNode = rootNode;

      while (instream.HasBytes(src) && dstIndex < dst.Length) {
        while (!currentNode.isData) {
          // if there are no bits left to read in the data, get a new byte from the input
          if (bitsLeft == 0) {
            data = instream.ReadInt32(src);
            bitsLeft = 32;
          }

          // get the next bit
          bitsLeft--;
          bool nextIsOne = (data & (1 << bitsLeft)) != 0;
          // go to the next node, the direction of the child depending on the value of the current/next bit
          currentNode = nextIsOne ? currentNode.child1 : currentNode.child0;
        }

        switch (aType) {
          case 0x28: {
            // just copy the data if the block size is a full byte
            //                        outstream.WriteByte(currentNode.Data);
            dst[dstIndex++] = currentNode.data;
            break;
          }
          case 0x24: {
            // cache the first half of the data if the block size is a half byte
            if (cachedByte < 0) {
              cachedByte = currentNode.data;
            } else {
              cachedByte |= currentNode.data << 4;
              dst[dstIndex++] = (byte) cachedByte;
              cachedByte = -1;
            }

            break;
          }
        }

        currentNode = rootNode;
      }

      return true;
    }

  private class HuffStream {
    public int p = 4;

    public bool HasBytes(ReadOnlySpan<byte> bytes) {
        return this.p < bytes.Length;
      }

    public int ReadByte(ReadOnlySpan<byte> bytes) {
        return bytes[this.p++] & 0xFF;
      }

    public int ReadThree(ReadOnlySpan<byte> bytes) {
        return ((bytes[this.p++] & 0xFF)) | ((bytes[this.p++] & 0xFF) << 8) |
               ((bytes[this.p++] & 0xFF) << 16);
      }

    public int ReadInt32(ReadOnlySpan<byte> bytes) {
        if (this.p >= bytes.Length)
          return 0;
        else
          return ((bytes[this.p++] & 0xFF)) | ((bytes[this.p++] & 0xFF) << 8) |
                 ((bytes[this.p++] & 0xFF) << 16) | ((bytes[this.p++] & 0xFF) << 24);
      }
  }

  private class HuffTreeNode {
    public byte data;
    public bool isData;
    public HuffTreeNode child0;
    public HuffTreeNode child1;

    public HuffTreeNode(HuffStream stream,
                        ReadOnlySpan<byte> bytes,
                        bool isData,
                        long relOffset,
                        long maxStreamPos) {
        if (stream.p >= maxStreamPos) {
          return;
        }

        int readData = stream.ReadByte(bytes);
        this.data = (byte) readData;

        this.isData = isData;

        if (!this.isData) {
          int offset = this.data & 0x3F;
          bool zeroIsData = (this.data & 0x80) > 0;
          bool oneIsData = (this.data & 0x40) > 0;

          long zeroRelOffset = (relOffset ^ (relOffset & 1)) + (offset * 2) + 2;

          int currStreamPos = stream.p;
          stream.p += (int) (zeroRelOffset - relOffset) - 1;
          this.child0 =
              new HuffTreeNode(stream,
                               bytes,
                               zeroIsData,
                               zeroRelOffset,
                               maxStreamPos);
          this.child1 =
              new HuffTreeNode(stream,
                               bytes,
                               oneIsData,
                               zeroRelOffset + 1,
                               maxStreamPos);

          stream.p = currStreamPos;
        }
      }
  }
}