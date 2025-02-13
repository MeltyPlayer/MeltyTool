using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HaloWarsTools;

public static class BinaryUtils {
  public static byte ReadByteLittleEndian(byte[] buffer, int startIndex) =>
      ReadByte(buffer, startIndex, BinaryEndianness.LittleEndian);

  public static byte ReadByte(byte[] buffer, int startIndex, BinaryEndianness endianness) {
    // I don't think this actually does anything since BinaryPrimitives.ReverseEndianness says it does nothing for bytes?
    byte value = buffer[startIndex];
    return IsAlreadyDesiredEndianness(endianness) ? value : BinaryPrimitives.ReverseEndianness(value);
  }

  public static short ReadInt16BigEndian(byte[] buffer, int startIndex) =>
      ReadInt16(buffer, startIndex, BinaryEndianness.BigEndian);

  public static short ReadInt16(byte[] buffer, int startIndex, BinaryEndianness endianness) {
    short value = BitConverter.ToInt16(buffer, startIndex);
    return IsAlreadyDesiredEndianness(endianness) ? value : BinaryPrimitives.ReverseEndianness(value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ReadInt32LittleEndian(byte[] buffer, int startIndex) =>
      ReadInt32(buffer, startIndex, BinaryEndianness.LittleEndian);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ReadInt32BigEndian(byte[] buffer, int startIndex) =>
      ReadInt32(buffer, startIndex, BinaryEndianness.BigEndian);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ReadInt32(byte[] buffer, int startIndex, BinaryEndianness endianness) {
    int value = BitConverter.ToInt32(buffer, startIndex);
    return IsAlreadyDesiredEndianness(endianness) ? value : BinaryPrimitives.ReverseEndianness(value);
  }

  public static long ReadInt64LittleEndian(byte[] buffer, int startIndex) =>
      ReadInt64(buffer, startIndex, BinaryEndianness.LittleEndian);

  public static long ReadInt64(byte[] buffer, int startIndex, BinaryEndianness endianness) {
    long value = BitConverter.ToInt64(buffer, startIndex);
    return IsAlreadyDesiredEndianness(endianness) ? value : BinaryPrimitives.ReverseEndianness(value);
  }

  public static ushort ReadUInt16LittleEndian(byte[] buffer, int startIndex) =>
      ReadUInt16(buffer, startIndex, BinaryEndianness.LittleEndian);

  public static ushort ReadUInt16BigEndian(byte[] buffer, int startIndex) =>
      ReadUInt16(buffer, startIndex, BinaryEndianness.BigEndian);

  public static ushort ReadUInt16(byte[] buffer, int startIndex, BinaryEndianness endianness) {
    ushort value = BitConverter.ToUInt16(buffer, startIndex);
    return IsAlreadyDesiredEndianness(endianness) ? value : BinaryPrimitives.ReverseEndianness(value);
  }

  public static uint ReadUInt32BigEndian(byte[] buffer, int startIndex) =>
      ReadUInt32(buffer, startIndex, BinaryEndianness.BigEndian);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ReadUInt32(byte[] buffer, int startIndex, BinaryEndianness endianness) {
    uint value = BitConverter.ToUInt32(buffer, startIndex);
    return IsAlreadyDesiredEndianness(endianness) ? value : BinaryPrimitives.ReverseEndianness(value);
  }

  public static ulong ReadUInt64BigEndian(byte[] buffer, int startIndex) =>
      ReadUInt64(buffer, startIndex, BinaryEndianness.BigEndian);

  public static ulong ReadUInt64(byte[] buffer, int startIndex, BinaryEndianness endianness) {
    ulong value = BitConverter.ToUInt64(buffer, startIndex);
    return IsAlreadyDesiredEndianness(endianness) ? value : BinaryPrimitives.ReverseEndianness(value);
  }

  public static float ReadFloatLittleEndian(byte[] buffer, int startIndex) =>
      ReadFloat(buffer, startIndex, BinaryEndianness.LittleEndian);

  public static float ReadFloatBigEndian(byte[] buffer, int startIndex) =>
      ReadFloat(buffer, startIndex, BinaryEndianness.BigEndian);

  public static float ReadFloat(byte[] buffer, int startIndex, BinaryEndianness endianness) {
    int value = BitConverter.ToInt32(buffer, startIndex);
    return BitConverter.Int32BitsToSingle(IsAlreadyDesiredEndianness(endianness)
                                              ? value
                                              : BinaryPrimitives.ReverseEndianness(value));
  }

  public static Vector3 ReadVector3BigEndian(byte[] buffer, int startIndex) =>
      ReadVector3(buffer, startIndex, BinaryEndianness.BigEndian);

  public static Vector3 ReadVector3(byte[] buffer, int offset, BinaryEndianness endianness) {
    return new Vector3(
        ReadFloat(buffer, offset, endianness),
        ReadFloat(buffer, offset + sizeof(float), endianness),
        ReadFloat(buffer, offset + sizeof(float) * 2, endianness)
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsAlreadyDesiredEndianness(BinaryEndianness endianness)
    => (endianness == BinaryEndianness.LittleEndian &&
        BitConverter.IsLittleEndian) ||
       (endianness == BinaryEndianness.BigEndian &&
        !BitConverter.IsLittleEndian);

  public static float ReadHalfLittleEndian(byte[] buffer, int offset)
    => (float) BitConverter.ToHalf(buffer, offset);
}

public enum BinaryEndianness {
  LittleEndian,
  BigEndian
}