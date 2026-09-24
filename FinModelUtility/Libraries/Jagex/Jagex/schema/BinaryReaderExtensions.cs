// Decompiled by Jad v1.5.8f. Copyright 2001 Pavel Kouznetsov.
// Jad home page: http://www.kpdus.com/jad.html
// Decompiler options: packimports(3) 

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier

using schema.binary;

namespace jagex.schema;

public static class BinaryReaderExtensions {
  extension(IBinaryReader br) {
    public int readUnsignedWord() => br.ReadUInt16();

    public int method421() {
      var position = br.Position;

      var i = br.ReadByte();
      if (i < 128) {
        return i - 64;
      } else {
        br.Position = position;
        return br.readUnsignedWord() - 49152;
      }
    }

    public int method422() {
      var position = br.Position;

      var i = br.ReadByte();
      if (i < 128) {
        return i;
      } else {
        return readUnsignedWord() - 32768;
      }
    }
  }
}