using System.Runtime.CompilerServices;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/MeleeMedia/blob/master/MeleeMediaLib/Audio/GcAdpcmMath.cs
/// </summary>
public static class GcAdpcmMath {
  public static readonly int BytesPerFrame = 8;
  public static readonly int SamplesPerFrame = 14;
  public static readonly int NibblesPerFrame = 16;

  public static int NibbleCountToSampleCount(int nibbleCount) {
    int frames = nibbleCount / NibblesPerFrame;
    int extraNibbles = nibbleCount % NibblesPerFrame;
    int extraSamples = extraNibbles < 2 ? 0 : extraNibbles - 2;

    return SamplesPerFrame * frames + extraSamples;
  }

  public static int SampleCountToNibbleCount(int sampleCount) {
    int frames = sampleCount / SamplesPerFrame;
    int extraSamples = sampleCount % SamplesPerFrame;
    int extraNibbles = extraSamples == 0 ? 0 : extraSamples + 2;

    return NibblesPerFrame * frames + extraNibbles;
  }

  public static int NibbleToSample(int nibble) {
    int frames = nibble / NibblesPerFrame;
    int extraNibbles = nibble % NibblesPerFrame;
    int samples = SamplesPerFrame * frames;

    return samples + extraNibbles - 2;
  }

  public static int SampleToNibble(int sample) {
    int frames = sample / SamplesPerFrame;
    int extraSamples = sample % SamplesPerFrame;

    return NibblesPerFrame * frames + extraSamples + 2;
  }

  public static int SampleCountToByteCount(int sampleCount)
    => SampleCountToNibbleCount(sampleCount).DivideBy2RoundUp();

  public static int ByteCountToSampleCount(int byteCount)
    => NibbleCountToSampleCount(byteCount * 2);

  public static int DivideByRoundUp(this int value, int divisor)
    => (int) Math.Ceiling((double) value / divisor);

  public static int DivideBy2RoundUp(this int value)
    => (value / 2) + (value & 1);

  private static readonly sbyte[] SignedNibbles =
      [0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1];

  public static byte GetHighNibble(byte value) => (byte) ((value >> 4) & 0xF);
  public static byte GetLowNibble(byte value) => (byte) (value & 0xF);

  public static sbyte GetHighNibbleSigned(byte value)
    => SignedNibbles[(value >> 4) & 0xF];

  public static sbyte GetLowNibbleSigned(byte value)
    => SignedNibbles[value & 0xF];

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short Clamp16(int value) {
    if (value > short.MaxValue)
      return short.MaxValue;
    if (value < short.MinValue)
      return short.MinValue;
    return (short) value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static sbyte Clamp4(int value) {
    if (value > 7)
      return 7;
    if (value < -8)
      return -8;
    return (sbyte) value;
  }

  public static byte CombineNibbles(int high, int low)
    => (byte) ((high << 4) | (low & 0xF));
}