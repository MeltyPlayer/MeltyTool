using System.Runtime.CompilerServices;

namespace fin.color;

public static class FinColorExtensions {
  extension<TColor>(TColor color) where TColor : IColor {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToBgraInt()
      => FinColor.MergeBgra(color.Rb, color.Gb, color.Bb, color.Ab);
  }
}